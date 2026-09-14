# 04 — Moteur graphique commun (Canvas)

> Statut : **implémenté (Phase 2)**. Code : `src/ElectricalDesigner.Canvas/`.
> Ce document décrit le moteur tel qu'il existe réellement, pas une intention.

## Objectif (guide §Phase 2)

> "Construire un moteur 2D générique réutilisable par les deux éditeurs [schéma
> unifilaire et plan de position]."

`ElectricalDesigner.Canvas` est un projet **à zéro dépendance** — y compris
vers `ElectricalDesigner.Domain` (vérifiable dans son `.csproj`). C'est
l'application stricte de la règle des trois vérités (guide §1.3) : le
graphique ne peut pas, même accidentellement, muter une caractéristique
électrique, puisqu'il n'a tout simplement aucune visibilité sur le modèle
électrique.

## Ordre exact de construction (guide §6) et couverture Phase 2

| Étape | Contenu | Statut | Où |
|---|---|---|---|
| G01 | Système de coordonnées monde | ✅ | `Geometry/Point2D.cs` |
| G02 | Transformation monde ↔ écran | ✅ | `Geometry/WorldTransform.cs` |
| G03 | Zoom centré sur le curseur | ✅ | `WorldTransform.ZoomAt` |
| G04 | Panoramique | ✅ | `WorldTransform.PanByScreenDelta` |
| G05 | Objet graphique unique | ✅ | `Scene/CanvasObject.cs` |
| G06 | Sélection | ✅ | `Selection/SelectionManager.cs` |
| G07 | Déplacement | ✅ | `Commands/MoveObjectsCommand.cs` |
| G08 | Rotation | ✅ | `Commands/RotateObjectCommand.cs` |
| G09 | Redimensionnement | ✅ | `Commands/ResizeObjectCommand.cs` |
| G10 | Multi-sélection | ✅ | `SelectionManager` (`SelectRange`/`AddRange`) + `CanvasScene.QueryRect` |
| G11 | Grille et snapping | ✅ | `Scene/GridSettings.cs` |
| G12 | Copier/coller | ✅ | `Clipboard/CanvasClipboard.cs` |
| G13 | Undo/Redo | ✅ | `Commands/CommandHistory.cs` |
| G14 | Rendu vectoriel | ⏳ Phase 3 | nécessite un binding Avalonia/SkiaSharp (NuGet) |
| G15 | Hit testing | 🟡 sous-ensemble | boîte englobante non pivotée uniquement (`CanvasScene.HitTest`) ; formes précises différées à la Phase 3 |
| G16 | Clipping et performance | ⏳ Phase 3/4 | pertinent seulement avec un vrai rendu et de gros plans importés |

Le guide interdit explicitement de commencer le dessin des symboles métier
avant que G01 à G13 soient fiables — c'est exactement le périmètre couvert
ici, avec 57 tests dédiés (voir `docs/11_Testing.md`).

## Pourquoi G14/G15/G16 sont hors périmètre Phase 2

- **G14 (rendu vectoriel)** suppose un moteur de rendu (SkiaSharp via
  Avalonia). Ce moteur n'a jamais besoin d'exister pour que les calculs de
  coordonnées, la sélection ou l'undo/redo soient corrects et testables —
  c'est précisément l'intérêt de séparer le moteur d'état (`Canvas`) du
  moteur de rendu (`Presentation`, Phase 3). L'ajouter maintenant aurait
  nécessité un accès NuGet indisponible dans cet environnement d'amorçage
  (voir `docs/01_Architecture.md` §5) sans apporter de valeur testable en
  Phase 2.
- **G15 (hit-testing)** : la version Phase 2 (boîte englobante axis-aligned)
  suffit au critère de sortie ("sélectionner") mais ne gère pas les formes
  pivotées avec précision pixel — cela n'a de sens qu'une fois de vraies
  formes de symboles définies (Phase 3, bibliothèque de symboles).
- **G16 (clipping/performance)** est une optimisation de rendu (culling des
  objets hors champ, retessellation partielle) : sans moteur de rendu réel,
  il n'y a rien à optimiser.

## Architecture interne

```text
ElectricalDesigner.Canvas/
├── Geometry/
│   ├── Point2D.cs        — point monde/écran (record struct, opérateurs +/-/*)
│   ├── Size2D.cs          — largeur/hauteur
│   ├── Rect2D.cs           — boîte englobante, Contains/Intersects/FromPoints
│   └── WorldTransform.cs    — G01-G04, immuable (chaque zoom/pan retourne une nouvelle valeur)
├── Scene/
│   ├── CanvasObjectId.cs   — identifiant fort (même principe que EntityId<T>)
│   ├── CanvasObject.cs      — G05 : x, y, width, height, rotation, scale, visible, locked, layer
│   ├── CanvasScene.cs        — collection d'objets, HitTest, QueryRect
│   └── GridSettings.cs        — G11 : grille + snap
├── Selection/
│   └── SelectionManager.cs    — G06/G10 : sélection simple et multiple
├── Commands/
│   ├── ICanvasCommand.cs       — pattern Command exact du guide §10
│   ├── CommandHistory.cs        — G13 : undo/redo
│   ├── AddObjectCommand.cs
│   ├── RemoveObjectCommand.cs
│   ├── MoveObjectsCommand.cs     — G07, groupé pour la multi-sélection
│   ├── RotateObjectCommand.cs     — G08
│   ├── ResizeObjectCommand.cs      — G09
│   ├── ChangePropertyCommand.cs     — générique (calque, verrouillage, ...)
│   └── CompositeCommand.cs           — regroupe plusieurs commandes en une entrée d'historique
├── Clipboard/
│   └── CanvasClipboard.cs             — G12 : copier/coller (nouveaux identifiants au collage)
└── Persistence/
    ├── SceneDto.cs                     — DTOs de sérialisation
    └── SceneSerializer.cs               — JSON, sauvegarde atomique (même principe que ProjectFileRepository)
```

## Décisions de conception notables

### `WorldTransform` est immuable

Chaque zoom ou panoramique retourne une **nouvelle** valeur plutôt que de
muter un état interne. Ce choix (déjà appliqué à `AuditableEntity` côté
Domain avec `nowUtc` explicite) rend le comportement déterministe et
testable sans widget réel : `WorldTransformTests.ZoomAt_GardeLePointMondeSousLeCurseurAuMemePointEcran`
vérifie exactement la propriété qui définit un "zoom centré sur le
curseur" — le point monde sous le curseur reste sous ce même point écran
après le zoom.

### Le pattern Command du guide §10, littéralement

```text
Command
  execute()
  undo()
```

`ICanvasCommand` reproduit cette interface à l'identique. `CommandHistory`
est un historique linéaire classique (pile undo + pile redo, la pile redo
est vidée à chaque nouvelle exécution). Les commandes composites
(`CompositeCommand`) permettent qu'un collage de plusieurs objets, ou un
déplacement de toute une sélection multiple, ne produise qu'**une seule**
entrée d'historique — l'avantage "actions groupables" explicitement cité au
guide §10.

### `CanvasObject` ne connaît aucune règle métier

Contrairement à `ElectricalSymbolInstance` (Domain), `CanvasObject` n'a pas
de notion de circuit, de type de symbole ou de validité RGIE. Le seul point
de contact prévu avec le métier est `BusinessObjectId` (`Guid?`), qui sera
renseigné en Phase 3/4 pour relier un objet canvas à un
`ElectricalSymbolInstance` — jamais par une référence de type ou une
dépendance de projet, seulement par un identifiant opaque. C'est directement
la stratégie annoncée dans la mémoire de projet ("lier les objets canvas aux
entités métier via un simple Guid optionnel, jamais une référence de type").

Par cohérence, `CloneAsNew` (utilisé par le copier/coller) efface ce lien
par défaut : coller un objet ne doit jamais créer silencieusement un second
objet graphique pointant vers le même équipement métier (cela anticipe une
règle explicite de la Phase 5 — guide §9.2 : "doublon d'identifiant
impossible").

### Verrouillage (`Locked`)

`MoveObjectsCommand`, `RotateObjectCommand` et `ResizeObjectCommand` ignorent
silencieusement les objets verrouillés plutôt que de lever une exception :
un déplacement groupé sur une sélection mixte (objets libres + objets
verrouillés) doit déplacer ce qui peut l'être sans faire échouer toute
l'opération.

### Persistance de scène volontairement séparée de `.elecproj`

`SceneSerializer` sait sauvegarder/recharger une scène en JSON de façon
autonome (voir `docs/03_File_Format.md`, section "Persistance du canvas").
En Phase 2, une scène est un outil générique sans rattachement à une page de
projet ; ce rattachement (une scène par `SchematicPage`/`PositionPlan`,
intégrée dans `pages/*.json` du conteneur `.elecproj`) est prévu en Phase 3/4.

## Critère de sortie — validé

> "Placer 100 objets sur une scène, les déplacer, les sélectionner, zoomer,
> annuler et sauvegarder leurs coordonnées sans perte."

Voir `tests/ElectricalDesigner.Canvas.Tests/Integration/Phase2ExitCriteriaTests.cs`
(scénario complet, un seul test) et `src/ElectricalDesigner.App/Program.cs`
(démonstration console, section "Phase 2").

## Limites assumées (à lever en Phase 3/4)

- Hit-testing sur boîte englobante non pivotée uniquement (voir tableau
  ci-dessus, G15).
- Pas de rendu réel (G14) : ce moteur ne dessine rien, il décrit et transforme
  un état.
- Pas de notion de page/document : une `CanvasScene` est autonome, le lien
  vers `SchematicPage`/`PositionPlan` viendra en Phase 3/4.
- Pas de limite de profondeur d'historique dans `CommandHistory` (à revisiter
  si la mémoire devient un problème sur de très longues sessions d'édition).
