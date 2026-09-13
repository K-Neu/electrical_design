# Cahier de spécification complet
## Logiciel de conception de schémas électriques et plans de position pour installations basse tension

**Référence fonctionnelle :** Trikker, logiciel belge de schéma unifilaire et plan de position  
**Périmètre cible :** Belgique, RGIE/AREI, installations domestiques et non-domestiques basse tension / très basse tension  
**État documentaire :** conception fonctionnelle cible, fondée sur les capacités observées de Trikker et sur le RGIE version applicable en 2026  
**Date de référence :** 8 septembre 2026

---

# 1. Objet du logiciel

Le logiciel a pour objectif de permettre à un électricien, un bureau d'études ou un utilisateur expérimenté de créer, modifier, contrôler, documenter et exporter une installation électrique sous forme de :

- schéma unifilaire ;
- schéma de circuits lorsque requis ;
- plan de position ;
- plan de situation / plan d'étage ;
- documentation administrative ;
- liste des circuits ;
- nomenclature / liste de matériel ;
- dossier exportable destiné au contrôle, au client et à l'archivage.

Le logiciel doit être pensé comme un **éditeur graphique à modèle électrique structuré**, et non comme un simple programme de dessin. Chaque symbole graphique doit correspondre à une entité métier capable de porter des propriétés électriques, documentaires et de localisation.

L'objectif est de reproduire le cœur fonctionnel observé chez Trikker, tout en construisant une base plus moderne, portable et extensible.

> **Important :** la conformité réglementaire ne doit jamais être présentée comme garantie automatique. Le logiciel fournit une aide à la conception, un contrôle de cohérence et une génération documentaire. La validation finale de la conformité dépend de l'installation réelle, du contexte réglementaire applicable et du contrôle par un organisme agréé.

---

# 2. Références fonctionnelles et réglementaires

## 2.1 Référence Trikker

Trikker se présente comme un logiciel permettant de créer un schéma unifilaire et un plan de position, d'importer des plans AutoCAD/DWG, d'imprimer ou exporter les documents et de générer automatiquement une liste de matériel. Le guide utilisateur documente également une bibliothèque de symboles, les propriétés des symboles, les favoris, les symboles personnalisés, les adresses domotiques, les symboles libres, les pages multiples, l'import d'images/DXF/DWG/PDF, le redimensionnement et la rotation, la recherche croisée entre schéma et plan, l'export Excel de la liste de matériel, les sauvegardes et plusieurs intégrations domotiques. [S1][S2][S3]

La documentation Trikker indique notamment que les symboles placés sur le schéma unifilaire peuvent être ajoutés sur le plan de position et que les deux représentations restent synchronisées. [S1]

Les versions récentes publiées en 2026 indiquent une évolution du moteur d'import PDF/DWG, une amélioration du rendu haute résolution, la conservation des couleurs de conducteurs et une migration vers .NET 10 dans la branche 1.5.99. [S4][S5][S6]

## 2.2 RGIE / AREI

Le Livre 1 du RGIE 2026 couvre les installations à basse tension et très basse tension. La version 06 est associée à l'arrêté royal du 6 octobre 2025, applicable à partir du 1er avril 2026. [S7]

Pour les installations domestiques nouvelles ou faisant l'objet d'une modification ou extension importante, les schémas unifilaires et plans de position doivent être établis. Ils portent notamment les informations d'identification de l'installation, du responsable de l'exécution et les signatures/dates requises lors du contrôle. [S8]

Le RGIE définit également les symboles graphiques pour les schémas et plans de position domestiques. Lorsqu'aucun symbole approprié n'est enregistré dans le tableau réglementaire, un autre symbole clairement identifiable et expliqué dans la légende peut être utilisé. [S8][S9]

Le schéma unifilaire doit notamment reprendre les caractéristiques des canalisations, les modes de pose, les dispositifs différentiels et de protection contre les surintensités, interrupteurs, boîtes de connexion et de dérivation, prises, points lumineux, appareils fixes et sources. [S10]

Le plan de position doit notamment localiser tableaux, boîtes, prises, points lumineux, interrupteurs, appareils fixes et sources mentionnés au schéma. [S10]

Pour les installations non-domestiques, le périmètre documentaire est plus riche : schémas de circuits, plans de position, plans de position des prises de terre, documents d'influences externes et, selon le cas, zonage, sécurité, criticité et autres annexes. [S10]

---

# 3. Principes directeurs

1. **Modèle métier unique** : le dessin est une vue du modèle, pas la source unique de vérité.
2. **Édition graphique fluide** : placement par points de connexion, accrochage à la grille, rotation, déplacement, duplication et édition multi-sélection.
3. **Synchronisation bidirectionnelle** : un même élément électrique doit pouvoir être retrouvé depuis le schéma et depuis le plan.
4. **Traçabilité** : version, date, auteur, modifications et règles de validation doivent être conservées.
5. **Règles réglementaires versionnées** : le moteur de règles ne doit pas être figé dans l'interface.
6. **Exports déterministes** : un même projet et les mêmes paramètres doivent produire le même résultat.
7. **Hors-ligne prioritaire** : l'édition principale doit fonctionner sans connexion permanente.
8. **Interopérabilité** : PDF, images, DXF/DWG, Excel/CSV, SVG/PDF d'édition, et format projet documenté.
9. **Performance graphique** : les plans importés volumineux doivent rester manipulables.
10. **Pas de fausse certification** : l'outil signale des incohérences et produit un dossier exploitable, mais ne se substitue pas à l'organisme agréé.

---

# 4. Profils utilisateurs

## 4.1 Électricien installateur

Besoins : rapidité, bibliothèque de symboles, saisie minimale, plans conformes, impression immédiate, réutilisation de modèles.

## 4.2 Bureau d'études

Besoins : projets complexes, multi-tableaux, multi-pages, calculs et contrôles, nomenclature détaillée, versions, annotations, imports architecturaux.

## 4.3 Contrôleur / inspecteur

Besoins : lecture claire, repérage univoque, correspondance schéma/terrain, version/date, signatures et dossier complet.

## 4.4 Client / propriétaire

Besoins : documents imprimables, lisibles, archivables, éventuellement lecture seule.

## 4.5 Établissement scolaire

Besoins : licence pédagogique, exercices, modèles et export avec filigrane si souhaité.

---

# 5. Cas d'utilisation principaux

### UC01 - Nouveau projet
Créer un document, choisir le type d'installation, renseigner l'adresse, le responsable et les paramètres réglementaires.

### UC02 - Dessiner un schéma unifilaire
Créer tableaux, protections, câbles, commandes, appareils et charges en les connectant par points d'ancrage.

### UC03 - Dessiner un plan de position
Dessiner ou importer le plan d'étage puis positionner les objets électriques.

### UC04 - Lier schéma et plan
Retrouver un même équipement depuis les deux vues et maintenir son identifiant commun.

### UC05 - Importer un plan architectural
Importer PDF, DXF/DWG ou image, choisir page/mise en page/calques, découper, recolorer et verrouiller le fond.

### UC06 - Produire la nomenclature
Générer automatiquement une liste des composants et quantités, avec export Excel/CSV.

### UC07 - Contrôler le projet
Analyser identifiants, propriétés manquantes, incohérences entre schéma et plan, règles de câblage, protections et documentation.

### UC08 - Imprimer / exporter
Générer schéma, plans, liste des circuits, légende, nomenclature et dossier PDF.

### UC09 - Réviser un projet
Créer une nouvelle version en conservant les versions antérieures.

### UC10 - Restaurer une sauvegarde
Rouvrir une version antérieure automatiquement créée ou manuellement archivée.

### UC11 - Créer un symbole favori
Enregistrer une configuration de symbole fréquemment utilisée.

### UC12 - Utiliser un symbole personnalisé
Importer ou construire une représentation graphique propre et documentée.

---

# 6. Architecture fonctionnelle de l'application

L'application est structurée autour de modules indépendants :

- **Core Project Model** : modèle de projet et persistance.
- **Electrical Model** : circuits, protections, canalisations, sources, charges.
- **Symbol Engine** : bibliothèque, propriétés, rendu et points de connexion.
- **Schematic Editor** : édition du schéma unifilaire/multifilaire.
- **Position Plan Editor** : dessin/import du plan et implantation des symboles.
- **Rules Engine** : règles RGIE et contrôles de cohérence.
- **Calculation Engine** : puissances, courants, chute de tension, protections, courts-circuits et paramètres associés selon le périmètre retenu.
- **Bill of Materials** : nomenclature.
- **Print & Export Engine** : PDF, image, Excel/CSV, SVG et formats interopérables.
- **Document Admin** : cartouche, client, installateur, organisme de contrôle, signatures et versions.
- **Import Engine** : image, PDF, DXF/DWG.
- **History & Backup** : undo/redo, snapshots, autosave.
- **Settings** : langue, bibliothèque, conventions graphiques, préférences d'impression.
- **Licensing** : démonstration, licence utilisateur, licences pédagogiques/professionnelles.
- **Integration API** : futures intégrations domotiques, ERP, devis, cloud.

---

# 7. Interface utilisateur

## 7.1 Fenêtre principale

Disposition cible :

- barre de menus ;
- barre d'outils contextuelle ;
- panneau bibliothèque ;
- panneau propriétés ;
- zone de dessin ;
- onglets des vues ;
- barre d'état ;
- indicateur de validation ;
- indicateur de sauvegarde ;
- zoom et coordonnées.

## 7.2 Vues principales

- Document
- Schéma unifilaire
- Plan de position
- Liste de matériel
- Contrôle / Validation
- Impression / Export
- Configuration

## 7.3 Modes d'édition

Chaque éditeur utilise au minimum :

- **Dessin** : ajoute des éléments depuis la bibliothèque.
- **Sélection** : sélectionne et modifie.
- **Déplacement** : déplace les éléments.
- **Panoramique** : déplace la feuille.
- **Zoom** : agrandit/réduit.
- **Annotation** : texte, ligne, rectangle, repère, cote.

Le comportement s'inspire de Trikker : les points de connexion doivent être visuellement repérables, le placement doit être rapide et la bibliothèque doit supporter des ensembles de symboles. [S2]

---

# 8. Gestion du projet et documents administratifs

## 8.1 Métadonnées du projet

Champs :

- identifiant du projet ;
- numéro de dossier ;
- nom du projet ;
- adresse ;
- unité / bâtiment ;
- type d'installation ;
- date de création ;
- date de modification ;
- numéro de version ;
- auteur ;
- responsable d'exécution ;
- entreprise ;
- numéro TVA ;
- client/propriétaire ;
- organisme de contrôle ;
- langue ;
- commentaires ;
- statut : brouillon, à vérifier, prêt au contrôle, contrôlé, archivé.

## 8.2 Cartouche

Le cartouche doit être paramétrable et intégrer automatiquement les métadonnées pertinentes sur chaque page. Trikker place notamment les données administratives et le logo dans ses impressions. [S2]

## 8.3 Versioning

Chaque publication de plan génère un identifiant de version :

`PROJECT-2026-000123 / v03 / 2026-09-08`

L'historique doit stocker : auteur, date, résumé, hash de version, statut et commentaire.

---

# 9. Modèle électrique de données

Le logiciel doit stocker une représentation structurée indépendante du rendu graphique.

## 9.1 Entités principales

### Project
Contient les métadonnées et collections de documents.

### ElectricalInstallation
Décrit le type d'installation, l'alimentation, le schéma de mise à la terre, les tensions et paramètres généraux.

### DistributionBoard
Tableau de répartition/manœuvre avec identifiant, tension, source, protection amont et paramètres de court-circuit si disponibles.

### Circuit
Circuit élémentaire avec lettre, nom, tableau source, protection, canalisation, mode de pose, charge et éléments terminaux.

### ElectricalSymbolInstance
Instance graphique d'un élément électrique. Contient référence au type de symbole, propriétés, identifiant métier et coordonnées dans une vue.

### Cable / Conduit / ConductorSet
Décrit type, nombre de conducteurs, section, matériau, isolation, pose, longueur et repérage.

### ProtectionDevice
Disjoncteur, fusible, différentiel, parafoudre ou protection spécialisée.

### Load / FixedAppliance
Charge ou appareil fixe avec puissance, courant, cos phi, catégorie et circuit d'alimentation.

### Source
Réseau, transformateur, photovoltaïque, onduleur, batterie, générateur, etc.

### Command / HomeAutomation
Interrupteur, bouton-poussoir, capteur, thermostat, relais, adresse logique et relations d'entrée/sortie.

### PositionPlan
Page physique comportant fond, murs, portes, fenêtres, objets électriques et annotations.

### SchematicPage
Page logique du schéma unifilaire/multifilaire.

### MaterialItem
Ligne de nomenclature avec article, description, fabricant/référence éventuelle, quantité, unité et provenance.

### RuleFinding
Résultat d'un contrôle : niveau, code, message, objet concerné, référence réglementaire et état.

---

# 10. Identifiants et repérage

Pour les installations domestiques, le logiciel doit prendre en charge le système de repérage du RGIE : circuits par lettre majuscule et points lumineux, prises et unités de commande par numéro d'ordre dans le circuit. Le plan de position doit reprendre les mêmes références. [S8]

Exemple :

- Circuit : `B`
- Prise : `B3`
- Point lumineux : `B1`
- Interrupteur commandant `B1` : `B1` ou référence associée selon la convention choisie.

Le système doit supporter :

- génération automatique ;
- lettres forcées ;
- préfixes de tableau ;
- renumérotation ;
- gel des identifiants ;
- détection de doublon ;
- références croisées.

Trikker permet déjà les lettres spécifiques, préfixes de tableau et noms de circuits. [S1]

---

# 11. Bibliothèque de symboles

## 11.1 Principe

La bibliothèque doit être organisée par catégories :

- généralités ;
- tableaux ;
- alimentation ;
- protections ;
- différentiels ;
- interrupteurs ;
- prises ;
- éclairage ;
- appareils fixes ;
- chauffage ;
- moteurs ;
- sources ;
- photovoltaïque ;
- batteries ;
- bornes de recharge ;
- domotique ;
- télécommunication ;
- sécurité ;
- annotations ;
- symboles personnalisés.

Le RGIE 2026 contient notamment des symboles pour courant AC/DC, protections, différentiels, interrupteurs, prises, appareils d'utilisation, transformateurs, panneaux solaires, onduleurs, hacheurs et éléments domotiques. [S9]

## 11.2 Propriétés d'un symbole

Une instance doit pouvoir exposer :

- nom ;
- catégorie ;
- identifiant ;
- référence circuit ;
- adresse domotique ;
- fonction domotique ;
- angle ;
- position du texte ;
- couleur ;
- nombre ;
- type ;
- caractéristiques électriques ;
- fabricant ;
- référence fabricant ;
- article de nomenclature ;
- commentaires ;
- état de verrouillage ;
- statut ancien/nouveau ;
- présence sur le schéma ;
- présence sur le plan.

Trikker permet de transformer un symbole de base en variant par modification de propriétés, par exemple prise simple/multiple, interrupteur et câble. [S2]

## 11.3 Favoris

Le système de favoris permet de mémoriser des configurations fréquemment utilisées. Les favoris doivent persister entre les sessions et pouvoir être regroupés par catégories.

## 11.4 Symboles personnalisés

Deux mécanismes :

1. symbole graphique utilisateur ;
2. symbole métier basé sur un type existant avec propriétés personnalisées.

Un symbole personnalisé doit obligatoirement avoir : nom, description, icône, points de connexion, règles de rotation et comportement d'export.

---

# 12. Moteur de schéma unifilaire

## 12.1 Fonctionnalités

- ajout par point de connexion ;
- connexions automatiques ;
- branches ;
- déplacement d'un symbole ;
- déplacement d'une branche ;
- réorganisation avant/arrière ;
- duplication ;
- copier/coller ;
- suppression ;
- rotation ;
- édition de propriétés ;
- texte libre ;
- lignes de dessin ;
- pages multiples ;
- recherche vers le plan de position ;
- sélection multiple ;
- alignement ;
- grille ;
- accrochage.

Trikker documente notamment les déplacements par ordre dans un circuit, le déplacement libre, la copie de groupes de symboles, les lignes dessinées et le dessin libre. [S1][S4]

## 12.2 Topologie

Le schéma ne doit pas être une simple image. Chaque branche doit être un lien topologique entre ports d'entités.

Exemple conceptuel :

`Source -> Interrupteur général -> DDR -> Disjoncteur -> Câble -> Boîte -> Prise`

Le moteur doit refuser ou signaler :

- branche orpheline ;
- port incompatible ;
- plusieurs alimentations interdites ;
- sortie vers sortie lorsque le type de port l'interdit ;
- circuit sans protection si une protection est requise dans le profil actif.

---

# 13. Moteur de plan de position

## 13.1 Dessin natif

Le logiciel doit permettre de construire un plan simple par :

- murs ;
- segments ;
- fenêtres ;
- portes ;
- portes de garage ;
- escaliers ;
- rectangles ;
- zones ;
- cotes ;
- textes ;
- images ;
- symboles électriques.

Trikker affiche pendant le dessin des murs la longueur, largeur et angle, propose l'accrochage à la grille et l'aimantation aux murs. [S2][S11]

## 13.2 Import de plans

Formats à supporter :

- PDF ;
- DXF ;
- DWG ;
- PNG ;
- JPEG ;
- BMP ;
- TIFF ;
- SVG à terme ;
- EMF/WMF si la plateforme native l'autorise.

Trikker documente l'import d'images et de DXF/DWG/PDF, la sélection des pages/mises en page, le filtrage des calques, la recoloration, la découpe et le verrouillage du fond. [S12]

## 13.3 Fond verrouillable

Un plan importé peut être verrouillé afin d'empêcher les déplacements accidentels. [S12]

## 13.4 Transparence

Le plan de référence peut être affiché en transparence afin de redessiner un plan par-dessus, fonctionnalité explicitement documentée par Trikker. [S13]

---

# 14. Synchronisation schéma ↔ plan

Chaque composant électrique doit avoir une identité métier unique.

Exemple :

```text
Circuit B
  ├── B1 Point lumineux
  ├── B2 Prise
  └── B3 Prise
```

Le symbole `B2` placé sur le schéma et celui placé sur le plan ne sont pas deux équipements métier distincts. Ce sont deux représentations du même objet.

Fonctions obligatoires :

- rechercher sur le schéma depuis le plan ;
- rechercher sur le plan depuis le schéma ;
- afficher les objets déjà placés ;
- signaler ceux absents ;
- interdire un double placement physique d'un même objet ;
- montrer les différences de propriétés ;
- permettre le changement de référence à partir d'un seul endroit.

Trikker applique déjà une logique de correspondance entre symbole logique et représentation sur plan, avec une fonction de recherche croisée. [S2][S14]

---

# 15. Symboles libres / mode devis

Le mode « symboles libres » doit permettre de placer des éléments sur le plan avant qu'ils soient rattachés à un circuit logique. Ces éléments restent visibles dans la nomenclature et peuvent être ensuite associés au schéma unifilaire. [S13]

Cas d'usage :

- relevé d'une installation existante ;
- préparation d'un devis ;
- réunion client ;
- placement rapide avant structuration électrique.

Le logiciel doit distinguer :

- `FREE_UNLINKED` ;
- `LINKED`; 
- `LOGICAL_ONLY`.

---

# 16. Domotique

## 16.1 Adresses logiques

Les objets domotiques peuvent partager une adresse logique pour représenter un lien entre commande et actionneur. Trikker utilise cette propriété pour faire apparaître des références croisées entre entrée et sortie. [S14]

## 16.2 Module multi-sorties

Un module domotique peut contenir plusieurs sorties. Sa largeur graphique doit s'adapter automatiquement au nombre de sorties. Les noms de sortie sont personnalisables. [S15]

## 16.3 Extensibilité

Le modèle doit prévoir :

- entrées ;
- sorties ;
- groupes ;
- adresses ;
- scènes ;
- temporisations ;
- détection ;
- commandes sans fil ;
- bus.

Une API de plugin doit permettre à terme d'intégrer des systèmes externes sans modifier le cœur.

---

# 17. Gestion de l'échelle et de l'impression

## 17.1 Échelle automatique

Le plan doit pouvoir être ajusté automatiquement au format de page. Trikker utilise des zones guides A4/A3 et adapte l'échelle afin que les éléments restent dans la page. [S16]

## 17.2 Échelle manuelle

L'utilisateur doit pouvoir définir par page :

- échelle ;
- taille des symboles ;
- taille des textes ;
- orientation ;
- format papier ;
- marges.

## 17.3 Pagination du schéma

Deux modes :

- division automatique ;
- division manuelle à un point défini.

Le résultat doit éviter des symboles illisibles et conserver les connexions compréhensibles. [S16]

---

# 18. Impression et exports

## 18.1 Documents

Le moteur doit pouvoir produire :

- schéma unifilaire ;
- schéma multifilaire ;
- plan de position ;
- liste des circuits ;
- nomenclature ;
- légende ;
- rapport de validation ;
- page de garde ;
- dossier PDF consolidé.

## 18.2 Formats

### PDF
Prioritaire, avec texte vectoriel et traits nets.

### SVG
Recommandé pour l'édition web et l'export vectoriel.

### PNG/JPEG
Pour visualisation rapide.

### Excel/CSV
Pour liste de matériel et données d'objets.

Trikker exporte sa liste de matériel vers Microsoft Excel et peut inclure une miniature des symboles. [S17]

### DXF/DWG
Import prioritaire. Export DXF/DWG à prévoir en phase ultérieure selon les contraintes de licence des bibliothèques techniques utilisées.

---

# 19. Liste de matériel / nomenclature

La nomenclature est calculée automatiquement à partir du modèle électrique et du plan. Trikker génère sa liste au fil du dessin et permet impression ou export Excel. [S17]

Chaque ligne doit contenir au minimum :

- code interne ;
- description ;
- catégorie ;
- référence fabricant ;
- fabricant ;
- unité ;
- quantité ;
- source : schéma, plan, symbole libre ou manuel ;
- circuit/tableau ;
- commentaire ;
- lien catalogue éventuel.

Fonctions avancées :

- regroupement par référence ;
- regroupement par circuit ;
- regroupement par tableau ;
- variantes commerciales ;
- prix unitaire ;
- coefficient de perte ;
- export devis ;
- verrouillage d'une ligne manuelle.

---

# 20. Moteur de validation réglementaire

## 20.1 Architecture

La conformité doit être traitée par un moteur de règles versionné :

```text
RulePack RGIE-2026
  ├── Identification
  ├── Schéma unifilaire
  ├── Plan de position
  ├── Repérage
  ├── Canalisations
  ├── Protections
  ├── Mise à la terre
  ├── DDR
  ├── Surtensions
  ├── Locaux spéciaux
  ├── Sources
  ├── Sécurité
  └── Documentation
```

## 20.2 Niveaux

- `ERROR` : incompatibilité grave ou donnée indispensable manquante.
- `WARNING` : incohérence ou contrôle manuel nécessaire.
- `INFO` : recommandation/documentation.
- `PASS` : règle vérifiée.
- `NOT_APPLICABLE` : règle hors périmètre du projet.
- `UNKNOWN` : information insuffisante pour conclure.

## 20.3 Exemple de résultat

```json
{
  "rule_id": "RGIE-3.1.2.1-A",
  "severity": "ERROR",
  "object_id": "circuit-B",
  "message": "Le responsable d'exécution n'est pas renseigné.",
  "source": "RGIE Livre 1, section 3.1.2.1",
  "status": "OPEN"
}
```

## 20.4 Règles documentaires prioritaires

Les contrôles doivent notamment vérifier :

- adresse présente ;
- numéro/version/date du document ;
- responsable d'exécution ;
- numéro TVA lorsque applicable ;
- identification des circuits ;
- tension/nature du courant ;
- correspondance schéma-plan ;
- présence des éléments exigés ;
- identification des tableaux ;
- symboles définis ;
- gestion des parties anciennes ;
- sources identifiées.

Ces éléments reposent sur les prescriptions 2026 du Livre 1. [S8][S10]

---

# 21. Moteur de calcul électrique

Le moteur de calcul doit être séparé du rendu graphique et du moteur de règles.

## 21.1 Calculs de base

Supporter :

- puissance active P ;
- puissance réactive Q ;
- puissance apparente S ;
- courant I ;
- facteur de puissance cos phi ;
- tension ;
- chute de tension ;
- résistance ;
- impédance ;
- énergie estimée.

## 21.2 Monophasé / triphasé

Le moteur doit distinguer explicitement :

- DC ;
- AC monophasé ;
- AC triphasé.

Ne jamais déduire silencieusement le nombre de phases à partir d'un texte libre.

## 21.3 Canalisations

Paramètres :

- matériau conducteur ;
- isolation ;
- section ;
- nombre de conducteurs ;
- longueur ;
- mode de pose ;
- température ;
- regroupement ;
- nombre de circuits voisins ;
- conducteur PE ;
- neutre ;
- présence éventuelle d'harmoniques ;
- courant admissible issu de la table de référence.

## 21.4 Protection contre surintensités

Le moteur doit pouvoir confronter :

- courant de charge ;
- courant admissible de la canalisation ;
- courant nominal de protection ;
- pouvoir de coupure ;
- conditions de déclenchement.

## 21.5 Différentiels

Le modèle doit distinguer :

- courant nominal ;
- sensibilité différentielle ;
- type ;
- temporisation ;
- circuits protégés ;
- ordre d'installation.

Le moteur doit gérer les cas nécessitant une validation contextuelle et signaler `UNKNOWN` lorsque les données manquent plutôt que d'afficher « conforme ».

## 21.6 Chute de tension

Calcul configurable selon :

- résistance du conducteur ;
- réactance ;
- longueur ;
- courant ;
- cos phi ;
- nombre de phases.

Afficher à la fois :

- volts ;
- pourcentage ;
- valeur limite ;
- statut.

## 21.7 Court-circuit

Pour les environnements non-domestiques, prévoir une gestion des courants de court-circuit présumés et de leur propagation entre tableaux. Le RGIE 2026 prévoit notamment l'indication de valeurs de court-circuit présumées maximales dans certains schémas non-domestiques. [S10]

---

# 22. Locaux spéciaux et influences externes

Le logiciel doit permettre d'associer des influences externes à un local ou à une zone :

- présence d'eau ;
- présence de corps solides ;
- conditions environnementales ;
- contact avec potentiel de terre ;
- évacuation ;
- matériaux ;
- zones particulières.

Le Livre 1 2026 comprend notamment une partie dédiée aux influences externes et des chapitres spécifiques pour baignoires/douches, piscines, saunas, chantiers/installations extérieures et enceintes conductrices exiguës. [S7]

Pour les locaux spéciaux, le moteur doit fonctionner avec des règles contextuelles et proposer les paramètres nécessaires au calcul ou au contrôle.

---

# 23. Photovoltaïque, batteries et mobilité électrique

Le modèle doit nativement prévoir :

- panneaux photovoltaïques ;
- chaînes PV ;
- onduleur ;
- batterie ;
- convertisseur ;
- bornes de recharge de véhicules électriques ;
- sources AC et DC ;
- flux bidirectionnels.

Le RGIE 2026 a intégré des modifications relatives aux systèmes en courant continu, notamment les schémas de mise à la terre DC et leurs implications sur les mesures de protection et le choix du matériel. [S18]

Le moteur doit donc considérer le type de courant comme une donnée fondamentale du modèle.

---

# 24. Gestion des anciennes installations

Le logiciel doit permettre de marquer une partie du réseau comme « partie ancienne » et d'en conserver la référence documentaire. Pour une partie ancienne commencée avant le 1er octobre 1981 et apparaissant sur un schéma unifilaire domestique, le RGIE 2026 prévoit une indication spécifique. [S8]

Le système doit pouvoir stocker :

- année connue ;
- date estimée ;
- statut ancien ;
- justification ;
- règles spécifiques associées ;
- commentaire de contrôle.

---

# 25. Undo / Redo / autosave

## 25.1 Undo/Redo

Toutes les modifications significatives sont des commandes annulables :

- création ;
- suppression ;
- déplacement ;
- rotation ;
- changement de propriété ;
- import ;
- collage ;
- réorganisation.

Trikker documente un undo/redo et certaines frontières d'historique lors de changement d'écran ou de page. Le produit cible doit améliorer cela avec un historique global de projet. [S17]

## 25.2 Sauvegarde automatique

Prévoir :

- autosave configurable ;
- snapshots horodatés ;
- récupération après crash ;
- récupération du dernier état valide ;
- avertissement de document non sauvegardé ;
- nettoyage configurable ;
- archivage manuel.

Trikker prévoit déjà un système de sauvegarde périodique et de restauration de versions antérieures. [S19]

---

# 26. Format de fichier projet

Le format recommandé est un conteneur ZIP versionné avec :

```text
project.elecproj
├── manifest.json
├── project.json
├── installation.json
├── symbols.json
├── rules.json
├── pages/
│   ├── schematic-001.json
│   ├── position-001.json
│   └── position-002.json
├── assets/
│   ├── drawings/
│   ├── images/
│   └── custom-symbols/
├── calculations/
│   └── results.json
└── history/
    └── snapshots/
```

Le manifeste doit contenir :

- version du format ;
- version de l'application ;
- version RGIE active ;
- hash des ressources ;
- date ;
- identifiant du projet ;
- compatibilité minimale.

Le format doit permettre une migration automatique des anciennes versions.

---

# 27. API interne et modèle d'événements

Chaque modification génère un événement métier :

```text
SymbolAdded
SymbolRemoved
SymbolPropertyChanged
CircuitCreated
CircuitRenamed
CircuitMoved
ConnectionCreated
ConnectionRemoved
PlanPageCreated
ImportedDrawingAdded
MaterialRecalculated
ValidationStarted
ValidationCompleted
VersionPublished
```

Cela permet :

- undo/redo ;
- journal d'audit ;
- collaboration future ;
- télémétrie technique optionnelle ;
- plugin API ;
- synchronisation cloud future.

---

# 28. Import DWG/DXF/PDF

## Pipeline

1. Détection du format.
2. Lecture de la structure.
3. Extraction des pages/layouts.
4. Extraction des calques.
5. Analyse des unités.
6. Transformation en coordonnées internes.
7. Sélection des calques.
8. Recoloration éventuelle.
9. Découpe.
10. Import dans le document.
11. Mise à l'échelle.
12. Verrouillage.

Pour les PDF complexes, prévoir une stratégie de réparation et fallback raster. Les versions 2026 de Trikker indiquent justement des améliorations de robustesse sur PDF et DWG et une fonction automatique de réparation des PDF difficiles. [S5][S6]

---

# 29. Internationalisation

Langues initiales :

- français ;
- néerlandais ;
- anglais.

Le changement de langue doit modifier :

- interface ;
- menus ;
- messages ;
- propriétés ;
- rapports ;
- impressions ;
- nomenclature ;
- libellés réglementaires.

Les identifiants internes, eux, restent stables et indépendants de la langue.

---

# 30. Accessibilité et ergonomie

Exigences :

- navigation clavier ;
- raccourcis documentés ;
- tailles de zones de clic suffisantes ;
- contraste ;
- retour visuel des sélections ;
- messages d'erreur non ambigus ;
- zoom DPI natif ;
- support écran haute résolution ;
- interface utilisable sur écran tactile Windows à terme.

Trikker a publié en 2026 une amélioration du rendu sur écrans haute résolution, ce qui confirme que le DPI est un critère important pour cette catégorie de logiciel. [S5]

---

# 31. Plateformes et architecture technique recommandée

## 31.1 Cible

L'architecture doit éviter de faire dépendre le métier d'un seul système d'exploitation.

### Option recommandée

- **Core** : .NET 10 / C# ;
- **UI desktop** : Avalonia ;
- **rendu vectoriel** : SkiaSharp ou équivalent ;
- **base locale** : SQLite ;
- **format projet** : ZIP + JSON ;
- **PDF** : moteur vectoriel dédié ;
- **DXF/DWG** : bibliothèque spécialisée ;
- **tests** : xUnit + tests de propriété ;
- **CI** : GitHub Actions ;
- **distribution** : installateur Windows + package macOS ;
- **web futur** : moteur Core partagé + front web.

Cette architecture est un choix de conception du nouveau produit, pas une affirmation sur l'architecture interne de Trikker. La documentation publique de Trikker indique en revanche une migration récente vers .NET 10. [S6]

## 31.2 Découplage

```text
UI
  ↓
Application Services
  ↓
Domain Model
  ↓
Electrical Engine / Rule Engine / Geometry
  ↓
Persistence & Export
```

Aucune règle RGIE ne doit être codée directement dans un bouton ou un écran.

---

# 32. Sécurité

Le logiciel manipule des informations pouvant être sensibles : adresses, installations privées, plans d'immeubles et données d'entreprises.

Exigences :

- stockage local chiffrable ;
- pas d'envoi de plans sans consentement explicite ;
- rapport d'erreur anonymisé ;
- possibilité de désactiver la télémétrie ;
- signature/hash des documents publiés ;
- permissions sur dossiers cloud futurs ;
- suppression sécurisée des fichiers temporaires.

---

# 33. Licences et édition de démonstration

Prévoir trois niveaux :

- **Demo** : fonctions complètes avec limites quantitatives et filigrane ;
- **Professionnelle** : fonctions officielles sans limite ;
- **Éducation** : fonctions pédagogiques avec marquage non officiel.

Trikker utilise actuellement une licence d'essai limitée à cinq circuits et une page de plan de position, avec filigrane, et un abonnement professionnel donnant accès aux fonctions complètes. [S20]

Le nouveau produit doit conserver la possibilité d'édition hors ligne et n'utiliser le réseau que pour l'activation, les mises à jour et les fonctions explicitement dépendantes du cloud.

---

# 34. Contrôle qualité et tests

## 34.1 Tests unitaires

Tester individuellement :

- calculs ;
- règles ;
- conversions d'unités ;
- identifiants ;
- topologie ;
- géométrie ;
- pagination ;
- nomenclature.

## 34.2 Tests de propriété

Exemples :

- une rotation de 360° restitue la géométrie initiale ;
- copier puis supprimer puis annuler restitue l'état ;
- sauvegarder/charger conserve toutes les propriétés ;
- renuméroter deux fois produit un état stable ;
- exporter puis réimporter conserve les coordonnées dans la tolérance définie.

## 34.3 Tests de non-régression visuelle

Chaque release doit comparer :

- schéma ;
- plan ;
- PDF ;
- SVG ;
- nomenclature.

## 34.4 Tests RGIE

Constituer une suite de projets de référence :

- installation domestique simple ;
- plusieurs tableaux ;
- éclairage ;
- prises ;
- appareils fixes ;
- PV ;
- batterie ;
- borne de recharge ;
- salle de bain ;
- installation ancienne ;
- non-domestique ;
- sécurité/critique.

---

# 35. Critères d'acceptation fonctionnels

Le produit est accepté lorsque :

1. un utilisateur peut créer un schéma complet sans saisir de coordonnées manuellement ;
2. chaque symbole électrique peut être modifié par propriétés ;
3. le plan et le schéma partagent les mêmes identifiants métier ;
4. un élément du schéma peut être localisé sur le plan et inversement ;
5. un plan PDF/DWG peut être importé et verrouillé ;
6. plusieurs pages sont supportées ;
7. la nomenclature se met à jour automatiquement ;
8. le projet est sauvegardable et récupérable ;
9. le PDF final est vectoriel et lisible ;
10. les erreurs de validation sont localisées jusqu'à l'objet concerné ;
11. les règles sont versionnées ;
12. le format de fichier est migrable ;
13. aucune validation « conforme » n'est affichée lorsque des données critiques sont inconnues ;
14. les documents peuvent être utilisés sans connexion réseau permanente.

---

# 36. Roadmap proposée

## Phase 0 - Fondations

- architecture ;
- modèle de données ;
- format projet ;
- moteur graphique ;
- grille et coordonnées ;
- système de commandes.

## Phase 1 - MVP dessin

- schéma unifilaire ;
- plan de position ;
- bibliothèque RGIE initiale ;
- propriétés ;
- identifiants ;
- synchronisation ;
- impression PDF ;
- sauvegarde.

## Phase 2 - Productivité

- DWG/DXF/PDF ;
- favoris ;
- symboles personnalisés ;
- modèles ;
- nomenclature ;
- export Excel/CSV ;
- autosave avancé ;
- pagination avancée.

## Phase 3 - Contrôle électrique

- moteur de règles RGIE ;
- calculs ;
- rapports ;
- PV/PDF ;
- anciennes installations ;
- locaux spéciaux.

## Phase 4 - Professionnalisation

- devis ;
- catalogue matériel ;
- fabricants ;
- bibliothèques partenaires ;
- signatures ;
- gestion des projets ;
- API.

## Phase 5 - Cloud et multiplateforme

- moteur cloud ;
- synchronisation ;
- collaboration ;
- web ;
- tablette ;
- historique serveur.

---

# 37. Backlog initial détaillé

### Epic A - Projet

- A001 créer projet
- A002 ouvrir projet
- A003 enregistrer
- A004 enregistrer sous
- A005 métadonnées
- A006 version
- A007 import/export

### Epic B - Schéma

- B001 bibliothèque
- B002 placement
- B003 connexion
- B004 branche
- B005 propriétés
- B006 déplacement
- B007 rotation
- B008 copier/coller
- B009 dessin libre
- B010 lignes
- B011 pagination

### Epic C - Plan

- C001 mur
- C002 porte
- C003 fenêtre
- C004 escalier
- C005 rectangle
- C006 image
- C007 import PDF
- C008 import DXF
- C009 import DWG
- C010 calques
- C011 découpe
- C012 transparence
- C013 accrochage

### Epic D - Synchronisation

- D001 identifiant unique
- D002 rechercher schéma
- D003 rechercher plan
- D004 synchronisation propriétés
- D005 objets orphelins
- D006 doublons

### Epic E - Nomenclature

- E001 calcul quantités
- E002 regroupement
- E003 export Excel
- E004 export CSV
- E005 miniature symbole

### Epic F - RGIE

- F001 RulePack
- F002 validation documentaire
- F003 validation repérage
- F004 validation protection
- F005 validation canalisation
- F006 validation sources
- F007 validation locaux spéciaux
- F008 rapport

### Epic G - Calculs

- G001 P/Q/S
- G002 courant
- G003 chute de tension
- G004 courant admissible
- G005 protection
- G006 court-circuit
- G007 calcul multi-tableaux

---

# 38. Documentation utilisateur à livrer

Le produit doit inclure un manuel couvrant au minimum :

1. Installation.
2. Création d'un projet.
3. Gestion des métadonnées.
4. Bibliothèque de symboles.
5. Schéma unifilaire.
6. Plan de position.
7. Import PDF/DXF/DWG.
8. Synchronisation.
9. Symboles libres.
10. Domotique.
11. Nomenclature.
12. Validation.
13. Impression.
14. Export.
15. Sauvegardes.
16. Versions.
17. Paramètres.
18. Raccourcis clavier.
19. Dépannage.
20. Gestion des licences.

---

# 39. Documentation technique à livrer

Livrables obligatoires :

- architecture système ;
- modèle de données ;
- schéma de persistance ;
- spécification format projet ;
- dictionnaire des symboles ;
- documentation des propriétés ;
- API publique ;
- API plugin ;
- moteur de règles ;
- catalogue des règles RGIE ;
- moteur de calculs ;
- guide de migration ;
- stratégie de tests ;
- procédure de release ;
- guide de contribution.

---

# 40. Documentation RGIE embarquée

La base réglementaire doit être livrée sous forme de métadonnées :

```text
RulePack
  id: RGIE-BE
  version: 2026-04-01
  source: SPF Economie
  jurisdiction: BE
  scope: LV/LVLS
```

Chaque règle doit pointer vers :

- livre ;
- partie ;
- chapitre ;
- section ;
- sous-section ;
- texte ou résumé autorisé ;
- source officielle ;
- date d'application ;
- statut.

Le texte réglementaire complet ne doit pas être copié dans l'application sans vérification des droits de reproduction et de la politique de diffusion. Les références officielles doivent être conservées.

---

# 41. Documentation de maintenance réglementaire

Le moteur réglementaire doit permettre de recevoir une nouvelle version sans réécrire le logiciel.

Pipeline :

1. publication d'une nouvelle source officielle ;
2. analyse des différences ;
3. création d'un RulePack ;
4. tests de non-régression ;
5. validation experte ;
6. activation à une date donnée ;
7. archivage de l'ancien RulePack.

Un projet doit conserver la version réglementaire sous laquelle il a été conçu et pouvoir être relu avec cette version.

---

# 42. Exigences non fonctionnelles

| Domaine | Exigence cible |
|---|---|
| Démarrage | < 3 s sur machine moderne pour projet standard |
| Interaction | < 50 ms sur action graphique courante |
| Zoom/pan | 60 FPS cible sur projet standard |
| Sauvegarde | < 2 s pour projet domestique standard |
| Crash recovery | aucune perte des modifications validées depuis le dernier autosave |
| Fichier | format versionné et migrable |
| PDF | vectoriel, textes sélectionnables lorsque possible |
| Disponibilité | fonctionnement hors ligne pour édition principale |
| Localisation | FR/NL/EN |
| Écran | support DPI élevé |
| Sécurité | aucune exfiltration des plans sans consentement |
| Compatibilité | Windows en première cible, architecture prête pour macOS/web |

---

# 43. Différences recommandées par rapport à Trikker

Le produit cible peut apporter une valeur supplémentaire sur les points suivants :

- véritable séparation modèle / rendu ;
- moteur de calculs ;
- validation RGIE explicable ;
- versions non destructives ;
- historique global du projet ;
- recherche plein texte ;
- modèles de projets ;
- bibliothèques fabricant ;
- catalogue de prix ;
- devis ;
- export JSON/API ;
- collaboration cloud ;
- mode tablette ;
- génération automatique de rapports ;
- comparaison entre deux versions ;
- check-list de contrôle terrain ;
- import BIM/IFC à terme.

---

# 44. Limites volontairement posées

Le logiciel ne doit pas :

- prétendre certifier une installation ;
- remplacer l'inspection physique ;
- déduire une conformité lorsqu'une donnée est manquante ;
- modifier silencieusement les données de l'utilisateur ;
- écraser une version publiée sans historique ;
- considérer un plan importé comme électriquement exact simplement parce qu'il est lisible.

---

# 45. Bibliographie et sources

**[S1] Trikker - Guide de l'utilisateur**, V1.5.88, Bluebits, documentation officielle.  
https://www.trikker.be/web/content/1460?download=true

**[S2] Trikker - Site officiel francophone**, fonctionnalités, plan de position, import DWG, nomenclature.  
https://www.trikker.be/fr

**[S3] Trikker - Tarifs et installation**, limites de démonstration et licence.  
https://www.trikker.be/fr/pricing

**[S4] Trikker 1.5.99**, lignes dans le schéma et migration .NET 10, 30 mars 2026.  
https://www.trikker.be/fr/blog/actualites-2/trikker-v1-5-99-est-maintenant-disponible-7

**[S5] Trikker 1.5.103**, PDF, DPI et stabilité, 22 juin 2026.  
https://www.trikker.be/en/blog/news-2/trikker-1-5-103-is-available-12

**[S6] Trikker évolue vers .NET 10**, 30 mars 2026.  
https://www.trikker.be/fr/blog/actualites-2/trikker-evolue-vers-net-10-8

**[S7] SPF Economie - RGIE Livre 1, version 06 / 2026**, basse tension et très basse tension.  
https://economie.fgov.be/sites/default/files/Files/Publications/files/RGIE-Annexe-Livre-1-Installations-a-basse-tenstion-et-a-tres-basse-tension-2026.pdf

**[S8] SPF Economie - Livre 1 RGIE 2026, section 3.1.2**, schémas, plans, documents et repérage domestique.  
Même source que [S7].

**[S9] SPF Economie - RGIE 2026, chapitre 2.13**, symboles graphiques.  
Même source que [S7].

**[S10] SPF Economie - RGIE 2026, sous-sections 3.1.2.2 et 3.1.2.3**, contenu minimal des schémas et plans.  
Même source que [S7].

**[S11] Trikker - Guide utilisateur**, dessin du plan d'étage, accrochage et géométrie.  
https://www.trikker.be/web/content/1460?download=true

**[S12] Trikker - Guide utilisateur**, import images, PDF, DXF et DWG.  
https://www.trikker.be/web/content/1460?download=true

**[S13] Trikker - Guide utilisateur**, symboles libres, scénarios et transparence.  
https://www.trikker.be/web/content/1460?download=true

**[S14] Trikker - Guide utilisateur**, adresses et synchronisation logique.  
https://www.trikker.be/web/content/1460?download=true

**[S15] Trikker - Guide utilisateur**, module domotique multi-sorties.  
https://www.trikker.be/web/content/1460?download=true

**[S16] Trikker - Guide utilisateur**, pagination et échelle d'impression.  
https://www.trikker.be/web/content/1460?download=true

**[S17] Trikker - Guide utilisateur**, nomenclature, undo/redo et impression.  
https://www.trikker.be/web/content/1460?download=true

**[S18] SPF Economie - RGIE 2026**, modifications 2026 relatives aux systèmes à courant continu.  
https://economie.fgov.be/fr/publications/reglement-general-sur-les

**[S19] Trikker - Guide utilisateur**, sauvegardes automatiques.  
https://www.trikker.be/web/content/1460?download=true

**[S20] Trikker - Tarifs**, version démo et licences.  
https://www.trikker.be/fr/pricing

**[S21] SPF Economie - Contrôle des installations électriques domestiques**, contenu du dossier et contrôle. Mise à jour 11 août 2026.  
https://economie.fgov.be/fr/themes/energie/sources-et-vecteurs-denergie/electricite/securite-et-controle-des/controle-des-installations

**[S22] Volta - Fiche 19, Symboles du schéma unifilaire**, synthèse professionnelle des symboles.  
https://volta-org.be/media/n5kfs03l/19_volta_symboles_du_schema_unifiaire.pdf

---

# 46. Annexes recommandées

## Annexe A - Exemple de projet minimal

Un projet minimal doit contenir :

- réseau d'alimentation ;
- tableau principal ;
- différentiel ;
- deux circuits ;
- une prise ;
- un point lumineux ;
- un interrupteur ;
- un plan simple avec deux pièces ;
- nomenclature ;
- validation ;
- PDF.

## Annexe B - Exemple de flux utilisateur

```text
Nouveau projet
   ↓
Informations administratives
   ↓
Choix alimentation / type installation
   ↓
Schéma unifilaire
   ↓
Plan de position
   ↓
Synchronisation automatique
   ↓
Validation
   ↓
Correction
   ↓
Nomenclature
   ↓
PDF dossier
   ↓
Publication v1
```

## Annexe C - Structure JSON simplifiée

```json
{
  "project": {
    "id": "PRJ-001",
    "version": 1,
    "regulatory_pack": "RGIE-2026"
  },
  "installation": {
    "voltage": 230,
    "current_type": "AC",
    "phases": 1,
    "earthing": "TT"
  },
  "boards": [],
  "circuits": [],
  "symbols": [],
  "plans": [],
  "validation": []
}
```

---

# Conclusion

Le logiciel à développer doit être considéré comme une plateforme de documentation et de conception électrique structurée, dont Trikker constitue une excellente référence ergonomique et fonctionnelle. Le périmètre minimal viable est l'édition synchronisée d'un schéma unifilaire et d'un plan de position, avec bibliothèque de symboles RGIE, import de plans, nomenclature, impression PDF et sauvegarde. La différenciation majeure recommandée est l'ajout d'un modèle électrique réellement structuré, d'un moteur de calcul indépendant, d'un moteur de règles RGIE versionné et d'une architecture multiplateforme.
