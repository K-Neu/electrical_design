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

// ---------------------------------------------------------------------------
// Point d'entrée temporaire — Phase 1 (fondation du modèle de données).
//
// Toujours pas d'interface graphique ici (elle arrive en Phase 2/3 avec
// Avalonia). Ce programme démontre le critère de sortie exact de la Phase 1
// (guide §Phase 1) : "créer et sauvegarder un projet avec un tableau, un
// circuit, une protection et une charge sans interface graphique."
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
Console.WriteLine("Prochaine étape : Phase 2 — moteur graphique commun (canvas).");
