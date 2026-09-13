# Changelog

Toutes les évolutions notables du dépôt sont consignées ici.

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
