# 11 — Testing Strategy

> Livrable Phase 0 (guide §2, Phase 0 : "stratégie de test"). Le contenu
> détaillé (jeux de données TEST-01 à TEST-10, tests réglementaires) sera
> rempli progressivement à partir de la Phase 6/7.

## Niveaux de test (guide §21, cahier §34)

1. **Tests unitaires** — calculs, règles, conversions, sérialisation,
   identifiants, géométrie.
2. **Tests d'intégration** — créer projet → créer circuit → sauvegarder →
   fermer → réouvrir → vérifier intégrité.
3. **Tests graphiques (non-régression visuelle)** — rendu → capture →
   comparaison pixel/tolérance, sur un projet de référence.
4. **Tests réglementaires** — jeu de cas de référence validés par un expert
   métier (`input`, `expectedResult`, `regulationReference`,
   `expectedMessage`).
5. **Tests de propriété** (cahier §34.2) — ex. une rotation de 360° restitue
   la géométrie initiale ; copier/supprimer/annuler restitue l'état exact.

## Outillage cible vs. outillage actuel

| | Cible (guide §31, cahier §31.1) | Implémentation actuelle |
|---|---|---|
| Framework | xUnit | Mini-framework maison partagé (`tests/ElectricalDesigner.TestKit`) |
| Runner | `dotnet test` | `dotnet run --project tests/...` |
| Raison de l'écart | — | Pas d'accès NuGet dans l'environnement d'amorçage (voir `docs/01_Architecture.md` §5) |

Le mini-framework maison reproduit volontairement l'API xUnit la plus simple
(`[Fact]`, `Assert.*`, `Assert.Throws<T>`) pour que la migration soit
mécanique dès qu'un accès NuGet est disponible. Il est désormais dans son
propre projet (`ElectricalDesigner.TestKit`) et partagé par tous les projets
de tests, plutôt que dupliqué dans chacun.

## État actuel (Phase 1)

| Projet de tests | Portée | Nombre de tests |
|---|---|---|
| `ElectricalDesigner.Domain.Tests` | Invariants des entités, validation structurelle, scénario complet du critère de sortie Phase 1 | 16 |
| `ElectricalDesigner.Infrastructure.Tests` | Persistance : sauvegarde/rechargement, intégrité, fichier corrompu/absent (guide §21.2) | 4 |

## Ce qui est testé depuis la Phase 1

- Unicité et validité des identifiants (`EntityId<T>`).
- Invariants métier (ex. tension positive, cohérence DC/fréquence, unicité
  des repères de tableau/circuit, source directe XOR tableau amont).
- Validation des références croisées (`Project.Validate()`) : protection,
  câble, charge, tableau de destination inexistants sont détectés.
- Persistance complète : sauvegarde → fermeture → réouverture → vérification
  d'intégrité (exactement le scénario recommandé au guide §21.2), plus les
  cas d'erreur (fichier corrompu, fichier absent, version de format non
  supportée).

## Jeux de données de référence (guide §22)

À constituer à partir de la Phase 6/7 :

- TEST-01 : maison simple (tableau + 5 circuits).
- TEST-02 : maison à plusieurs tableaux.
- TEST-03 : installation triphasée.
- TEST-04 : installation avec PV.
- TEST-05 : installation avec borne de recharge.
- TEST-06 : ancienne installation.
- TEST-07 : plan architectural complexe.
- TEST-08 : DWG lourd.
- TEST-09 : PDF multipage.
- TEST-10 : projet volontairement incohérent (vérification du moteur de
  validation).
