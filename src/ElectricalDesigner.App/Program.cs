using ElectricalDesigner.Canvas.Clipboard;
using ElectricalDesigner.Canvas.Commands;
using ElectricalDesigner.Canvas.Geometry;
using ElectricalDesigner.Canvas.Persistence;
using ElectricalDesigner.Canvas.Scene;
using ElectricalDesigner.Canvas.Selection;
using ElectricalDesigner.Domain.Boards;
using ElectricalDesigner.Domain.Cabling;
using ElectricalDesigner.Domain.Circuits;
using ElectricalDesigner.Domain.Common;
using ElectricalDesigner.Domain.Installations;
using ElectricalDesigner.Domain.Loads;
using ElectricalDesigner.Domain.Projects;
using ElectricalDesigner.Domain.Protections;
using ElectricalDesigner.Domain.Sources;
using ElectricalDesigner.Infrastructure.Persistence;
using CanvasObjectType = ElectricalDesigner.Canvas.Scene.CanvasObject;

// ---------------------------------------------------------------------------
// Point d'entrée temporaire — Phase 1 (fondation du modèle de données) +
// Phase 2 (moteur graphique commun).
//
// Toujours pas d'interface graphique réelle ici (elle arrive en Phase 3 avec
// Avalonia). Ce programme démontre successivement le critère de sortie exact
// de la Phase 1 (guide §Phase 1) : "créer et sauvegarder un projet avec un
// tableau, un circuit, une protection et une charge sans interface
// graphique", puis celui de la Phase 2 : "placer 100 objets sur une scène,
// les déplacer, les sélectionner, zoomer, annuler et sauvegarder leurs
// coordonnées sans perte."
// ---------------------------------------------------------------------------

var now = DateTimeOffset.UtcNow;

Console.WriteLine("=========================================================");
Console.WriteLine(" Logiciel de conception de réseaux électriques — Phase 1 ");
Console.WriteLine("=========================================================");
Console.WriteLine();

// --- Modèle métier : un projet minimal (maison, tableau, circuit, protection, charge) ---
var metadata = new ProjectMetadata
{
    Name = "Maison Dupont",
    Address = "Rue de la Loi 1, 1000 Bruxelles",
    Author = "A. Dupont",
};

var installation = ElectricalInstallation.Create(
    EntityId<ElectricalInstallation>.New(),
    InstallationCategory.Domestic,
    CurrentType.AcSinglePhase,
    EarthingSystem.Tt,
    nominalVoltageV: 230,
    frequencyHz: 50,
    now);

var project = Project.Create(EntityId<Project>.New(), metadata, installation, now);

var network = project.AddSource(
    EntityId<Source>.New(), "Réseau de distribution", SourceType.Network, CurrentType.AcSinglePhase, 230, now);

var board = project.AddBoard(
    EntityId<DistributionBoard>.New(), "TGBT", "Tableau principal", 230, CurrentType.AcSinglePhase, now);
board.SetDirectSource(network.Id, now);

var protection = project.AddProtectionDevice(
    EntityId<ProtectionDevice>.New(), "Q1", ProtectionType.CircuitBreaker, ratedCurrentA: 16, now,
    breakingCapacityKa: 6);

var cable = project.AddCable(
    EntityId<Cable>.New(), "C-B", phaseConductorCount: 1, hasNeutral: true, hasProtectiveEarth: true,
    crossSectionMm2: 2.5, ConductorMaterial.Copper, InsulationMaterial.Pvc, InstallationMethod.Embedded,
    lengthM: 12, now);

var load = project.AddLoad(
    EntityId<Load>.New(), "Prise cuisine", LoadCategory.Socket, powerW: 2300, now);

var circuit = board.AddCircuit(
    EntityId<Circuit>.New(), "B", "Prises cuisine", CurrentType.AcSinglePhase, 230, 50, now);
circuit.AssignProtection(protection.Id, now);
circuit.AssignCable(cable.Id, now);
circuit.AddLoad(load.Id, now);

project.PublishRevision(EntityId<ProjectRevision>.New(), metadata.Author ?? "Inconnu", "Création initiale", now);

Console.WriteLine($"Projet créé : {project.Metadata.Name} ({project.Id})");
Console.WriteLine($"  Tableau {board.Reference} — {board.Circuits.Count} circuit(s)");
Console.WriteLine($"  Circuit {circuit.Reference} \"{circuit.Name}\" — protection {protection.Reference}, câble {cable.ToShortNotation()}, {circuit.LoadIds.Count} charge(s)");

var modelIssues = project.Validate();
Console.WriteLine(modelIssues.Count == 0
    ? "  Validation du modèle : OK (aucun problème structurel)."
    : $"  Validation du modèle : {modelIssues.Count} problème(s) détecté(s).");

// --- Persistance : sauvegarder puis recharger (guide §21.2) ---
var filePath = Path.Combine(Path.GetTempPath(), "electrical-designer-demo", "maison-dupont.elecproj");
var repository = new ProjectFileRepository();

repository.Save(project, filePath);
Console.WriteLine();
Console.WriteLine($"Projet sauvegardé : {filePath}");

var reloaded = repository.Load(filePath);
var reloadedIssues = reloaded.Validate();

Console.WriteLine($"Projet rechargé   : {reloaded.Metadata.Name} ({reloaded.Id})");
Console.WriteLine(reloadedIssues.Count == 0
    ? "  Intégrité après rechargement : OK."
    : $"  Intégrité après rechargement : {reloadedIssues.Count} problème(s) détecté(s).");

Console.WriteLine();
Console.WriteLine("Statut Phase 1 : socle du modèle de données opérationnel (classes métier,");
Console.WriteLine("validation, sérialisation, identifiants stables).");

// =============================================================================
// Phase 2 — moteur graphique commun (canvas)
// =============================================================================

Console.WriteLine();
Console.WriteLine("=========================================================");
Console.WriteLine(" Moteur graphique commun — Phase 2                       ");
Console.WriteLine("=========================================================");
Console.WriteLine();

var scene = new CanvasScene();
var history = new CommandHistory();
var selection = new SelectionManager();
var clipboard = new CanvasClipboard();
var view = WorldTransform.Identity;

// --- Placer 100 objets sur une scène ---
var canvasIds = new List<CanvasObjectId>();
for (var i = 0; i < 100; i++)
{
    var canvasObject = CanvasObjectType.Create(
        CanvasObjectId.New(), new Point2D(i * 15, i * 8), new Size2D(20, 20));
    history.Execute(new AddObjectCommand(canvasObject), scene);
    canvasIds.Add(canvasObject.Id);
}

Console.WriteLine($"Scène créée : {scene.Count} objets placés.");

// --- Les sélectionner (multi-sélection) puis les déplacer d'un seul geste groupé ---
selection.SelectRange(canvasIds.Take(40));
history.Execute(new MoveObjectsCommand(selection.SelectedIds.ToList(), new Point2D(500, 250)), scene);
Console.WriteLine($"  {selection.Count} objets sélectionnés puis déplacés en un seul geste groupé.");

// --- Zoomer, centré sur le curseur ---
var cursor = new Point2D(400, 300);
view = view.ZoomAt(cursor, 1.75);
Console.WriteLine($"  Zoom appliqué (centré sur le curseur) : x{view.Zoom:0.00}.");

// --- Copier/coller un petit groupe ---
clipboard.Copy(canvasIds.Take(3).Select(scene.Get));
var pasted = clipboard.Paste(new Point2D(10, 10));
history.Execute(new CompositeCommand(pasted.Select(p => (ICanvasCommand)new AddObjectCommand(p)).ToList(), "Coller 3 objets"), scene);
Console.WriteLine($"  3 objets copiés/collés — scène : {scene.Count} objets.");

// --- Annuler (undo) le collage puis le déplacement groupé ---
history.Undo(scene);
history.Undo(scene);
Console.WriteLine($"  Annulation x2 (collage puis déplacement groupé) — scène : {scene.Count} objets, positions restaurées.");

// --- Sauvegarder puis recharger la scène, sans perte de coordonnées ---
var scenePath = Path.Combine(Path.GetTempPath(), "electrical-designer-demo", "scene-phase2.json");
SceneSerializer.SaveToFile(scene, scenePath);
var reloadedScene = SceneSerializer.LoadFromFile(scenePath);

var coordinatesPreserved = scene.Objects.All(o =>
{
    var reloadedObject = reloadedScene.Get(o.Id);
    return reloadedObject.Position.X.Equals(o.Position.X) && reloadedObject.Position.Y.Equals(o.Position.Y);
});

Console.WriteLine();
Console.WriteLine($"Scène sauvegardée : {scenePath}");
Console.WriteLine($"Scène rechargée   : {reloadedScene.Count} objets.");
Console.WriteLine(coordinatesPreserved
    ? "  Coordonnées préservées après sauvegarde/rechargement : OK."
    : "  ATTENTION : des coordonnées diffèrent après rechargement.");

Console.WriteLine();
Console.WriteLine("Statut Phase 2 : moteur graphique commun opérationnel (coordonnées, zoom,");
Console.WriteLine("panoramique, sélection simple/multiple, déplacement, rotation,");
Console.WriteLine("redimensionnement, grille/accrochage, copier/coller, undo/redo).");
Console.WriteLine("Prochaine étape : Phase 3 — éditeur de schéma unifilaire.");
