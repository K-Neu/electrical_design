# 01 — Architecture

> Statut : Phase 0 (cadrage technique). Ce document décrit l'architecture cible
> telle que définie par le cahier de spécification (§6, §31) et le guide de
> développement (§3, §4, §37). Il sera complété au fil des phases.

## 1. Vue en couches

```text
Presentation            (Phase 2/3 — Avalonia, sera ajouté quand le canvas démarre)
    ↓
Application              ElectricalDesigner.Application
    ↓
Domain                    ElectricalDesigner.Domain
    ↑
Infrastructure            ElectricalDesigner.Infrastructure
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
| `ElectricalDesigner.App` | Point d'entrée exécutable (console en Phase 0, Avalonia à partir de la Phase 2) | `Domain`, `Application`, `Infrastructure` |

Ce découpage reprend exactement la section 3 du guide de développement.

## 2. Découpage en modules fonctionnels (cible)

Les modules M01 à M15 du guide (§4) seront répartis dans ces projets au fur et
à mesure des phases. Ils ne sont pas tous créés dès la Phase 0 : ils
apparaîtront comme des dossiers/namespaces dans `Domain`, `Application` ou
`Infrastructure` selon leur nature, jamais comme des dépendances circulaires.

| Module | Nom | Couche cible | Phase d'introduction |
|---|---|---|---|
| M01 | Project Core | Domain + Infrastructure (persistance) | Phase 1 |
| M02 | Electrical Domain | Domain | Phase 1 |
| M03 | Symbol Library | Domain (définitions) + Infrastructure (assets) | Phase 3 |
| M04 | Canvas Engine | Presentation | Phase 2 |
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
3. **Vérité graphique** — position, taille, rotation, apparence, pagination (Presentation).

Une modification graphique ne doit jamais modifier une caractéristique
électrique par accident. Concrètement : les classes de `Domain` n'ont
**aucune** propriété de rendu (x, y, rotation, couleur...). Ces propriétés
vivront dans des objets de représentation séparés (`SchematicRepresentation`,
`PositionRepresentation`), introduits en Phase 1/Phase 5.

## 4. Solution actuelle (Phase 1)

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
│   └── ElectricalDesigner.App/               (exécutable console — placeholder avant Avalonia)
└── tests/
    ├── ElectricalDesigner.TestKit/           (class library — mini-framework de test partagé)
    ├── ElectricalDesigner.Domain.Tests/      (tests unitaires du modèle métier)
    └── ElectricalDesigner.Infrastructure.Tests/ (tests d'intégration de la persistance)
```

Voir `docs/02_Domain_Model.md` pour le détail du modèle et
`docs/03_File_Format.md` pour la persistance.

## 5. Pourquoi pas encore Avalonia/SkiaSharp/SQLite ?

L'architecture technique cible (cahier §31.1) est : .NET 10, Avalonia,
SkiaSharp, SQLite, xUnit. Dans l'environnement ayant servi à amorcer ce dépôt,
l'accès réseau à `nuget.org` n'était pas disponible (liste blanche réseau
restreinte), donc **aucun package NuGet n'a pu être restauré**. Le socle
Phase 0 a donc été construit avec zéro dépendance NuGet, en s'appuyant
uniquement sur le SDK .NET 10 lui-même.

Dès que ce dépôt tourne dans un environnement avec accès NuGet standard (poste
de développement normal, CI GitHub Actions — voir `.github/workflows/ci.yml`),
il faut :

1. Ajouter les packages `Avalonia`, `Avalonia.Desktop`, `Avalonia.Skia` au
   projet `ElectricalDesigner.App` (ou à un nouveau projet
   `ElectricalDesigner.Presentation` dédié, recommandé dès la Phase 2).
2. Ajouter `Microsoft.NET.Test.Sdk`, `xunit`, `xunit.runner.visualstudio` au
   projet de tests, puis migrer les `[Fact]` maison vers les `[Fact]` xUnit
   (API volontairement identique, voir commentaire dans
   `tests/.../TestKit/FactAttribute.cs`).
3. Ajouter `Microsoft.Data.Sqlite` à `ElectricalDesigner.Infrastructure` le
   jour où une base locale devient nécessaire (M13 — Persistence).

## 6. Décision d'architecture : portabilité (guide §37)

`Domain`, `Application` et le futur moteur de calcul/règles ne doivent jamais
dépendre d'un framework graphique. C'est déjà vrai aujourd'hui : ni `Domain`
ni `Application` ne référencent Avalonia, WPF ou tout autre toolkit UI.
