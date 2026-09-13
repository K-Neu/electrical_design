# Logiciel de conception de réseaux électriques

Logiciel de bureau de conception électrique (schéma unifilaire + plan de
position) pour installations basse tension, inspiré fonctionnellement de
Trikker, ciblant le périmètre réglementaire belge (RGIE/AREI).

Ce dépôt suit deux documents sources, versionnés dans `specs/` :
- le **cahier de spécification fonctionnelle**
  (`specs/cahier_specification_logiciel_reseau_electrique.md`) ;
- le **guide de développement**
  (`specs/guide_developpement_reseau_electrique.md`), qui définit l'ordre de
  construction du produit et sert de feuille de route phase par phase.

## État actuel : Phase 1 — fondation du modèle de données ✅

Phase 0 (cadrage technique) ✅ puis Phase 1 ✅.

| Critère de sortie Phase 1 (guide §Phase 1) | Statut |
|---|---|
| Classes métier (Project, Circuit, DistributionBoard, ProtectionDevice, Cable, Load, ...) | ✅ `src/ElectricalDesigner.Domain/` |
| Validation du modèle | ✅ `Project.Validate()` + validation par entité |
| Sérialisation | ✅ `ProjectFileRepository` (ZIP + JSON, voir `docs/03_File_Format.md`) |
| Migrations/version du format | ✅ `FormatVersion` vérifié à l'ouverture (point d'extension prêt) |
| Identifiants stables | ✅ `EntityId<T>` fortement typé (guide §5.1) |
| Créer et sauvegarder un projet (tableau + circuit + protection + charge) sans UI | ✅ voir `src/ElectricalDesigner.App/Program.cs` et les tests d'intégration |

**Prochaine étape : Phase 2 — moteur graphique commun** (canvas : coordonnées,
zoom, pan, sélection, déplacement, rotation, undo/redo...).

## Démarrage rapide

```bash
dotnet build
dotnet run --project src/ElectricalDesigner.App
dotnet run --project tests/ElectricalDesigner.Domain.Tests
dotnet run --project tests/ElectricalDesigner.Infrastructure.Tests
```

Voir `docs/DEV_ENVIRONMENT.md` pour l'installation du SDK et le détail.

## Structure du dépôt

```text
ElectricalDesigner.slnx
├── src/
│   ├── ElectricalDesigner.Domain/          # Modèle métier — zéro dépendance (voir docs/02_Domain_Model.md)
│   ├── ElectricalDesigner.Application/     # Cas d'utilisation, ports (IProjectFileRepository)
│   ├── ElectricalDesigner.Infrastructure/  # Persistance .elecproj (ZIP+JSON), imports/exports futurs
│   └── ElectricalDesigner.App/             # Exécutable (console en Phase 1 → Avalonia en Phase 2)
├── tests/
│   ├── ElectricalDesigner.TestKit/         # Mini-framework de test partagé (voir docs/11_Testing.md)
│   ├── ElectricalDesigner.Domain.Tests/    # Tests unitaires du modèle métier (16 tests)
│   └── ElectricalDesigner.Infrastructure.Tests/  # Tests d'intégration de la persistance (4 tests)
├── data/
│   └── regulatory/RGIE-2026/               # Catalogue réglementaire versionné (squelette, Phase 7)
├── docs/                                   # Documentation vivante, écrite au fil du développement
└── .github/workflows/ci.yml                # CI (build, format, tests)
```

## Documentation

| Document | Contenu |
|---|---|
| `docs/01_Architecture.md` | Couches, modules, règles de dépendance, décisions techniques |
| `docs/02_Domain_Model.md` | Entités du modèle métier (stub → Phase 1) |
| `docs/03_File_Format.md` | Format de fichier projet `.elecproj`, stratégie de persistance |
| `docs/11_Testing.md` | Stratégie de tests, jeux de données de référence |
| `docs/CODING_CONVENTIONS.md` | Conventions de code |
| `docs/DEV_ENVIRONMENT.md` | Installation et prise en main |
| `docs/CATALOGUE_NORMES.md` | Principe du catalogue réglementaire versionné |

## Principe fondamental

Le logiciel n'est **pas** un éditeur graphique auquel on ajoute des calculs.
Le cœur est un **modèle électrique structuré** ; le schéma unifilaire et le
plan de position en sont deux représentations. Voir `docs/01_Architecture.md`
pour le détail des trois vérités à ne jamais mélanger (réglementaire,
électrique, graphique).

> **Rappel important** (cahier §1) : la conformité réglementaire n'est
> jamais garantie automatiquement par le logiciel. L'outil fournit une aide à
> la conception et un contrôle de cohérence ; la validation finale dépend de
> l'installation réelle et du contrôle par un organisme agréé.
