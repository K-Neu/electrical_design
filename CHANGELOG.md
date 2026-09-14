# Changelog

Toutes les évolutions notables du dépôt sont consignées ici.

## [Phase 2] — Moteur graphique commun (canvas)

### Ajouté
- Nouveau projet `ElectricalDesigner.Canvas`, **zéro dépendance** (y compris
  vers `Domain`) : voir `docs/04_Canvas.md` et `docs/01_Architecture.md` §7.
- Géométrie (étapes G01-G04, guide §6) : `Point2D`, `Size2D`, `Rect2D`,
  `WorldTransform` (immuable ; transformation monde↔écran, zoom centré sur
  le curseur, panoramique).
- Scène (étapes G05, G11) : `CanvasObjectId` (identifiant fort), `CanvasObject`
  (position, taille, rotation, échelle, visibilité, verrouillage, calque,
  lien métier optionnel `BusinessObjectId`), `CanvasScene` (collection
  d'objets, hit-test ponctuel, requête rectangulaire), `GridSettings`
  (grille + accrochage).
- Sélection (étapes G06, G10) : `SelectionManager` (sélection simple,
  multiple, bascule).
- Commandes et historique (étapes G07-G09, G13, pattern Command exact du
  guide §10) : `ICanvasCommand`, `CommandHistory` (undo/redo),
  `AddObjectCommand`, `RemoveObjectCommand`, `MoveObjectsCommand` (déplacement
  groupé pour la multi-sélection), `RotateObjectCommand`,
  `ResizeObjectCommand`, `ChangePropertyCommand<T>` (générique),
  `CompositeCommand` (regroupe plusieurs commandes en une seule entrée
  d'historique).
- Presse-papiers (étape G12) : `CanvasClipboard` (copie de groupes de
  symboles, collage avec nouveaux identifiants et décalage, le lien métier
  n'est pas hérité par défaut).
- Persistance de scène : `SceneSerializer` (JSON, sauvegarde atomique),
  volontairement indépendante du format `.elecproj` (voir
  `docs/03_File_Format.md`, section "Persistance du canvas").
- `ElectricalDesigner.Canvas.Tests` : 57 tests (géométrie, scène, sélection,
  commandes/undo-redo, presse-papiers, persistance), incluant le scénario
  complet du critère de sortie Phase 2 en un seul test d'intégration.
- `src/ElectricalDesigner.App` étendu : démonstration du moteur canvas après
  la démonstration Phase 1 (100 objets placés, sélection/déplacement groupé,
  zoom centré sur le curseur, copier/coller, undo x2, sauvegarde/rechargement
  de scène sans perte de coordonnées).
- Documentation : nouveau `docs/04_Canvas.md` (ordre exact de construction
  G01-G16 et statut de chaque étape, décisions de conception, limites
  assumées) ; `docs/01_Architecture.md`, `docs/02_Domain_Model.md`,
  `docs/03_File_Format.md`, `docs/DEV_ENVIRONMENT.md`, `docs/11_Testing.md`
  mis à jour.

### Critère de sortie Phase 2 — validé
- "Placer 100 objets sur une scène, les déplacer, les sélectionner, zoomer,
  annuler et sauvegarder leurs coordonnées sans perte" : ✅
  (`Phase2ExitCriteriaTests`, démonstration `App/Program.cs`).
- Ordre exact du guide §6 : G01 à G13 implémentés et testés ; G14 (rendu
  vectoriel), G15 (hit-testing précis sur formes pivotées) et G16
  (clipping/performance) sont explicitement différés à la Phase 3/4 — voir
  `docs/04_Canvas.md` pour la justification détaillée de cette limite.

### Prochaine étape
Phase 3 — éditeur de schéma unifilaire (bibliothèque de symboles, palette,
placement, propriétés, liaisons, pagination, cartouche), consommant
`ElectricalDesigner.Canvas` pour l'édition graphique.

## [Phase 1] — Fondation du modèle de données

### Ajouté
- Modèle de domaine complet (`src/ElectricalDesigner.Domain/`) : `Project`
  (agrégat racine), `ProjectMetadata`, `ProjectRevision`,
  `ElectricalInstallation`, `Source`, `DistributionBoard`, `Circuit`,
  `ProtectionDevice`, `Cable`, `Load`, `ElectricalSymbolDefinition`,
  `ElectricalSymbolInstance`, `SchematicPage`, `PositionPlan`.
- Identifiant fortement typé `EntityId<TEntity>` et base `AuditableEntity`
  (createdAt/updatedAt, sans dépendance à l'horloge système).
- Validation structurelle du modèle (`Validate()` sur chaque entité,
  agrégée et complétée par des vérifications de références croisées sur
  `Project`).
- Persistance : format `.elecproj` (ZIP + `manifest.json` + `project.json`),
  `ProjectFileRepository` (sauvegarde atomique, vérification d'intégrité par
  hash SHA-256, détection de version de format non supportée).
- Port `IProjectFileRepository` dans `ElectricalDesigner.Application`
  (inversion de dépendance : Application définit le besoin, Infrastructure
  l'implémente).
- `ElectricalDesigner.TestKit` extrait en projet partagé (utilisé par
  `Domain.Tests` et le nouveau `Infrastructure.Tests`).
- 16 tests unitaires (Domain) + 4 tests d'intégration (Infrastructure,
  persistance) — voir `docs/11_Testing.md`.
- `src/ElectricalDesigner.App` démontre le scénario complet : créer un
  projet (tableau + circuit + protection + câble + charge), le valider, le
  sauvegarder, le recharger, vérifier l'intégrité.
- Documentation mise à jour : `docs/02_Domain_Model.md`,
  `docs/03_File_Format.md`, `docs/11_Testing.md`.

### Critère de sortie Phase 1 — validé
- Créer et sauvegarder un projet avec un tableau, un circuit, une protection
  et une charge, sans interface graphique : ✅ (`App/Program.cs`, tests
  d'intégration).
- Classes métier, validation du modèle, sérialisation, migrations/version du
  format (point d'extension prêt), identifiants stables : ✅.

### Prochaine étape
Phase 2 — moteur graphique commun (canvas).

## [Phase 0] — Cadrage technique

### Ajouté
- Dépôt Git initial.
- Solution .NET 10 (`ElectricalDesigner.slnx`) avec 4 projets :
  `Domain`, `Application`, `Infrastructure`, `App`.
- Projet de tests `ElectricalDesigner.Domain.Tests` avec mini-framework de
  test maison (`TestKit`), en attendant un accès NuGet pour migrer vers
  xUnit (voir `docs/01_Architecture.md` §5).
- Conventions de code (`docs/CODING_CONVENTIONS.md`, `.editorconfig`).
- Documentation d'architecture (`docs/01_Architecture.md`).
- Stratégie de persistance — stub (`docs/03_File_Format.md`).
- Stratégie de test (`docs/11_Testing.md`).
- Catalogue des normes versionné — squelette
  (`data/regulatory/RGIE-2026/manifest.json`, `docs/CATALOGUE_NORMES.md`).
- Guide d'environnement de développement (`docs/DEV_ENVIRONMENT.md`).
- CI GitHub Actions (`.github/workflows/ci.yml`) : restore, format, build,
  tests.
- `global.json` (SDK .NET 10 épinglé), `Directory.Build.props`
  (Nullable, ImplicitUsings, LangVersion).
- Documents sources versionnés dans `specs/`.

### Critère de sortie Phase 0 — validé
- `dotnet build` : succès, 0 avertissement, 0 erreur.
- `dotnet run --project src/ElectricalDesigner.App` : s'exécute et affiche
  l'état des trois couches.
- `dotnet run --project tests/ElectricalDesigner.Domain.Tests` : 3/3 tests
  passent, code de sortie 0.
- `dotnet format --verify-no-changes` : aucun écart.

### Prochaine étape
Phase 1 — fondation du modèle de données (voir
`specs/guide_developpement_reseau_electrique.md`, section "Phase 1").
