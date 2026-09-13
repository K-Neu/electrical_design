# Catalogue des normes et règles à versionner

> Livrable Phase 0 (guide §2, Phase 0 : "catalogue des normes et règles à
> versionner"). Ce document explique la structure ; le contenu réglementaire
> réel sera peuplé en Phase 7 (moteur de validation générique) et Phase 11
> (conformité réglementaire complète).

## Emplacement

```text
data/regulatory/RGIE-2026/manifest.json
```

## Principe (guide §Phase 11, §13.3 ; cahier §40)

Le référentiel réglementaire (RGIE aujourd'hui, potentiellement d'autres
juridictions plus tard) est **une donnée versionnée**, jamais du code codé en
dur dans l'interface ou dans le moteur de calcul.

```text
RulePack
  id: RGIE-BE
  version: 2026-04-01
  source: SPF Économie
  jurisdiction: BE
  scope: LV/LVLS
  rules: [...]
```

Chaque règle individuelle (`RuleDefinition`), lorsqu'elle sera ajoutée en
Phase 7, devra porter :

```text
RuleDefinition
    ├── id
    ├── version
    ├── domaine
    ├── severity        (INFO | WARNING | ERROR | BLOCKING)
    ├── condition
    ├── message
    ├── correctionHint
    └── regulationReference
          ├── livre
          ├── partie
          ├── chapitre
          ├── section
          ├── sous-section
          ├── source officielle (URL)
          ├── date d'application
          └── statut
```

## Ce que ce catalogue n'est pas

- Ce n'est **pas** une reproduction du texte réglementaire complet (risque de
  droits de reproduction, cahier §40) — seulement des références structurées
  vers les sources officielles.
- Ce n'est **pas** encore un moteur exécutable en Phase 0 : `rules: []` est
  intentionnellement vide.

## Sources déjà identifiées à cadrer (à affiner en Phase 7/11)

- RGIE Livre 1 (basse tension / très basse tension), version 06, arrêté royal
  du 6 octobre 2025, applicable à partir du 1er avril 2026 — SPF Économie.
- RGIE Livres 2 et 3 (hors périmètre MVP initial, à cadrer plus tard).

## Prochaine étape

Au démarrage de la Phase 7 (moteur de validation), transformer les notes
`rules: []` de `manifest.json` en véritables `RuleDefinition`, en commençant
par les règles documentaires prioritaires listées au cahier §20.4 (adresse
présente, responsable d'exécution, identification des circuits, etc.).
