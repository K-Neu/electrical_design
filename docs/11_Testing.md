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

## État actuel (Phase 2)

| Projet de tests | Portée | Nombre de tests |
|---|---|---|
| `ElectricalDesigner.Domain.Tests` | Invariants des entités, validation structurelle, scénario complet du critère de sortie Phase 1 | 16 |
| `ElectricalDesigner.Infrastructure.Tests` | Persistance : sauvegarde/rechargement, intégrité, fichier corrompu/absent (guide §21.2) | 4 |
| `ElectricalDesigner.Canvas.Tests` | Géométrie/transform, scène, sélection, commandes/undo-redo, presse-papiers, persistance de scène, scénario complet du critère de sortie Phase 2 | 57 |

**Total : 77 tests, tous verts.**

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

## Ce qui est testé depuis la Phase 2

- `WorldTransform` : aller-retour monde↔écran exact, zoom centré sur le
  curseur (le point monde sous le curseur reste sous ce même point écran),
  bornes min/max de zoom, panoramique tenant compte du zoom courant.
- `Rect2D` : `Contains`/`Intersects`/`FromPoints` (normalisation quel que
  soit l'ordre des points).
- `CanvasObject` : validation à la création, déplacement, **test de
  propriété** (cahier §34.2) "une rotation de 360° restitue la géométrie
  initiale", normalisation d'angle, boîte englobante tenant compte de
  l'échelle, clonage pour le copier/coller (nouvel identifiant, décalage,
  perte du verrouillage et du lien métier par défaut).
- `CanvasScene` : ajout/suppression, doublon d'identifiant refusé,
  hit-test ponctuel (objets invisibles jamais touchés), sélection
  rectangulaire, et le scénario "placer 100 objets" du critère de sortie.
- `SelectionManager` : sélection simple vs. multiple, bascule (toggle),
  vidage.
- `CommandHistory` : exécution/undo/redo, la pile de redo est vidée après une
  nouvelle exécution, une séquence create→move→undo→undo restitue l'état
  vide (**test de propriété**), undo sans historique lève une exception.
- Commandes individuelles : déplacement groupé (ignore les objets
  verrouillés), rotation/redimensionnement/changement de propriété
  génériques avec undo exact, suppression avec ré-insertion à l'identique,
  commande composite annulée dans l'ordre inverse.
- `CanvasClipboard` : copier/coller avec nouveaux identifiants, copie de
  groupes de symboles (cahier §12.1), le lien métier n'est pas hérité par
  défaut au collage.
- `SceneSerializer` : aller-retour JSON préservant toutes les propriétés,
  sauvegarde/rechargement fichier de 100 objets sans perte de coordonnées,
  fichier inexistant détecté.
- **Scénario complet du critère de sortie Phase 2**, en un seul test
  d'intégration (`Phase2ExitCriteriaTests`) : placer 100 objets, en
  sélectionner et déplacer une partie en un geste groupé, zoomer centré sur
  le curseur, copier/coller, annuler deux fois, puis sauvegarder/recharger
  sans perte de coordonnées.

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
