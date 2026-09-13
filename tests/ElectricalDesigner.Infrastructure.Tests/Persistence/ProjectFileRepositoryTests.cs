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
using ElectricalDesigner.TestKit;

namespace ElectricalDesigner.Infrastructure.Tests.Persistence;

/// <summary>
/// Matérialise très exactement le test d'intégration recommandé au guide §21.2 :
/// "Créer projet → créer circuit → sauvegarder → fermer → réouvrir → vérifier
/// intégrité", ainsi que le critère de sortie de la Phase 1 côté persistance
/// ("sauvegarder un projet avec un tableau, un circuit, une protection et une
/// charge sans interface graphique").
/// </summary>
public class ProjectFileRepositoryTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 10, 8, 0, 0, TimeSpan.Zero);

    private static Project BuildSampleProject()
    {
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
            Now);

        var project = Project.Create(EntityId<Project>.New(), metadata, installation, Now);

        var network = project.AddSource(
            EntityId<Source>.New(), "Réseau de distribution", SourceType.Network, CurrentType.AcSinglePhase, 230, Now);

        var board = project.AddBoard(
            EntityId<DistributionBoard>.New(), "TGBT", "Tableau principal", 230, CurrentType.AcSinglePhase, Now);
        board.SetDirectSource(network.Id, Now);
        board.SetPresumedShortCircuitCurrent(3.5, Now);

        var protection = project.AddProtectionDevice(
            EntityId<ProtectionDevice>.New(), "Q1", ProtectionType.CircuitBreaker, 16, Now,
            breakingCapacityKa: 6);

        var cable = project.AddCable(
            EntityId<Cable>.New(), "C-B", phaseConductorCount: 1, hasNeutral: true, hasProtectiveEarth: true,
            crossSectionMm2: 2.5, ConductorMaterial.Copper, InsulationMaterial.Pvc, InstallationMethod.Embedded,
            lengthM: 12, Now);

        var load = project.AddLoad(
            EntityId<Load>.New(), "Prise cuisine", LoadCategory.Socket, powerW: 2300, Now);

        var circuit = board.AddCircuit(
            EntityId<Circuit>.New(), "B", "Prises cuisine", CurrentType.AcSinglePhase, 230, 50, Now,
            notes: "Circuit de test");
        circuit.AssignProtection(protection.Id, Now);
        circuit.AssignCable(cable.Id, Now);
        circuit.AddLoad(load.Id, Now);

        project.PublishRevision(EntityId<ProjectRevision>.New(), "A. Dupont", "Création initiale", Now);

        return project;
    }

    private static string CreateTempFilePath()
    {
        var directory = Path.Combine(Path.GetTempPath(), "electrical-designer-tests", Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(directory);
        return Path.Combine(directory, "projet-test.elecproj");
    }

    [Fact]
    public void SauvegarderPuisRecharger_PreserveLIntegriteDuProjet()
    {
        var original = BuildSampleProject();
        var filePath = CreateTempFilePath();
        var repository = new ProjectFileRepository();

        try
        {
            // --- sauvegarder ---
            repository.Save(original, filePath);
            Assert.True(File.Exists(filePath), "Le fichier .elecproj doit exister après Save().");

            // --- fermer / réouvrir (nouvelle instance de dépôt + nouveau graphe d'objets) ---
            var reloadedRepository = new ProjectFileRepository();
            var reloaded = reloadedRepository.Load(filePath);

            // --- vérifier intégrité ---
            Assert.Equal(0, reloaded.Validate().Count);

            Assert.Equal(original.Id, reloaded.Id);
            Assert.Equal(original.CreatedAtUtc, reloaded.CreatedAtUtc);
            Assert.Equal(original.Metadata.Name, reloaded.Metadata.Name);
            Assert.Equal(original.Metadata.Address, reloaded.Metadata.Address);

            Assert.Equal(original.Installation.NominalVoltageV, reloaded.Installation.NominalVoltageV);
            Assert.Equal(original.Installation.EarthingSystem, reloaded.Installation.EarthingSystem);

            Assert.Equal(1, reloaded.Sources.Count);
            Assert.Equal(1, reloaded.Boards.Count);

            var reloadedBoard = reloaded.Boards[0];
            Assert.Equal("TGBT", reloadedBoard.Reference);
            Assert.Equal(3.5, reloadedBoard.PresumedShortCircuitCurrentKa);
            Assert.Equal(1, reloadedBoard.Circuits.Count);

            var reloadedCircuit = reloadedBoard.Circuits[0];
            Assert.Equal("B", reloadedCircuit.Reference);
            Assert.Equal("Circuit de test", reloadedCircuit.Notes);
            Assert.NotNull(reloadedCircuit.ProtectionId);
            Assert.NotNull(reloadedCircuit.CableId);
            Assert.Equal(1, reloadedCircuit.LoadIds.Count);

            Assert.Equal(1, reloaded.ProtectionDevices.Count);
            Assert.Equal(1, reloaded.Cables.Count);
            Assert.Equal(1, reloaded.Loads.Count);
            Assert.Equal(1, reloaded.Revisions.Count);
            Assert.Equal("Création initiale", reloaded.Revisions[0].Summary);
        }
        finally
        {
            CleanupDirectory(filePath);
        }
    }

    [Fact]
    public void Load_AvecFichierIntacte_NeLeveAucuneException()
    {
        var project = BuildSampleProject();
        var filePath = CreateTempFilePath();
        var repository = new ProjectFileRepository();

        try
        {
            repository.Save(project, filePath);

            // Ne doit pas lever : c'est le scénario nominal.
            var reloaded = repository.Load(filePath);

            Assert.Equal(project.Id, reloaded.Id);
        }
        finally
        {
            CleanupDirectory(filePath);
        }
    }

    [Fact]
    public void Load_AvecFichierCorrompu_LeveInvalidDataException()
    {
        var project = BuildSampleProject();
        var filePath = CreateTempFilePath();
        var repository = new ProjectFileRepository();

        try
        {
            repository.Save(project, filePath);

            // Corruption : on altère un octet du fichier après coup, ce qui casse
            // soit la structure du ZIP, soit (le plus souvent) le hash déclaré
            // dans le manifeste sans casser le ZIP lui-même.
            CorruptLastByte(filePath);

            Assert.Throws<InvalidDataException>(() => repository.Load(filePath));
        }
        finally
        {
            CleanupDirectory(filePath);
        }
    }

    [Fact]
    public void Load_AvecFichierInexistant_LeveFileNotFoundException()
    {
        var repository = new ProjectFileRepository();
        var missingPath = Path.Combine(Path.GetTempPath(), "electrical-designer-tests", Guid.NewGuid().ToString("n"), "absent.elecproj");

        Assert.Throws<FileNotFoundException>(() => repository.Load(missingPath));
    }

    private static void CorruptLastByte(string filePath)
    {
        var bytes = File.ReadAllBytes(filePath);
        bytes[^1] ^= 0xFF;
        File.WriteAllBytes(filePath, bytes);
    }

    private static void CleanupDirectory(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (directory is not null && Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
