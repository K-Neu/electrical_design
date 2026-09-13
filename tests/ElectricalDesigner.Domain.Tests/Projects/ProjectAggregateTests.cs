using ElectricalDesigner.Domain.Cabling;
using ElectricalDesigner.Domain.Common;
using ElectricalDesigner.Domain.Installations;
using ElectricalDesigner.Domain.Loads;
using ElectricalDesigner.Domain.Projects;
using ElectricalDesigner.Domain.Protections;
using ElectricalDesigner.Domain.Sources;
using ElectricalDesigner.TestKit;

namespace ElectricalDesigner.Domain.Tests.Projects;

/// <summary>
/// Ces tests matérialisent le critère de sortie de la Phase 1 (guide §Phase 1) :
/// "créer et sauvegarder un projet avec un tableau, un circuit, une protection
/// et une charge sans interface graphique." La partie "sauvegarder" est testée
/// séparément dans ElectricalDesigner.Infrastructure.Tests (persistance).
/// </summary>
public class ProjectAggregateTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 10, 8, 0, 0, TimeSpan.Zero);

    private static Project CreateMinimalProject()
    {
        var metadata = new ProjectMetadata { Name = "Maison Dupont" };
        var installation = ElectricalInstallation.Create(
            EntityId<ElectricalInstallation>.New(),
            InstallationCategory.Domestic,
            CurrentType.AcSinglePhase,
            EarthingSystem.Tt,
            nominalVoltageV: 230,
            frequencyHz: 50,
            Now);

        return Project.Create(EntityId<Project>.New(), metadata, installation, Now);
    }

    [Fact]
    public void ScenarioCritereDeSortiePhase1_TableauCircuitProtectionCharge_EstValide()
    {
        var project = CreateMinimalProject();

        var network = project.AddSource(
            EntityId<Source>.New(), "Réseau de distribution", SourceType.Network,
            CurrentType.AcSinglePhase, 230, Now);

        var board = project.AddBoard(
            EntityId<Boards.DistributionBoard>.New(), "TGBT", "Tableau principal", 230,
            CurrentType.AcSinglePhase, Now);
        board.SetDirectSource(network.Id, Now);

        var protection = project.AddProtectionDevice(
            EntityId<ProtectionDevice>.New(), "Q1", ProtectionType.CircuitBreaker, 16, Now);

        var cable = project.AddCable(
            EntityId<Cable>.New(), "C-B", phaseConductorCount: 1, hasNeutral: true, hasProtectiveEarth: true,
            crossSectionMm2: 2.5, ConductorMaterial.Copper, InsulationMaterial.Pvc, InstallationMethod.Embedded,
            lengthM: 12, Now);

        var load = project.AddLoad(
            EntityId<Load>.New(), "Prise cuisine", LoadCategory.Socket, powerW: 2300, Now);

        var circuit = board.AddCircuit(
            EntityId<Circuits.Circuit>.New(), "B", "Prises cuisine", CurrentType.AcSinglePhase, 230, 50, Now);
        circuit.AssignProtection(protection.Id, Now);
        circuit.AssignCable(cable.Id, Now);
        circuit.AddLoad(load.Id, Now);

        var issues = project.Validate();

        Assert.Equal(0, issues.Count);
        Assert.Equal(1, project.Boards.Count);
        Assert.Equal(1, project.Boards[0].Circuits.Count);
        Assert.Equal("B", project.Boards[0].Circuits[0].Reference);
    }

    [Fact]
    public void Create_AvecMetadonneesSansNom_Echoue()
    {
        var installation = ElectricalInstallation.Create(
            EntityId<ElectricalInstallation>.New(),
            InstallationCategory.Domestic,
            CurrentType.AcSinglePhase,
            EarthingSystem.Tt,
            230,
            50,
            Now);

        Assert.Throws<DomainValidationException>(() =>
            Project.Create(EntityId<Project>.New(), new ProjectMetadata { Name = "   " }, installation, Now));
    }

    [Fact]
    public void AddBoard_AvecReferenceDejaUtilisee_Echoue()
    {
        var project = CreateMinimalProject();
        project.AddBoard(EntityId<Boards.DistributionBoard>.New(), "TGBT", "Tableau principal", 230, CurrentType.AcSinglePhase, Now);

        Assert.Throws<ArgumentException>(() =>
            project.AddBoard(EntityId<Boards.DistributionBoard>.New(), "TGBT", "Doublon", 230, CurrentType.AcSinglePhase, Now));
    }

    [Fact]
    public void AddCircuit_AvecReferenceDejaUtiliseeSurLeMemeTableau_Echoue()
    {
        var project = CreateMinimalProject();
        var board = project.AddBoard(EntityId<Boards.DistributionBoard>.New(), "TGBT", "Tableau principal", 230, CurrentType.AcSinglePhase, Now);
        board.AddCircuit(EntityId<Circuits.Circuit>.New(), "B", "Prises cuisine", CurrentType.AcSinglePhase, 230, 50, Now);

        Assert.Throws<ArgumentException>(() =>
            board.AddCircuit(EntityId<Circuits.Circuit>.New(), "B", "Doublon", CurrentType.AcSinglePhase, 230, 50, Now));
    }

    [Fact]
    public void Validate_CircuitAvecProtectionInexistante_EstSignale()
    {
        var project = CreateMinimalProject();
        var board = project.AddBoard(EntityId<Boards.DistributionBoard>.New(), "TGBT", "Tableau principal", 230, CurrentType.AcSinglePhase, Now);
        var network = project.AddSource(EntityId<Source>.New(), "Réseau", SourceType.Network, CurrentType.AcSinglePhase, 230, Now);
        board.SetDirectSource(network.Id, Now);

        var circuit = board.AddCircuit(EntityId<Circuits.Circuit>.New(), "B", "Prises cuisine", CurrentType.AcSinglePhase, 230, 50, Now);
        // Référence une protection qui n'a jamais été ajoutée au projet.
        circuit.AssignProtection(EntityId<ProtectionDevice>.New(), Now);

        var issues = project.Validate();

        Assert.True(issues.Any(i => i.Path.Contains("ProtectionId") && i.Message.Contains("n'existe pas")));
    }

    [Fact]
    public void Validate_TableauSansSourceNiAmont_EstSignale()
    {
        var project = CreateMinimalProject();
        project.AddBoard(EntityId<Boards.DistributionBoard>.New(), "TGBT", "Tableau principal", 230, CurrentType.AcSinglePhase, Now);

        var issues = project.Validate();

        Assert.True(issues.Any(i => i.Message.Contains("ni source directe ni tableau amont")));
    }

    [Fact]
    public void PublishRevision_IncrementeLeNumeroDeVersionAChaquePublication()
    {
        var project = CreateMinimalProject();

        var first = project.PublishRevision(EntityId<ProjectRevision>.New(), "A. Dupont", "Création initiale", Now);
        var second = project.PublishRevision(EntityId<ProjectRevision>.New(), "A. Dupont", "Ajout du tableau principal", Now);

        Assert.Equal(1, first.VersionNumber);
        Assert.Equal(2, second.VersionNumber);
        Assert.Equal(2, project.Revisions.Count);
    }
}
