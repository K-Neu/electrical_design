# 03 — File Format & Persistence Strategy

> Statut : **implémenté (Phase 1, version 1 du format)**. Code :
> `src/ElectricalDesigner.Infrastructure/Persistence/`.

## Format actuel

Conteneur ZIP (`.elecproj`) contenant deux entrées :

```text
projet.elecproj
├── manifest.json
└── project.json
```

Les sous-dossiers `pages/`, `assets/`, `calculations/`, `history/` prévus par
la cible finale (cahier §26) n'ont pas encore de contenu réel en Phase 1 (pas
de canvas, pas de moteur de calcul) : `project.json` contient pour l'instant
l'intégralité de l'agrégat `Project` sérialisé (métadonnées, installation,
sources, tableaux et leurs circuits, protections, câbles, charges,
définitions de symboles, pages vides, révisions). Ils seront introduits au
fur et à mesure des phases correspondantes plutôt qu'anticipés maintenant.

### `manifest.json`

```json
{
  "FormatVersion": 1,
  "ApplicationVersion": "1.0.0.0",
  "RegulatoryPackVersion": null,
  "ProjectId": "b5409c9b-...",
  "SavedAtUtc": "2026-09-10T10:34:58.18Z",
  "ContentHash": "a1d18dedcc6c7e2c29e6b9888aabc437f0e1a4678db85ed2f24ed36d54671d4a"
}
```

`ContentHash` est un SHA-256 du contenu exact de `project.json`, vérifié à
chaque `Load()` : un fichier altéré en dehors de l'application est détecté et
rejeté (`InvalidDataException`) plutôt que silencieusement accepté.

### `project.json`

DTOs dédiés dans `Persistence/Dtos/ProjectFileDtos.cs`, distincts des
entités Domain (voir `docs/02_Domain_Model.md` — le Domain ignore tout de la
sérialisation). Enums sérialisées en toutes lettres (`"AcSinglePhase"`, pas
un entier) pour rester lisible et stable si l'ordre des valeurs d'une enum
change un jour.

## Principes non négociables (guide §11, §30 ; cahier §25.2) — état d'implémentation

1. **Sauvegarde atomique** ✅ — `ProjectFileRepository.Save` écrit dans
   `<fichier>.tmp`, relit et vérifie le hash du contenu écrit, puis
   `File.Move(..., overwrite: true)`. Le fichier `.tmp` est toujours nettoyé
   (bloc `finally`), y compris en cas d'échec.
2. **Autosave séparé** ⏳ — pas encore implémenté (au-delà du périmètre
   Phase 1 ; prévu Phase 10).
3. **Format versionné dès le premier jour** ✅ — `FormatVersion = 1` est
   vérifié à l'ouverture. Un écart lève `NotSupportedException` avec un
   message explicite ; c'est le point d'extension où viendra la logique de
   migration `v1 → v2` (guide §30) quand un `v2` existera.
4. **Vérification d'intégrité au chargement** ✅ — hash SHA-256 vérifié,
   `FileNotFoundException` si le fichier n'existe pas,
   `InvalidDataException` si une entrée obligatoire manque ou que le hash ne
   correspond pas.

## Comportement testé

Voir `tests/ElectricalDesigner.Infrastructure.Tests/Persistence/ProjectFileRepositoryTests.cs`
(guide §21.2 — test d'intégration "créer → sauvegarder → fermer → rouvrir →
vérifier intégrité") :

- sauvegarde puis rechargement d'un projet complet (tableau, circuit,
  protection, câble, charge, révision) : toutes les valeurs et références
  sont identiques après rechargement, `Validate()` retourne 0 problème ;
- chargement d'un fichier corrompu (un octet altéré) : `InvalidDataException` ;
- chargement d'un fichier inexistant : `FileNotFoundException`.

## Persistance du canvas (Phase 2)

Le moteur canvas (`ElectricalDesigner.Canvas`, voir `docs/04_Canvas.md`) a
son propre mécanisme de sérialisation (`SceneSerializer`, JSON), volontairement
**indépendant** du format `.elecproj` : en Phase 2, une scène canvas est un
outil générique (100 objets génériques, sans signification électrique) qui
n'est pas encore rattaché à un `SchematicPage`/`PositionPlan`. Ce rattachement
(et donc l'intégration de `pages/*.json` dans `.elecproj`) est prévu en
Phase 3/4, quand l'éditeur de schéma/plan attribuera une scène par page.

## Hors périmètre Phase 1/2 (rappel)

- Chiffrement du stockage local (cahier §32) — Phase 10.
- Signature/hash des documents **publiés** (distinct du hash d'intégrité
  interne déjà en place) — Phase 10/11.
- Snapshots d'historique complet (`history/snapshots/`), au-delà de l'undo/
  redo en mémoire du canvas (Phase 2, en mémoire uniquement) et de
  l'historique de révisions déjà en place (`Project.Revisions`, append-only).
- Migration `v1 → v2` : le point d'extension existe (`FormatVersion` vérifié
  explicitement) mais aucune migration n'est encore nécessaire.
- Intégration `pages/*.json` (scènes canvas rattachées aux pages du projet) —
  Phase 3/4.
