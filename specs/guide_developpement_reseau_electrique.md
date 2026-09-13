# Guide de développement — logiciel de conception de réseaux électriques

## 0. Objet

Ce document transforme le cahier de spécification fonctionnel en plan de production logiciel. L'objectif est de définir **dans quel ordre construire le produit**, quels composants doivent être stabilisés avant les suivants, quels livrables doivent être produits à chaque étape et comment valider chaque incrément.

Le produit cible un logiciel de bureau de conception électrique inspiré fonctionnellement de Trikker : création d'un projet, schéma unifilaire, plan de position, liaison entre les deux représentations, bibliothèque de symboles, nomenclature, import de plans, impression/export et contrôles réglementaires.

Le guide ne cherche pas à reproduire l'implémentation interne de Trikker. Il définit une architecture indépendante et un ordre de développement permettant d'obtenir progressivement un produit professionnel.

---

# 1. Principes de production

## 1.1 Règle fondamentale

Le logiciel ne doit pas être développé comme un gros éditeur graphique auquel on ajouterait ensuite des calculs. Le cœur doit être un **modèle électrique structuré**, dont les vues graphiques ne sont que des représentations.

Architecture logique :

```text
Projet
  ├── Modèle électrique
  │     ├── tableaux
  │     ├── circuits
  │     ├── protections
  │     ├── câbles
  │     ├── récepteurs
  │     └── équipements
  │
  ├── Schéma unifilaire
  │     └── représentation du modèle électrique
  │
  ├── Plan de position
  │     └── représentation spatiale du même projet
  │
  ├── Moteur de calcul
  │
  ├── Moteur de validation
  │
  ├── Nomenclature
  │
  └── Documents / exports
```

Cette séparation est capitale. Elle permettra de modifier l'affichage sans casser les calculs et de recalculer les documents à partir d'un seul référentiel de données.

## 1.2 Développement par incréments verticaux

Chaque phase doit produire un morceau utilisable et testable du logiciel.

Éviter :

```text
6 mois de développement backend
→ 4 mois de moteur graphique
→ première démonstration
```

Préférer :

```text
Projet minimal
→ un circuit
→ affichage du circuit
→ sauvegarde
→ réouverture
→ export
```

Puis enrichir ce socle progressivement.

## 1.3 Séparer trois types de vérité

Le logiciel doit distinguer :

1. **Vérité réglementaire** : ce que le RGIE exige.
2. **Vérité électrique** : propriétés et relations physiques du réseau.
3. **Vérité graphique** : position, taille, rotation, apparence et pagination.

Une modification graphique ne doit jamais modifier une caractéristique électrique par accident.

---

# 2. Vision d'ensemble du programme

## Phase 0 — cadrage technique

Durée indicative : 1 à 2 semaines.

Objectif : transformer le cahier des charges en architecture de travail.

Livrables :
- dépôt Git initial ;
- conventions de code ;
- architecture des modules ;
- système de build ;
- CI ;
- environnement de développement ;
- premier projet vide ;
- stratégie de persistance ;
- stratégie de test ;
- catalogue des normes et règles à versionner.

Critère de sortie : un développeur doit pouvoir cloner le projet, compiler, lancer l'application et exécuter les tests.

## Phase 1 — fondation du modèle de données

Objectif : créer le cœur métier sans interface complexe.

Construire :
- Project ;
- Installation ;
- DistributionBoard ;
- Circuit ;
- ProtectionDevice ;
- Cable ;
- Load ;
- ElectricalSymbolDefinition ;
- ElectricalSymbolInstance ;
- PositionPlan ;
- SchematicPage ;
- ProjectRevision.

Livrables :
- classes métier ;
- validation du modèle ;
- sérialisation ;
- migrations/version du format ;
- identifiants stables.

Critère de sortie : créer et sauvegarder un projet avec un tableau, un circuit, une protection et une charge sans interface graphique.

## Phase 2 — moteur graphique commun

Objectif : construire un moteur 2D générique réutilisable par les deux éditeurs.

Fonctions :
- coordonnées ;
- zoom ;
- panoramique ;
- sélection ;
- déplacement ;
- rotation ;
- redimensionnement ;
- accrochage/grille ;
- multi-sélection ;
- copie/collage ;
- annulation/rétablissement ;
- calques ;
- repères ;
- snapping ;
- transformations ;
- rendu vectoriel.

Critère de sortie : placer 100 objets sur une scène, les déplacer, les sélectionner, zoomer, annuler et sauvegarder leurs coordonnées sans perte.

## Phase 3 — éditeur de schéma unifilaire

Objectif : obtenir le premier produit véritablement utilisable.

Construire :
- bibliothèque de symboles ;
- palette ;
- placement ;
- propriétés ;
- liaisons ;
- circuits ;
- tableaux ;
- repérage ;
- textes ;
- lignes ;
- pagination ;
- cartouche.

Le manuel Trikker documente notamment des modes dessin/sélection, copie, recherche d'un symbole dans l'autre vue, déplacement libre et raccourcis dédiés au schéma et au plan. Ces comportements constituent de bonnes références UX, mais doivent être réimplémentés indépendamment. [1]

Critère de sortie : créer un petit tableau avec plusieurs circuits et produire un schéma lisible imprimable.

## Phase 4 — plan de position

Objectif : permettre la représentation spatiale.

Construire :
- feuille ;
- murs ;
- portes ;
- fenêtres ;
- pièces ;
- texte ;
- symboles électriques ;
- rotation ;
- échelle ;
- import d'un fond ;
- transparence/verrouillage du fond.

Critère de sortie : importer un plan architectural, placer les équipements et sortir un plan de position.

## Phase 5 — synchronisation schéma ↔ plan

Objectif : rendre le logiciel réellement orienté projet plutôt que dessin.

Exigences :
- chaque équipement électrique possède un identifiant métier ;
- le même équipement peut avoir une représentation dans les deux vues ;
- changement de désignation propagé ;
- recherche croisée ;
- avertissement lorsqu'un élément existe dans une vue mais pas dans l'autre ;
- aucune duplication manuelle de l'identité métier.

Critère de sortie : sélectionner une prise sur le plan et retrouver son circuit sur le schéma, puis inversement.

## Phase 6 — nomenclature

Objectif : produire automatiquement la liste de matériel.

Construire :
- agrégation ;
- quantités ;
- unités ;
- désignation ;
- caractéristiques ;
- regroupement ;
- tri ;
- export CSV/XLSX ;
- impression.

Critère de sortie : modifier le schéma et constater que la nomenclature évolue automatiquement.

## Phase 7 — moteur de validation

Objectif : détecter les incohérences avant impression ou livraison.

Créer un moteur de règles générique indépendant du RGIE.

Structure :

```text
RuleDefinition
    ├── id
    ├── version
    ├── domaine
    ├── severity
    ├── condition
    ├── message
    └── correctionHint
```

Le moteur ne doit pas contenir les règles directement dans l'interface utilisateur.

Critère de sortie : le logiciel peut produire une liste structurée d'avertissements et erreurs.

## Phase 8 — calculs électriques

Objectif : transformer le logiciel d'outil de dessin en outil de conception.

Ordre conseillé :
1. courant d'emploi ;
2. puissance ;
3. dimensionnement de protection ;
4. section de conducteur ;
5. chute de tension ;
6. capacités des canalisations ;
7. courant de court-circuit lorsque le périmètre le permet ;
8. sélectivité/coordination selon données disponibles.

Chaque résultat calculé doit conserver :
- formule ;
- unités ;
- hypothèses ;
- données d'entrée ;
- arrondis ;
- version du moteur ;
- statut du résultat.

## Phase 9 — imports et exports professionnels

Construire ensuite :
- PDF ;
- image ;
- SVG ;
- CSV/XLSX ;
- DXF ;
- DWG si bibliothèque stable disponible.

Les imports complexes PDF/DWG doivent être isolés dans un pipeline afin qu'une bibliothèque externe ou un changement de format ne contamine pas le modèle métier.

Les mises à jour 2026 de Trikker montrent que les performances et la fiabilité du chargement des gros fichiers, du PDF, du DWG et de l'impression constituent des sujets importants de production et de non-régression. [2][3]

## Phase 10 — qualité professionnelle

Construire :
- sauvegarde automatique ;
- récupération après crash ;
- journaux ;
- diagnostic ;
- système de mise à jour ;
- gestion de licence ;
- mode démonstration ;
- signature des binaires ;
- tests d'installation.

## Phase 11 — conformité réglementaire complète

Cette phase est à traiter comme un produit vivant. Le RGIE 2026 est publié par le SPF Économie et doit être traité comme une source réglementaire versionnée. [4]

Créer une base de règles versionnée :

```text
RegulationPack
    country = BE
    code = RGIE
    version = 2026
    effectiveDate = ...
    rules = [...]
```

Une mise à jour réglementaire doit pouvoir être déployée sans réécrire les composants graphiques.

## Phase 12 — distribution et exploitation

Prévoir :
- installateur ;
- mise à jour automatique ;
- sauvegarde de configuration ;
- télémétrie strictement optionnelle ;
- page de support ;
- système de rapports d'erreur ;
- licence ;
- documentation utilisateur.

---

# 3. Architecture technique recommandée

Une architecture desktop moderne en couches est recommandée.

```text
Presentation
    ↓
Application Services
    ↓
Domain
    ↓
Infrastructure
```

## 3.1 Domain

Aucune dépendance à l'interface graphique.

Contenu :
- modèles ;
- règles métier ;
- calculs ;
- validations ;
- événements métier.

## 3.2 Application

Orchestre les cas d'utilisation.

Exemples :

```text
CreateProject
AddCircuit
PlaceSymbol
ConnectCircuit
MoveSymbol
DeleteElement
GenerateMaterialList
ValidateProject
ExportProject
```

## 3.3 Presentation

Contient :
- fenêtres ;
- panneaux ;
- inspecteur ;
- palettes ;
- canvas ;
- dialogues.

## 3.4 Infrastructure

Contient :
- fichier projet ;
- base locale éventuelle ;
- PDF ;
- DWG/DXF ;
- impression ;
- licence ;
- mise à jour.

---

# 4. Découpage en modules logiciels

## M01 — Project Core

Responsable : gestionnaire de cycle de vie projet.

Fonctions :
- nouveau ;
- ouvrir ;
- fermer ;
- enregistrer ;
- enregistrer sous ;
- metadata ;
- version ;
- empreinte ;
- migration.

## M02 — Electrical Domain

Fonctions :
- circuits ;
- tableaux ;
- protections ;
- câbles ;
- appareils ;
- sources ;
- charges.

## M03 — Symbol Library

Fonctions :
- bibliothèque ;
- catégories ;
- favoris ;
- recherche ;
- symboles personnalisés ;
- propriétés graphiques ;
- propriétés métier.

## M04 — Canvas Engine

Fonctions :
- rendu ;
- interaction ;
- transformations ;
- sélection ;
- snapping ;
- clipboard.

## M05 — Schematic Editor

Fonctions spécifiques au schéma unifilaire.

## M06 — Position Editor

Fonctions spécifiques au plan de position.

## M07 — Cross Reference Engine

Synchronisation des vues.

## M08 — Calculation Engine

Calculs déterministes et traçables.

## M09 — Rule Engine

Validation et conformité.

## M10 — Material Engine

Nomenclature.

## M11 — Import/Export

Tous les formats externes.

## M12 — Document Composer

Création de documents paginés, cartouches, signatures, métadonnées.

## M13 — Persistence

Fichiers projet, sauvegardes, migrations.

## M14 — Licensing

Démo, licences et activation.

## M15 — Diagnostics

Logs, crash reports, diagnostics utilisateur.

---

# 5. Modèle de données à construire en premier

## 5.1 Identifiant universel

Tout objet métier doit avoir :

```text
id: UUID
createdAt
updatedAt
```

Ne jamais utiliser la position graphique comme identité.

## 5.2 Circuit

Attributs minimaux :

```text
id
reference
name
phase
voltage
frequency
protectionId
cableId
sourceBoardId
destinationBoardId
loads[]
positionRepresentations[]
calculationData
notes
```

## 5.3 Symbole

Séparer :

```text
SymbolDefinition
```

et

```text
SymbolInstance
```

La définition décrit le symbole. L'instance décrit son utilisation dans un projet.

## 5.4 Propriété graphique

```text
x
y
width
height
rotation
scale
visible
locked
layer
```

## 5.5 Propriété métier

```text
type
subtype
circuitId
deviceId
materialId
manufacturerData
```

---

# 6. Moteur graphique : ordre exact de développement

Développer le canvas dans cet ordre :

### Étape G01
Système de coordonnées monde.

### Étape G02
Transformation monde ↔ écran.

### Étape G03
Zoom centré sur le curseur.

### Étape G04
Panoramique.

### Étape G05
Objet graphique unique.

### Étape G06
Sélection.

### Étape G07
Déplacement.

### Étape G08
Rotation.

### Étape G09
Redimensionnement.

### Étape G10
Multi-sélection.

### Étape G11
Grille et snapping.

### Étape G12
Copier/coller.

### Étape G13
Undo/Redo.

### Étape G14
Rendu vectoriel.

### Étape G15
Hit testing.

### Étape G16
Clipping et performance.

Ne jamais commencer le dessin des symboles métier avant que G01 à G13 soient fiables.

---

# 7. Développement du schéma unifilaire

## 7.1 Pipeline utilisateur

```text
Nouveau projet
→ ajouter tableau
→ choisir source
→ ajouter départ
→ choisir protection
→ choisir câble
→ choisir charge
→ générer représentation
→ ajuster position
→ valider
→ imprimer
```

## 7.2 Fonctionnement interne

La création d'un circuit doit produire simultanément :

```text
Circuit métier
+
Symbol instances
+
Relations topologiques
```

Les lignes graphiques ne doivent pas être la seule source de vérité de la connexion électrique.

## 7.3 Topologie

Créer un graphe interne :

```text
Node
Edge
Terminal
Connection
```

Ce graphe servira ensuite aux calculs et aux validations.

---

# 8. Développement du plan de position

## 8.1 Types d'objets

Géométrie :
- mur ;
- porte ;
- fenêtre ;
- pièce ;
- escalier ;
- annotation.

Électricité :
- prise ;
- interrupteur ;
- point lumineux ;
- appareil ;
- tableau ;
- équipement spécial.

## 8.2 Import du fond

Le fond doit être traité comme un objet externe :

```text
ImportedPlan
    path/reference
    format
    scale
    offset
    rotation
    opacity
    locked
```

Ne pas convertir immédiatement tout le fond en objets natifs. Cela dégraderait les performances et complexifierait le modèle.

---

# 9. Synchronisation des deux vues

## 9.1 Référence unique

Exemple :

```text
Circuit C07
```

possède :

```text
SchematicRepresentation(C07)
PositionRepresentation(C07)
```

## 9.2 Cas à gérer

Créer :
- existe dans schéma uniquement ;
- existe dans plan uniquement ;
- existe dans les deux ;
- représentation supprimée ;
- élément métier supprimé ;
- doublon d'identifiant impossible.

## 9.3 UX

La recherche croisée doit être instantanée. Le manuel Trikker indique notamment un accès croisé par recherche entre schéma et plan. [1]

---

# 10. Undo / Redo

Ne pas sauvegarder uniquement l'état complet avant chaque action.

Préférer le pattern Command :

```text
Command
  execute()
  undo()
```

Exemples :

```text
CreateSymbolCommand
MoveSymbolCommand
RotateSymbolCommand
DeleteSymbolCommand
ChangePropertyCommand
ConnectCircuitCommand
```

Avantages :
- historique fiable ;
- actions groupables ;
- raccourcis ;
- tests simples ;
- sauvegarde possible de séquences si nécessaire.

---

# 11. Sauvegarde et robustesse

## 11.1 Sauvegarde explicite

`Ctrl+S` doit enregistrer le projet de manière atomique.

Processus :

```text
Projet courant
→ fichier temporaire
→ flush
→ vérification
→ remplacement atomique
```

## 11.2 Auto-save

Créer un fichier de récupération séparé.

Exemple :

```text
project.elx
project.autosave
project.bak
```

## 11.3 Crash recovery

Au redémarrage :

```text
autosave plus récent ?
→ proposer récupération
```

---

# 12. Moteur de calcul électrique

## 12.1 Architecture

Le moteur doit être pur autant que possible.

```text
Input Model
    ↓
Normalized Electrical Model
    ↓
Calculation Engine
    ↓
Calculation Result
```

## 12.2 Résultat calculé

Chaque résultat doit contenir :

```text
value
unit
status
formulaId
inputs[]
assumptions[]
warnings[]
engineVersion
```

## 12.3 Ne jamais cacher les hypothèses

Exemple :

```text
Chute de tension = 2.4 %

Entrées :
- longueur = 18 m
- courant = 16 A
- section = 2.5 mm²
- matériau = cuivre

Hypothèses :
- température de référence
- facteur de puissance
- configuration monophasée
```

---

# 13. Moteur de règles réglementaires

Le système doit distinguer :

```text
Règle
Résultat de règle
Explication
Correction suggérée
Référence réglementaire
```

## 13.1 Niveaux

```text
INFO
WARNING
ERROR
BLOCKING
```

## 13.2 Exemple

```text
RULE-CIRCUIT-PROTECTION-001

Condition : courant nominal de protection > capacité admissible
Résultat : ERROR
Message : protection potentiellement surdimensionnée
Action suggérée : vérifier la section et le mode de pose
```

## 13.3 Versionnement

Chaque règle possède :

```text
regulation = RGIE
version = 2026
article = ...
validFrom = ...
validTo = ...
```

Le Livre 1 du RGIE belge publié par le SPF Économie constitue la référence principale pour le périmètre basse et très basse tension. [4]

---

# 14. Nomenclature

Pipeline :

```text
Modèle projet
→ collecte des composants
→ normalisation
→ regroupement
→ comptage
→ tri
→ rendu
→ export
```

## Règles

Deux composants identiques doivent pouvoir être regroupés selon une clé métier contrôlée :

```text
materialType
manufacturer
reference
rating
variant
```

Ne pas regrouper simplement sur le nom affiché.

---

# 15. Import PDF / DWG / DXF

## Pipeline

```text
Fichier externe
→ détection format
→ lecture
→ normalisation géométrique
→ système de coordonnées
→ unités
→ conversion interne
→ aperçu
→ validation utilisateur
→ insertion projet
```

## Règles de sécurité

- ne jamais modifier directement le fichier source ;
- conserver les unités ;
- conserver le facteur d'échelle ;
- gérer les entités non supportées ;
- journaliser les conversions ;
- afficher les erreurs d'import sans perdre le projet.

Les versions récentes de Trikker accordent explicitement de l'attention à la fiabilité des ouvertures de fichiers, du PDF, du DWG et à la stabilité de l'impression. [2][3]

---

# 16. Impression et composition documentaire

La composition doit être séparée du dessin interactif.

Créer un `DocumentComposer` capable de prendre :

```text
Project
+
View configuration
+
Paper format
+
Scale
+
Margins
+
Title block
```

et produire :

```text
RenderedDocument
```

Cela évite d'avoir un moteur d'impression rempli de conditions spécifiques à l'interface.

---

# 17. UX à implémenter dans cet ordre

## Niveau 1

- nouveau projet ;
- ouvrir ;
- sauvegarder ;
- annuler ;
- sélectionner ;
- zoomer ;
- déplacer.

## Niveau 2

- palette ;
- propriétés ;
- recherche ;
- copier/coller ;
- duplication ;
- raccourcis.

## Niveau 3

- assistants ;
- validations ;
- synchronisation ;
- nomenclature ;
- export.

Le manuel Trikker utilise notamment des raccourcis pour passer du document au schéma, au plan, à la nomenclature et à l'impression, ce qui confirme l'intérêt d'une navigation très directe entre les grandes vues. [1]

---

# 18. Priorisation MVP

## MVP absolument nécessaire

```text
Projet
Schéma unifilaire
Bibliothèque de symboles
Circuits
Tableaux
Protections
Câbles
Plan de position simple
Lien schéma/plan
Sauvegarde
Impression PDF
```

## Version 1

Ajouter :

```text
Nomenclature
Import PDF
Import DXF
Validation documentaire
Autosave
Favoris
Symboles personnalisés
```

## Version 1.5

```text
Calculs électriques avancés
RGIE plus complet
DWG
Exports avancés
Domotique
PV
Batteries
Bornes de recharge
```

## Version 2

```text
Cloud
Synchronisation
Collaboration
Versioning avancé
Applications web/mobile
```

---

# 19. Backlog de développement détaillé

## Epic A — Fondations

### A01
Initialiser dépôt.

### A02
Créer solution et projets.

### A03
Configurer CI.

### A04
Créer système de logging.

### A05
Créer système de configuration.

### A06
Créer gestion des versions.

### A07
Créer gestion des erreurs.

## Epic B — Projet

### B01
Créer projet.

### B02
Modifier métadonnées.

### B03
Sauvegarder.

### B04
Ouvrir.

### B05
Sauvegarde automatique.

### B06
Migration ancien format.

## Epic C — Modèle électrique

### C01
Tableau.

### C02
Circuit.

### C03
Protection.

### C04
Câble.

### C05
Charge.

### C06
Source.

### C07
Connexions.

## Epic D — Canvas

### D01
Zoom.

### D02
Pan.

### D03
Sélection.

### D04
Déplacement.

### D05
Rotation.

### D06
Grille.

### D07
Snapping.

### D08
Copie.

### D09
Undo.

### D10
Redo.

## Epic E — Schéma

### E01
Palette.

### E02
Placement symbole.

### E03
Propriétés.

### E04
Connexion.

### E05
Repérage.

### E06
Textes.

### E07
Pagination.

### E08
Cartouche.

## Epic F — Plan

### F01
Pièces.

### F02
Murs.

### F03
Portes.

### F04
Fenêtres.

### F05
Symboles électriques.

### F06
Fond importé.

### F07
Échelle.

## Epic G — Synchronisation

### G01
Identifiants partagés.

### G02
Recherche croisée.

### G03
Propagation des propriétés.

### G04
Détection d'orphelins.

## Epic H — Matériel

### H01
Catalogue.

### H02
Association symbole/matériel.

### H03
Quantification.

### H04
Export.

## Epic I — Validation

### I01
Moteur de règles.

### I02
Rapport.

### I03
Références réglementaires.

### I04
Règles documentaires.

### I05
Règles électriques.

## Epic J — Calculs

### J01
Puissance.

### J02
Courant.

### J03
Section.

### J04
Protection.

### J05
Chute de tension.

### J06
Court-circuit.

## Epic K — Imports/exports

### K01
PDF.

### K02
SVG.

### K03
PNG.

### K04
CSV.

### K05
XLSX.

### K06
DXF.

### K07
DWG.

## Epic L — Professionnalisation

### L01
Licensing.

### L02
Demo mode.

### L03
Updater.

### L04
Crash report.

### L05
Installation.

### L06
Documentation.

---

# 20. Organisation Git

Branches recommandées :

```text
main
 develop
 feature/*
 bugfix/*
 release/*
 hotfix/*
```

## Pull request obligatoire

Toute fonctionnalité doit fournir :

```text
code
+ tests
+ documentation
+ migration si nécessaire
+ données de démonstration si nécessaire
```

Aucun code métier important directement dans les composants graphiques.

---

# 21. Stratégie de tests

## 21.1 Tests unitaires

Tester :
- calculs ;
- règles ;
- conversions ;
- sérialisation ;
- identifiants ;
- géométrie.

## 21.2 Tests d'intégration

Tester :

```text
Créer projet
→ créer circuit
→ sauvegarder
→ fermer
→ réouvrir
→ vérifier intégrité
```

## 21.3 Tests graphiques

Pour chaque version importante :

```text
projet de référence
→ rendu
→ capture
→ comparaison pixel / tolérance
```

## 21.4 Tests réglementaires

Créer un jeu de cas de référence validés par un expert métier.

Chaque cas possède :

```text
input
expectedResult
regulationReference
expectedMessage
```

---

# 22. Jeux de données de test

Créer au minimum :

### TEST-01
Maison simple : tableau + 5 circuits.

### TEST-02
Maison à plusieurs tableaux.

### TEST-03
Installation avec triphasé.

### TEST-04
Installation avec PV.

### TEST-05
Installation avec borne de recharge.

### TEST-06
Ancienne installation.

### TEST-07
Plan architectural complexe.

### TEST-08
DWG lourd.

### TEST-09
PDF multipage.

### TEST-10
Projet volontairement incohérent pour vérifier le moteur de validation.

---

# 23. Critères de sortie d'une phase

Chaque phase doit répondre à cinq questions :

1. Le code compile-t-il ?
2. Les tests passent-ils ?
3. Le projet peut-il être fermé et rouvert ?
4. Une démonstration reproductible existe-t-elle ?
5. Une documentation correspondante existe-t-elle ?

Si une réponse est non, la phase n'est pas terminée.

---

# 24. Definition of Done

Une fonctionnalité est terminée uniquement si :

```text
[ ] fonctionnelle
[ ] testée
[ ] documentée
[ ] persistante
[ ] compatible undo/redo si applicable
[ ] compatible avec les autres vues si applicable
[ ] sans fuite de ressources
[ ] sans erreur critique dans les logs
[ ] testée sur projet réel de référence
```

---

# 25. Équipe d'agents de développement

Une organisation agentique efficace peut être structurée ainsi :

## Agent 1 — Product Owner technique

Transforme le besoin en tickets et critères d'acceptation.

## Agent 2 — Architecte

Maintient l'architecture et interdit les dépendances incorrectes.

## Agent 3 — Domain Engineer

Développe les modèles et règles métier.

## Agent 4 — Graphics Engineer

Développe le moteur canvas.

## Agent 5 — Electrical Engineer

Développe calculs et modèle électrique.

## Agent 6 — Compliance Engineer

Maintient le moteur RGIE et les références.

## Agent 7 — UX Engineer

Travaille sur interactions et ergonomie.

## Agent 8 — Import/Export Engineer

Gère PDF, DXF, DWG, SVG et documents.

## Agent 9 — QA Engineer

Construit les tests automatiques et les projets de référence.

## Agent 10 — Release Engineer

Build, packaging, signature, installation et mises à jour.

## Agent 11 — Technical Writer

Maintient documentation utilisateur et développeur.

---

# 26. Flux agentique recommandé pour chaque ticket

```text
Besoin
  ↓
Analyse fonctionnelle
  ↓
Architecture
  ↓
Conception technique
  ↓
Implémentation
  ↓
Tests unitaires
  ↓
Tests d'intégration
  ↓
Validation graphique
  ↓
Documentation
  ↓
Review architecture
  ↓
Merge
```

Pour les fonctions réglementaires :

```text
Règle RGIE
→ interprétation métier
→ formalisation algorithmique
→ implémentation
→ test réglementaire
→ revue expert
→ publication
```

---

# 27. Ordre de construction conseillé pour une première version

Si une seule personne développe le produit, l'ordre suivant minimise le risque :

```text
1. Projet + sauvegarde
2. Modèle électrique
3. Canvas
4. Undo/Redo
5. Bibliothèque de symboles
6. Schéma unifilaire
7. Plan de position
8. Lien schéma/plan
9. PDF
10. Nomenclature
11. Validation documentaire
12. Calculs simples
13. Import PDF
14. Import DXF
15. Import DWG
16. RGIE avancé
17. Calculs avancés
18. Licence + updater
```

La raison est stratégique : cette séquence permet d'avoir un logiciel montrable très tôt tout en construisant les fondations nécessaires aux fonctionnalités complexes.

---

# 28. Ce qu'il ne faut surtout pas faire

## 28.1 Mettre le RGIE directement dans l'UI

Mauvais :

```python
if breaker_amp > 20:
    label.configure(...)
```

Bon :

```text
RuleEngine
→ Finding
→ UI affiche Finding
```

## 28.2 Stocker uniquement les dessins

Un schéma visuellement correct ne doit pas être le modèle électrique.

## 28.3 Mélanger import DWG et domaine électrique

Le module DWG doit être remplaçable.

## 28.4 Développer les calculs avant le modèle

Les calculs doivent consommer un modèle propre, pas lire directement les widgets.

## 28.5 Négliger les performances graphiques

Le logiciel doit rester utilisable avec de grands plans avant d'ajouter les fonctions secondaires.

---

# 29. Documentation à produire pendant le développement

La documentation ne doit pas être écrite à la fin.

À produire progressivement :

```text
01_Architecture.md
02_Domain_Model.md
03_File_Format.md
04_Canvas.md
05_Schematic.md
06_PositionPlan.md
07_Calculations.md
08_Rules.md
09_Imports.md
10_Exports.md
11_Testing.md
12_Release.md
13_User_Manual.md
14_Regulatory_Maintenance.md
```

---

# 30. Documentation du format de fichier

Le format doit être versionné dès le premier jour.

Exemple :

```json
{
  "format": "electric-project",
  "version": 1,
  "project": {},
  "installation": {},
  "schematics": [],
  "positionPlans": [],
  "materials": [],
  "metadata": {}
}
```

Toute évolution doit être accompagnée d'une migration :

```text
v1 → v2
v2 → v3
```

Jamais de rupture silencieuse.

---

# 31. Architecture de performance

Cibles recommandées :

- ouverture d'un projet courant < 2 s ;
- zoom/pan fluide sur projet courant ;
- déplacement d'un symbole sans reconstruction complète du modèle ;
- rendu différentiel ;
- chargement asynchrone des gros imports ;
- index de recherche pour les symboles et équipements.

Les performances doivent être mesurées avec des projets de référence, pas uniquement ressenties sur le poste du développeur.

---

# 32. Sécurité et intégrité

Minimum :

- validation des fichiers importés ;
- chemins sandboxés lorsque nécessaire ;
- contrôle des extensions ;
- pas d'exécution de code depuis un fichier projet ;
- signature de l'application distribuée ;
- sauvegardes atomiques ;
- vérification des fichiers de mise à jour ;
- protection des clés de licence.

---

# 33. Stratégie de livraison

## Alpha 0.1

Schéma basique.

## Alpha 0.2

Plan de position.

## Alpha 0.3

Synchronisation.

## Beta 0.5

Nomenclature + PDF + validation de base.

## RC 0.9

Calculs, stabilité, import/export et tests réels.

## 1.0

Produit commercialisable avec documentation, installateur et support.

---

# 34. Jalons majeurs

| Jalon | Résultat |
|---|---|
| M0 | Application vide qui se lance |
| M1 | Projet sauvegardable |
| M2 | Premier symbole éditable |
| M3 | Premier schéma complet |
| M4 | Premier plan de position |
| M5 | Synchronisation schéma/plan |
| M6 | Premier PDF professionnel |
| M7 | Nomenclature automatique |
| M8 | Première validation réglementaire |
| M9 | Premiers calculs électriques |
| M10 | Import PDF/DXF |
| M11 | Beta métier |
| M12 | Release 1.0 |

---

# 35. Première semaine de développement

## Jour 1

- créer repository ;
- créer solution ;
- configurer CI ;
- créer documentation architecture.

## Jour 2

- créer modèle Project ;
- créer modèle Installation ;
- créer modèle Circuit ;
- créer sérialisation.

## Jour 3

- sauvegarde ;
- ouverture ;
- version de fichier ;
- tests.

## Jour 4

- canvas minimal ;
- coordonnées ;
- zoom ;
- pan.

## Jour 5

- premier symbole ;
- sélection ;
- déplacement ;
- sauvegarde de la position.

Objectif : vendredi soir, l'application doit permettre de lancer un projet, placer un symbole, le déplacer et retrouver le même état après réouverture.

---

# 36. Première vertical slice complète

La première vraie tranche fonctionnelle doit être minuscule mais complète :

```text
Créer projet
→ tableau principal
→ circuit C1
→ protection 16 A
→ câble 3G2.5
→ prise
→ représentation dans schéma
→ représentation dans plan
→ sauvegarder
→ rouvrir
→ imprimer PDF
```

Cette tranche permet déjà de tester presque toutes les fondations : modèle, graphisme, persistance, synchronisation et document.

---

# 37. Décision d'architecture importante

Le logiciel doit pouvoir évoluer vers macOS, web ou autre environnement sans réécrire le moteur métier.

Donc :

```text
Domain
Application
Calculation
Rules
Persistence abstraction
```

ne doivent dépendre d'un framework graphique précis.

Le moteur d'interface doit pouvoir être remplacé.

---

# 38. Références actuelles utilisées pour le guide

[1] Trikker, manuel utilisateur et raccourcis clavier, notamment navigation Document / Schéma unifilaire / Plan de position / Liste des matériaux / Impression et interactions d'édition.
https://www.trikker.be/web/content/1460?download=true&unique=2f6cfdcab55a265c47f1a75553136a82c5715ae8

[2] Trikker 1.5.101, amélioration de l'ouverture des fichiers, de l'import AutoCAD/PDF et de l'impression.
https://www.trikker.be/en/blog/news-2/trikker-1-5-101-is-available-10

[3] Trikker 1.5.103 et 1.5.104, fiabilité PDF, affichage haute résolution et correction de rendu du schéma unifilaire.
https://www.trikker.be/fr/blog/actualites-2

[4] SPF Économie, RGIE - Livres 1, 2 et 3, édition 2026, dont le Livre 1 relatif aux installations à basse et très basse tension.
https://economie.fgov.be/fr/publications/reglement-general-sur-les

[5] Trikker, présentation fonctionnelle : schéma unifilaire, plan de position, import DWG, export/impression et liste de matériel.
https://www.trikker.be/fr

---

# 39. Conclusion opérationnelle

Le projet doit être traité comme la construction de **quatre moteurs liés** :

```text
1. Moteur de données électriques
2. Moteur graphique
3. Moteur de calcul/validation
4. Moteur documentaire
```

Le schéma unifilaire et le plan de position sont deux clients du même modèle métier.

Le point de bascule du projet est la synchronisation entre ces quatre moteurs. Une fois ce socle solide, les fonctionnalités avancées comme le RGIE détaillé, les calculs, la nomenclature, les imports complexes et la génération de documents deviennent des extensions contrôlées plutôt que des rustines accumulées autour d'un éditeur de dessin.

La stratégie recommandée est donc : **modèle → canvas → schéma → plan → synchronisation → documents → validation → calculs → imports/exports → professionnalisation → cloud**.
