# Conventions de code

> Livrable Phase 0 (guide §2, Phase 0 : "conventions de code").

## Langage et style

- C# 13 / .NET 10, `Nullable` activé partout (`Directory.Build.props`).
- Namespaces **file-scoped** (`namespace Foo;`, pas de bloc `{ }`).
- 4 espaces d'indentation, fin de ligne `LF`, UTF-8. Voir `.editorconfig`.
- `var` uniquement quand le type est évident à la lecture.
- Les avertissements nullable (`CS8600`, `CS8602`, `CS8618`) sont traités
  comme des avertissements bloquants dans les revues (voir `.editorconfig`).

## Nommage

- Types, méthodes, propriétés : `PascalCase`.
- Interfaces : préfixe `I` (`IRuleEngine`, `ICalculationEngine`).
- Champs privés : `_camelCase`.
- Un fichier = un type public. Le nom du fichier correspond au nom du type.

## Organisation d'un projet

- Un projet par couche (`Domain`, `Application`, `Infrastructure`), jamais de
  référence "remontante" (ex. `Domain` ne référence jamais `Application`).
- Aucune règle RGIE codée en dur dans un composant graphique ou un
  gestionnaire d'événement UI (guide §28.1) — toujours via le `RuleEngine`.
- Aucun calcul électrique ne lit directement un widget ou une propriété
  graphique (guide §28.4) — toujours via un modèle normalisé
  (`Input Model → Normalized Electrical Model → Calculation Engine`).

## Règles métier vs. règles réglementaires

- Une règle de cohérence structurelle (ex. "un circuit doit avoir une
  protection") est une règle métier : elle vit dans `Domain`/`RuleEngine`
  générique.
- Une règle qui cite un article du RGIE est une règle réglementaire
  versionnée (`RegulationPack`, guide §Phase 11) : elle ne doit jamais être
  écrite "en dur" sans référence versionnée (livre, section, date
  d'application).

## Commits et Pull Requests (guide §20)

- Branches : `main`, `develop`, `feature/*`, `bugfix/*`, `release/*`,
  `hotfix/*`.
- Toute fonctionnalité doit fournir : code + tests + documentation +
  migration si nécessaire + données de démonstration si nécessaire.
- Aucun code métier important directement dans les composants graphiques.

## Documentation en ligne

- Les commentaires XML (`///`) sont réservés aux API publiques dont le
  comportement n'est pas évident (ex. règles de validation, formats de
  sérialisation). Ne pas documenter l'évident.
- Toute classe "temporaire" (comme les `BuildProbe` actuels) doit dire
  explicitement dans son commentaire XML pourquoi elle existe et quand elle
  doit être supprimée.
