# 02 — Domain Model

> Statut : **implémenté (Phase 1)**. Ce document décrit le modèle tel qu'il
> existe réellement dans `src/ElectricalDesigner.Domain/`, pas une intention.

## Vue d'ensemble

```text
Project (agrégat racine)
  ├── Metadata: ProjectMetadata               (value object immuable)
  ├── Installation: ElectricalInstallation
  ├── Sources[]                               (catalogue plat)
  ├── ProtectionDevices[]                      (catalogue plat)
  ├── Cables[]                                 (catalogue plat)
  ├── Loads[]                                  (catalogue plat)
  ├── SymbolDefinitions[]                       (catalogue plat)
  ├── SchematicPages[]                          (structure minimale, Phase 3)
  ├── PositionPlans[]                            (structure minimale, Phase 4)
  ├── Revisions[]                                 (historique append-only)
  └── Boards[]: DistributionBoard
        └── Circuits[]: Circuit
              ├── ProtectionId?  -> ProtectionDevices
              ├── CableId?       -> Cables
              ├── DestinationBoardId? -> Boards (chaîne vers un sous-tableau)
              └── LoadIds[]      -> Loads
```

`Project` est la **seule** porte d'entrée pour construire un projet cohérent :
toutes les méthodes `AddXxx` valident l'unicité des repères avant de muter
l'état. `Project.Validate()` vérifie en plus toutes les références croisées
(protectionId, cableId, sourceId, boardId, loadIds...) — c'est le livrable
"validation du modèle" de la Phase 1.

## Identifiants (guide §5.1)

`EntityId<TEntity>` (`Domain/Common/EntityId.cs`) est un identifiant fortement
typé : `EntityId<Circuit>` et `EntityId<DistributionBoard>` sont des types
distincts pour le compilateur, ce qui empêche par construction de confondre
un identifiant de circuit avec un identifiant de tableau. La position
graphique n'est jamais utilisée comme identité — d'ailleurs, il n'existe
aucune propriété graphique dans le Domain (voir plus bas). Le moteur canvas
de la Phase 2 (`ElectricalDesigner.Canvas`) réutilise le même schéma
d'identifiant fort (`CanvasObjectId`), mais dans un projet totalement
séparé : voir `docs/04_Canvas.md`.

## Horodatage et déterminisme

`AuditableEntity` (`Domain/Common/AuditableEntity.cs`) porte `CreatedAtUtc` /
`UpdatedAtUtc`. Aucune entité ne lit `DateTimeOffset.UtcNow` en interne :
chaque méthode de mutation reçoit `nowUtc` en paramètre explicite. Le Domain
est donc entièrement déterministe et testable sans dépendre de l'horloge de
la machine qui exécute les tests.

Chaque entité expose deux chemins de construction :
- **`Create(...)`** : création d'une toute nouvelle entité (un seul instant
  "maintenant" pour `createdAt` et `updatedAt`), avec validation complète des
  invariants. C'est le chemin utilisé par le code applicatif.
- **`Restore(...)`** : reconstruction depuis des données déjà persistées, en
  préservant exactement les horodatages d'origine. Utilisé **uniquement** par
  `ElectricalDesigner.Infrastructure.Persistence.ProjectDtoMapper`.

## Séparation définition / instance (guide §5.3)

`ElectricalSymbolDefinition` (le gabarit) et `ElectricalSymbolInstance`
(l'utilisation dans un projet) sont deux types distincts. En Phase 1,
`ElectricalSymbolInstance` ne porte que des propriétés métier (référence à la
définition, au circuit éventuel, repère affiché) — **aucune** propriété
graphique.

## Les trois vérités ne sont pas mélangées (guide §1.3)

Concrètement :
- Aucune classe du Domain n'a de propriété `x`, `y`, `rotation`, `layer`,
  `color`... Ces propriétés graphiques (guide §5.4) vivent désormais dans
  `ElectricalDesigner.Canvas` (Phase 2, voir `docs/04_Canvas.md`) — un projet
  distinct, sans dépendance vers `Domain`, précisément pour que le graphique
  ne puisse jamais muter une caractéristique électrique par accident.
- `SchematicPage` et `PositionPlan` existent déjà comme structures (identité,
  titre, ordre, liste des symboles qu'elles contiennent) mais n'ont aucun
  contenu géométrique réel : le lien entre une page et une scène canvas sera
  établi en Phase 3/4 lorsque l'éditeur de schéma/plan consommera
  `ElectricalDesigner.Canvas`.
- `Circuit.CalculationData` (type `CircuitCalculationSnapshot`) est un type
  intentionnellement vide : il réserve la place pour les résultats du moteur
  de calcul (Phase 8) sans anticiper leur forme exacte.

## Entités implémentées

| Entité | Fichier | Remarque |
|---|---|---|
| `Project` | `Projects/Project.cs` | Agrégat racine |
| `ProjectMetadata` | `Projects/ProjectMetadata.cs` | Value object immuable (`record`) |
| `ProjectRevision` | `Projects/ProjectRevision.cs` | Immuable une fois créée |
| `ElectricalInstallation` | `Installations/ElectricalInstallation.cs` | |
| `Source` | `Sources/Source.cs` | Réseau, transformateur, PV, batterie... |
| `DistributionBoard` | `Boards/DistributionBoard.cs` | Possède ses `Circuits` |
| `Circuit` | `Circuits/Circuit.cs` | Attributs exacts du guide §5.2 |
| `ProtectionDevice` | `Protections/ProtectionDevice.cs` | |
| `Cable` | `Cabling/Cable.cs` | |
| `Load` | `Loads/Load.cs` | |
| `ElectricalSymbolDefinition` | `Symbols/ElectricalSymbolDefinition.cs` | Gabarit, Phase 3 le peuplera |
| `ElectricalSymbolInstance` | `Symbols/ElectricalSymbolInstance.cs` | Sans propriété graphique |
| `SchematicPage` | `Plans/SchematicPage.cs` | Structure minimale |
| `PositionPlan` | `Plans/PositionPlan.cs` | Structure minimale |

## Validation structurelle vs. RGIE

`Validate()` (présent sur `Project` et sur chaque entité) ne vérifie que des
invariants structurels (référence obligatoire, tension positive, référence
croisée existante...). Ce n'est **pas** le moteur de règles RGIE : celui-ci
arrivera en Phase 7 sous forme de `RuleFinding` versionnés avec sévérité et
référence légale (voir `docs/CATALOGUE_NORMES.md`).

## Décisions de conception notables

- **Load.CircuitId n'existe pas** : la relation circuit ↔ charges est portée
  uniquement par `Circuit.LoadIds` (guide §5.2), pour n'avoir qu'une seule
  source de vérité.
- **DistributionBoard.SourceId / UpstreamBoardId sont mutuellement
  exclusifs** : `SetDirectSource` efface `UpstreamBoardId`,
  `SetUpstreamBoard` efface `SourceId`. Garanti par construction, pas
  seulement par convention.
- **ProtectionDevice, Cable, Load sont des catalogues plats** au niveau
  `Project` (comme des tables normalisées), référencés par identifiant depuis
  `Circuit` — plutôt que possédés directement par le circuit — pour permettre
  une éventuelle réutilisation et rester proche du modèle relationnel décrit
  au cahier §9.
