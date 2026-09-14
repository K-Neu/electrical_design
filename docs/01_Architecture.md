# 01 — Architecture

> Statut : Phase 0 (cadrage technique) + mises à jour Phase 1/2. Ce document
> décrit l'architecture cible telle que définie par le cahier de
> spécification (§6, §31) et le guide de développement (§3, §4, §37).

## 1. Vue en couches

```text
Presentation            (Phase 3 — Avalonia, sera ajouté quand l'éditeur de schéma démarre)
    ↓
Application              ElectricalDesigner.Application
    ↓
Domain                    ElectricalDesigner.Domain
    ↑
Infrastructure            ElectricalDesigner.Infrastructure

ElectricalDesigner.Canvas   (moteur 2D générique, Phase 2 — voir §7 ci-dessous)
```

Règle stricte (guide §3.1, §37) : **Domain ne dépend de rien**. Il n'a ni
référence de projet, ni package NuGet. C'est la seule garantie qui permette,
plus tard, de porter le moteur métier vers un autre système d'exploitation ou
un autre moteur d'interface sans réécriture (cahier §31, §43).

| Projet | Rôle | Dépend de |
|---|---|---|
| `ElectricalDesigner.Domain` | Modèles métier, règles, calculs, événements de domaine | *(rien)* |
| `ElectricalDesigner.Application` | Orchestration des cas d'utilisation (`CreateProject`, `AddCircuit`, ...) | `Domain` |
| `ElectricalDesigner.Infrastructure` | Persistance, import/export, impression, licence, mise à jour | `Domain`, `Application` |
| `ElectricalDesigner.Canvas` | Moteur graphique 2D générique (coordonnées, zoom, pan, sélection, undo/redo) | *(rien — voir §7)* |
| `ElectricalDesigner.App` | Point d'entrée exécutable (console en Phase 0/1/2, Avalonia à partir de la Phase 3) | `Domain`, `Application`, `Infrastructure`, `Canvas` |

Ce découpage reprend exactement la section 3 du guide de développement.

## 2. Découpage en modules fonctionnels (cible)

Les modules M01 à M15 du guide (§4) seront répartis dans ces projets au fur et
à mesure des phases. Ils ne sont pas tous créés dès la Phase 0 : ils
apparaîtront comme des dossiers/namespaces dans `Domain`, `Application`,
`Infrastructure` ou `Canvas` selon leur nature, jamais comme des dépendances
circulaires.

| Module | Nom | Couche cible | Phase d'introduction |
|---|---|---|---|
| M01 | Project Core | Domain + Infrastructure (persistance) | Phase 1 |
| M02 | Electrical Domain | Domain | Phase 1 |
| M03 | Symbol Library | Domain (définitions) + Infrastructure (assets) | Phase 3 |
| M04 | Canvas Engine | `ElectricalDesigner.Canvas` (voir §7) | **Phase 2** |
| M05 | Schematic Editor | Presentation + Application | Phase 3 |
| M06 | Position Editor | Presentation + Application | Phase 4 |
| M07 | Cross Reference Engine | Application | Phase 5 |
| M08 | Calculation Engine | Domain | Phase 8 |
| M09 | Rule Engine | Domain | Phase 7 |
| M10 | Material Engine | Application | Phase 6 |
| M11 | Import/Export | Infrastructure | Phase 9 |
| M12 | Document Composer | Infrastructure | Phase 6/9 |
| M13 | Persistence | Infrastructure | Phase 1 |
| M14 | Licensing | Infrastructure | Phase 10 |
| M15 | Diagnostics | Infrastructure | Phase 10 |

## 3. Trois vérités à ne jamais mélanger (guide §1.3)

1. **Vérité réglementaire** — ce que le RGIE exige (Rule Engine).
2. **Vérité électrique** — propriétés et relations physiques (Domain).
3. **Vérité graphique** — position, taille, rotation, apparence, pagination
   (`ElectricalDesigner.Canvas`, depuis la Phase 2).

Une modification graphique ne doit jamais modifier une caractéristique
électrique par accident. Concrètement : les classes de `Domain` n'ont
**aucune** propriété de rendu (x, y, rotation, couleur...). Ces propriétés
vivent désormais dans `ElectricalDesigner.Canvas` (`CanvasObject`), un projet
qui ne référence PAS `Domain` — le découplage est donc garanti par le
compilateur, pas seulement par convention. Le lien entre un objet canvas et
une entité métier (un `ElectricalSymbolInstance`) se fera en Phase 3/4 via un
identifiant applicatif (`BusinessObjectId` optionnel sur `CanvasObject`),
jamais par héritage ni par référence directe.

## 4. Solution actuelle (Phase 2)

```text
ElectricalDesigner.slnx
├── src/
│   ├── ElectricalDesigner.Domain/            (class library, zéro dépendance)
│   │   ├── Common/          EntityId<T>, AuditableEntity, ValidationIssue, DomainValidationException
│   │   ├── Projects/        Project (agrégat racine), ProjectMetadata, ProjectRevision, enums
│   │   ├── Installations/   ElectricalInstallation, enums (CurrentType, EarthingSystem...)
│   │   ├── Sources/         Source, SourceType
│   │   ├── Boards/          DistributionBoard
│   │   ├── Circuits/        Circuit, CircuitCalculationSnapshot
│   │   ├── Protections/     ProtectionDevice, ProtectionType, ResidualCurrentType
│   │   ├── Cabling/         Cable, ConductorMaterial, InsulationMaterial, InstallationMethod
│   │   ├── Loads/           Load, LoadCategory
│   │   ├── Symbols/         ElectricalSymbolDefinition, ElectricalSymbolInstance, SymbolCategory
│   │   └── Plans/           SchematicPage, PositionPlan
│   ├── ElectricalDesigner.Application/       (class library, dépend de Domain)
│   │   └── Persistence/     IProjectFileRepository (port, implémenté par Infrastructure)
│   ├── ElectricalDesigner.Infrastructure/    (class library, dépend de Domain + Application)
│   │   └── Persistence/     ProjectFileRepository (ZIP+JSON), ProjectDtoMapper, Dtos/
│   ├── ElectricalDesigner.Canvas/            (class library, ZÉRO dépendance — voir docs/04_Canvas.md)
│   │   ├── Geometry/         Point2D, Size2D, Rect2D, WorldTransform
│   │   ├── Scene/             CanvasObjectId, CanvasObject, CanvasScene, GridSettings
│   │   ├── Selection/          SelectionManager
│   │   ├── Commands/            ICanvasCommand, CommandHistory, tous les CanvasCommand*
│   │   ├── Clipboard/            CanvasClipboard
│   │   └── Persistence/           SceneSerializer, SceneDto
│   └── ElectricalDesigner.App/               (exécutable console — placeholder avant Avalonia)
└── tests/
    ├── ElectricalDesigner.TestKit/           (class library — mini-framework de test partagé)
    ├── ElectricalDesigner.Domain.Tests/      (tests unitaires du modèle métier)
    ├── ElectricalDesigner.Infrastructure.Tests/ (tests d'intégration de la persistance)
    └── ElectricalDesigner.Canvas.Tests/      (tests unitaires + critère de sortie Phase 2)
```

Voir `docs/02_Domain_Model.md` pour le détail du modèle métier,
`docs/03_File_Format.md` pour la persistance du projet et
`docs/04_Canvas.md` pour le moteur graphique.

## 5. Pourquoi pas encore Avalonia/SkiaSharp/SQLite ?

L'architecture technique cible (cahier §31.1) est : .NET 10, Avalonia,
SkiaSharp, SQLite, xUnit. Dans l'environnement ayant servi à amorcer ce dépôt,
l'accès réseau à `nuget.org` n'était pas disponible (liste blanche réseau
restreinte), donc **aucun package NuGet n'a pu être restauré**. Le socle
Phase 0/1/2 a donc été construit avec zéro dépendance NuGet, en s'appuyant
uniquement sur le SDK .NET 10 lui-même — y compris `ElectricalDesigner.Canvas`,
qui n'a par ailleurs *jamais* besoin d'Avalonia : c'est un moteur d'état 2D
pur (mathématiques + structures de données), pas un moteur de rendu. Le
rendu réel (étape G14 du guide §6) sera ajouté en Phase 3 quand un binding
Avalonia/SkiaSharp deviendra nécessaire pour *afficher* les scènes que ce
moteur sait déjà décrire, transformer et faire évoluer.

Dès que ce dépôt tourne dans un environnement avec accès NuGet standard (poste
de développement normal, CI GitHub Actions — voir `.github/workflows/ci.yml`),
il faut :

1. Ajouter les packages `Avalonia`, `Avalonia.Desktop`, `Avalonia.Skia` à un
   nouveau projet `ElectricalDesigner.Presentation` (Phase 3), qui
   consommera `ElectricalDesigner.Canvas` pour le rendu et l'interaction.
2. Ajouter `Microsoft.NET.Test.Sdk`, `xunit`, `xunit.runner.visualstudio` aux
   projets de tests, puis migrer les `[Fact]` maison vers les `[Fact]` xUnit
   (API volontairement identique, voir commentaire dans
   `tests/.../TestKit/FactAttribute.cs`).
3. Ajouter `Microsoft.Data.Sqlite` à `ElectricalDesigner.Infrastructure` le
   jour où une base locale devient nécessaire (M13 — Persistence).

## 6. Décision d'architecture : portabilité (guide §37)

`Domain`, `Application`, `Canvas` et le futur moteur de calcul/règles ne
doivent jamais dépendre d'un framework graphique. C'est déjà vrai aujourd'hui :
aucun de ces projets ne référence Avalonia, WPF ou tout autre toolkit UI —
`ElectricalDesigner.Canvas` en particulier ne référence **aucun** autre
projet du dépôt (vérifiable dans son `.csproj`), afin de rester réutilisable
tel quel par le futur éditeur de schéma (Phase 3) et l'éditeur de plan
(Phase 4), comme l'exige le guide (§1.1, "moteur 2D générique réutilisable
par les deux éditeurs").

## 7. `ElectricalDesigner.Canvas` (Phase 2)

Le guide de développement (§6) impose un ordre exact de construction du
moteur graphique, de G01 (coordonnées monde) à G16 (clipping/performance),
et interdit explicitement de commencer le dessin des symboles métier avant
que G01 à G13 soient fiables. `ElectricalDesigner.Canvas` implémente G01 à
G13 (le rendu vectoriel G14, le hit-testing avancé G15 et le clipping/perf
G16 sont hors périmètre Phase 2 — voir `docs/04_Canvas.md` pour le détail et
la justification de cette limite).

Ce projet est volontairement un pur moteur d'état, sans notion de "symbole
électrique" : il manipule des `CanvasObject` génériques (position, taille,
rotation, calque, verrouillage, visibilité) identifiés par un
`CanvasObjectId` fort, complètement ignorants du modèle RGIE. C'est cette
généricité qui permettra, en Phase 3, de l'utiliser à la fois pour le
schéma unifilaire et, en Phase 4, pour le plan de position — sans dupliquer
de code de zoom/pan/sélection/undo entre les deux éditeurs.
