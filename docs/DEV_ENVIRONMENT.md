# Environnement de développement

> Livrable Phase 0 (guide §2, Phase 0 : "environnement de développement").

## Prérequis

- SDK .NET 10 (voir `global.json` — version épinglée : `10.0.100`,
  `rollForward: latestFeature`).
- Git.
- (À partir de la Phase 2) un poste avec accès NuGet standard pour restaurer
  Avalonia/SkiaSharp — voir `docs/01_Architecture.md`, section 5, pour le
  contexte de cette limitation en Phase 0.

## Installation du SDK (Ubuntu/Debian)

```bash
sudo apt-get update
sudo apt-get install -y dotnet-sdk-10.0
dotnet --version
```

Sur d'autres plateformes, utiliser le script d'installation officiel
Microsoft ou le gestionnaire de paquets natif (Homebrew, winget...).

## Cloner et vérifier le socle

```bash
git clone <url-du-depot>
cd electrical-designer
dotnet build
dotnet run --project src/ElectricalDesigner.App
dotnet run --project tests/ElectricalDesigner.Domain.Tests
dotnet run --project tests/ElectricalDesigner.Infrastructure.Tests
dotnet run --project tests/ElectricalDesigner.Canvas.Tests
```

La sortie attendue de `src/ElectricalDesigner.App` : création d'un projet
minimal (tableau, circuit, protection, câble, charge), validation du modèle,
sauvegarde dans un fichier `.elecproj`, rechargement avec vérification
d'intégrité, puis (Phase 2) une démonstration du moteur canvas (100 objets
placés, sélectionnés, déplacés, zoom, undo, sauvegarde/rechargement de scène)
— voir `docs/03_File_Format.md` et `docs/04_Canvas.md`.

C'est exactement le critère de sortie de la Phase 1 (guide §Phase 1) :
"créer et sauvegarder un projet avec un tableau, un circuit, une protection
et une charge sans interface graphique", puis celui de la Phase 2 : "placer
100 objets sur une scène, les déplacer, les sélectionner, zoomer, annuler et
sauvegarder leurs coordonnées sans perte."

## Éditeurs recommandés

- Visual Studio / Visual Studio Code avec l'extension C# Dev Kit.
- JetBrains Rider.

`.editorconfig` est pris en charge nativement par les trois.

## Formatage automatique

```bash
dotnet format
```

À exécuter avant chaque commit (sera automatisé en CI, voir
`.github/workflows/ci.yml`).

## Limitation connue de cet environnement d'amorçage

L'environnement utilisé pour créer ce dépôt initial n'avait pas accès à
`nuget.org` (liste blanche réseau restreinte à `archive.ubuntu.com`,
`github.com`, etc.). Le socle Phase 0/1/2 a donc été construit sans aucun
package NuGet. Un poste de développement normal, ou la CI GitHub Actions,
n'a pas cette limitation. Voir `docs/01_Architecture.md` §5 pour le détail et
la marche à suivre pour ajouter Avalonia/xUnit/SQLite dès que possible.
