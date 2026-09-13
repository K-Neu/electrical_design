using ElectricalDesigner.Domain.Boards;
using ElectricalDesigner.Domain.Cabling;
using ElectricalDesigner.Domain.Common;
using ElectricalDesigner.Domain.Installations;
using ElectricalDesigner.Domain.Loads;
using ElectricalDesigner.Domain.Plans;
using ElectricalDesigner.Domain.Protections;
using ElectricalDesigner.Domain.Sources;
using ElectricalDesigner.Domain.Symbols;

namespace ElectricalDesigner.Domain.Projects;

/// <summary>
/// Agrégat racine du modèle électrique (cahier §9 ; guide §1.1 : "Le cœur doit
/// être un modèle électrique structuré, dont les vues graphiques ne sont que des
/// représentations.").
///
/// <see cref="Project"/> possède directement les catalogues plats (sources,
/// protections, câbles, charges, définitions de symboles, pages) référencés par
/// identifiant depuis les tableaux/circuits, ainsi que la liste des tableaux
/// (qui possèdent eux-mêmes leurs circuits). C'est la SEULE porte d'entrée pour
/// construire un projet cohérent : toutes les méthodes d'ajout valident les
/// références et l'unicité des repères avant de muter l'état.
/// </summary>
public sealed class Project : AuditableEntity
{
    public EntityId<Project> Id { get; }

    public ProjectMetadata Metadata { get; private set; }

    public ElectricalInstallation Installation { get; private set; }

    private readonly List<Source> _sources = [];
    public IReadOnlyList<Source> Sources => _sources;

    private readonly List<DistributionBoard> _boards = [];
    public IReadOnlyList<DistributionBoard> Boards => _boards;

    private readonly List<ProtectionDevice> _protectionDevices = [];
    public IReadOnlyList<ProtectionDevice> ProtectionDevices => _protectionDevices;

    private readonly List<Cable> _cables = [];
    public IReadOnlyList<Cable> Cables => _cables;

    private readonly List<Load> _loads = [];
    public IReadOnlyList<Load> Loads => _loads;

    private readonly List<ElectricalSymbolDefinition> _symbolDefinitions = [];
    public IReadOnlyList<ElectricalSymbolDefinition> SymbolDefinitions => _symbolDefinitions;

    private readonly List<SchematicPage> _schematicPages = [];
    public IReadOnlyList<SchematicPage> SchematicPages => _schematicPages;

    private readonly List<PositionPlan> _positionPlans = [];
    public IReadOnlyList<PositionPlan> PositionPlans => _positionPlans;

    private readonly List<ProjectRevision> _revisions = [];
    public IReadOnlyList<ProjectRevision> Revisions => _revisions;

    private Project(
        EntityId<Project> id,
        ProjectMetadata metadata,
        ElectricalInstallation installation,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc)
        : base(createdAtUtc, updatedAtUtc)
    {
        Id = id;
        Metadata = metadata;
        Installation = installation;
    }

    public static Project Create(
        EntityId<Project> id,
        ProjectMetadata metadata,
        ElectricalInstallation installation,
        DateTimeOffset nowUtc)
    {
        var metadataIssues = metadata.Validate().ToList();
        if (metadataIssues.Count > 0)
        {
            throw new DomainValidationException(
                metadataIssues.Select(m => new ValidationIssue("Project.Metadata", m)).ToList());
        }

        return new Project(id, metadata, installation, nowUtc, nowUtc);
    }

    // ---------------------------------------------------------------
    // Métadonnées
    // ---------------------------------------------------------------

    public void UpdateMetadata(ProjectMetadata metadata, DateTimeOffset nowUtc)
    {
        var issues = metadata.Validate().ToList();
        if (issues.Count > 0)
        {
            throw new DomainValidationException(
                issues.Select(m => new ValidationIssue("Project.Metadata", m)).ToList());
        }

        Metadata = metadata;
        Touch(nowUtc);
    }

    // ---------------------------------------------------------------
    // Sources
    // ---------------------------------------------------------------

    public Source AddSource(
        EntityId<Source> id,
        string name,
        SourceType sourceType,
        CurrentType currentType,
        double nominalVoltageV,
        DateTimeOffset nowUtc,
        string? notes = null)
    {
        var source = Source.Create(id, name, sourceType, currentType, nominalVoltageV, nowUtc, notes);
        _sources.Add(source);
        Touch(nowUtc);
        return source;
    }

    // ---------------------------------------------------------------
    // Tableaux
    // ---------------------------------------------------------------

    public DistributionBoard AddBoard(
        EntityId<DistributionBoard> id,
        string reference,
        string name,
        double voltageV,
        CurrentType phaseConfiguration,
        DateTimeOffset nowUtc,
        string? notes = null)
    {
        if (_boards.Any(b => string.Equals(b.Reference, reference.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException($"Un tableau avec la référence '{reference}' existe déjà dans le projet.", nameof(reference));
        }

        var board = DistributionBoard.Create(id, reference, name, voltageV, phaseConfiguration, nowUtc, notes);
        _boards.Add(board);
        Touch(nowUtc);
        return board;
    }

    public DistributionBoard GetBoard(EntityId<DistributionBoard> boardId) =>
        _boards.SingleOrDefault(b => b.Id == boardId)
        ?? throw new KeyNotFoundException($"Aucun tableau avec l'identifiant '{boardId}'.");

    // ---------------------------------------------------------------
    // Catalogues plats (protections, câbles, charges, symboles)
    // ---------------------------------------------------------------

    public ProtectionDevice AddProtectionDevice(
        EntityId<ProtectionDevice> id,
        string reference,
        ProtectionType type,
        double ratedCurrentA,
        DateTimeOffset nowUtc,
        double? breakingCapacityKa = null,
        double? residualCurrentSensitivityMa = null,
        ResidualCurrentType? residualCurrentType = null,
        string? manufacturerReference = null,
        string? notes = null)
    {
        if (_protectionDevices.Any(p => string.Equals(p.Reference, reference.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException($"Une protection avec la référence '{reference}' existe déjà dans le projet.", nameof(reference));
        }

        var protection = ProtectionDevice.Create(
            id, reference, type, ratedCurrentA, nowUtc, breakingCapacityKa,
            residualCurrentSensitivityMa, residualCurrentType, manufacturerReference, notes);
        _protectionDevices.Add(protection);
        Touch(nowUtc);
        return protection;
    }

    public Cable AddCable(
        EntityId<Cable> id,
        string reference,
        int phaseConductorCount,
        bool hasNeutral,
        bool hasProtectiveEarth,
        double crossSectionMm2,
        ConductorMaterial material,
        InsulationMaterial insulation,
        InstallationMethod installationMethod,
        double lengthM,
        DateTimeOffset nowUtc,
        string? notes = null)
    {
        if (_cables.Any(c => string.Equals(c.Reference, reference.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException($"Un câble avec la référence '{reference}' existe déjà dans le projet.", nameof(reference));
        }

        var cable = Cable.Create(
            id, reference, phaseConductorCount, hasNeutral, hasProtectiveEarth,
            crossSectionMm2, material, insulation, installationMethod, lengthM, nowUtc, notes);
        _cables.Add(cable);
        Touch(nowUtc);
        return cable;
    }

    public Load AddLoad(
        EntityId<Load> id,
        string label,
        LoadCategory category,
        double powerW,
        DateTimeOffset nowUtc,
        double powerFactor = 1.0,
        string? notes = null)
    {
        var load = Load.Create(id, label, category, powerW, nowUtc, powerFactor, notes);
        _loads.Add(load);
        Touch(nowUtc);
        return load;
    }

    public ElectricalSymbolDefinition AddSymbolDefinition(
        EntityId<ElectricalSymbolDefinition> id,
        string name,
        SymbolCategory category,
        DateTimeOffset nowUtc,
        string? description = null)
    {
        var definition = ElectricalSymbolDefinition.Create(id, name, category, nowUtc, description);
        _symbolDefinitions.Add(definition);
        Touch(nowUtc);
        return definition;
    }

    public SchematicPage AddSchematicPage(EntityId<SchematicPage> id, string title, int pageOrder, DateTimeOffset nowUtc)
    {
        var page = SchematicPage.Create(id, title, pageOrder, nowUtc);
        _schematicPages.Add(page);
        Touch(nowUtc);
        return page;
    }

    public PositionPlan AddPositionPlan(EntityId<PositionPlan> id, string title, int pageOrder, DateTimeOffset nowUtc)
    {
        var plan = PositionPlan.Create(id, title, pageOrder, nowUtc);
        _positionPlans.Add(plan);
        Touch(nowUtc);
        return plan;
    }

    // ---------------------------------------------------------------
    // Versioning (guide §8.3)
    // ---------------------------------------------------------------

    public ProjectRevision PublishRevision(
        EntityId<ProjectRevision> id,
        string author,
        string summary,
        DateTimeOffset nowUtc,
        string? contentHash = null,
        string? comment = null)
    {
        var nextVersionNumber = _revisions.Count == 0 ? 1 : _revisions.Max(r => r.VersionNumber) + 1;
        var revision = ProjectRevision.Create(id, nextVersionNumber, author, summary, Metadata.Status, nowUtc, contentHash, comment);
        _revisions.Add(revision);
        Touch(nowUtc);
        return revision;
    }

    // ---------------------------------------------------------------
    // Reconstruction depuis la persistance
    // ---------------------------------------------------------------

    /// <summary>
    /// Reconstruit un projet complet à partir de données déjà persistées, en
    /// préservant exactement tous les horodatages. Les sous-entités doivent
    /// avoir été préalablement reconstruites via leurs propres méthodes
    /// <c>Restore</c> respectives (<see cref="DistributionBoard.Restore"/>,
    /// <see cref="Circuit.Restore"/>, etc.). Utilisé uniquement par la couche
    /// Infrastructure (mapping de persistance) — jamais par le code applicatif,
    /// qui doit toujours passer par <see cref="Create"/> puis les méthodes
    /// <c>AddXxx</c>.
    /// </summary>
    public static Project Restore(
        EntityId<Project> id,
        ProjectMetadata metadata,
        ElectricalInstallation installation,
        IEnumerable<Source> sources,
        IEnumerable<DistributionBoard> boards,
        IEnumerable<ProtectionDevice> protectionDevices,
        IEnumerable<Cable> cables,
        IEnumerable<Load> loads,
        IEnumerable<ElectricalSymbolDefinition> symbolDefinitions,
        IEnumerable<SchematicPage> schematicPages,
        IEnumerable<PositionPlan> positionPlans,
        IEnumerable<ProjectRevision> revisions,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc)
    {
        var project = new Project(id, metadata, installation, createdAtUtc, updatedAtUtc);
        project._sources.AddRange(sources);
        project._boards.AddRange(boards);
        project._protectionDevices.AddRange(protectionDevices);
        project._cables.AddRange(cables);
        project._loads.AddRange(loads);
        project._symbolDefinitions.AddRange(symbolDefinitions);
        project._schematicPages.AddRange(schematicPages);
        project._positionPlans.AddRange(positionPlans);
        project._revisions.AddRange(revisions);
        return project;
    }

    // ---------------------------------------------------------------
    // Validation (structurelle — pas de règle RGIE ici, voir Phase 7)
    // ---------------------------------------------------------------

    /// <summary>
    /// Valide la cohérence structurelle complète du projet : invariants locaux
    /// de chaque entité, unicité des repères, et existence de toutes les
    /// références croisées (protectionId, cableId, sourceId, boardId, loadIds...).
    /// C'est le livrable "validation du modèle" de la Phase 1 (guide §2).
    /// </summary>
    public IReadOnlyList<ValidationIssue> Validate()
    {
        var issues = new List<ValidationIssue>();

        issues.AddRange(Metadata.Validate().Select(m => new ValidationIssue("Project.Metadata", m)));
        issues.AddRange(Installation.Validate());

        foreach (var source in _sources)
        {
            issues.AddRange(source.Validate());
        }

        foreach (var protectionDevice in _protectionDevices)
        {
            issues.AddRange(protectionDevice.Validate());
        }

        foreach (var cable in _cables)
        {
            issues.AddRange(cable.Validate());
        }

        foreach (var load in _loads)
        {
            issues.AddRange(load.Validate());
        }

        foreach (var symbolDefinition in _symbolDefinitions)
        {
            issues.AddRange(symbolDefinition.Validate());
        }

        foreach (var page in _schematicPages)
        {
            issues.AddRange(page.Validate());
        }

        foreach (var plan in _positionPlans)
        {
            issues.AddRange(plan.Validate());
        }

        // Unicité des repères de tableau (les repères de circuit sont déjà
        // vérifiés à l'intérieur de chaque DistributionBoard.Validate()).
        foreach (var duplicateBoardReference in _boards
                     .GroupBy(b => b.Reference, StringComparer.OrdinalIgnoreCase)
                     .Where(g => g.Count() > 1)
                     .Select(g => g.Key))
        {
            issues.Add(new ValidationIssue("Project.Boards", $"Référence de tableau dupliquée : '{duplicateBoardReference}'."));
        }

        foreach (var board in _boards)
        {
            issues.AddRange(board.Validate());
        }

        // Références croisées.
        var sourceIds = _sources.Select(s => s.Id).ToHashSet();
        var boardIds = _boards.Select(b => b.Id).ToHashSet();
        var protectionIds = _protectionDevices.Select(p => p.Id).ToHashSet();
        var cableIds = _cables.Select(c => c.Id).ToHashSet();
        var loadIds = _loads.Select(l => l.Id).ToHashSet();

        foreach (var board in _boards)
        {
            if (board.SourceId is { } sourceId && !sourceIds.Contains(sourceId))
            {
                issues.Add(new ValidationIssue($"Board[{board.Reference}].SourceId", "La source référencée n'existe pas dans le projet."));
            }

            if (board.UpstreamBoardId is { } upstreamBoardId && !boardIds.Contains(upstreamBoardId))
            {
                issues.Add(new ValidationIssue($"Board[{board.Reference}].UpstreamBoardId", "Le tableau amont référencé n'existe pas dans le projet."));
            }

            if (board.UpstreamProtectionId is { } upstreamProtectionId && !protectionIds.Contains(upstreamProtectionId))
            {
                issues.Add(new ValidationIssue($"Board[{board.Reference}].UpstreamProtectionId", "La protection amont référencée n'existe pas dans le projet."));
            }

            foreach (var circuit in board.Circuits)
            {
                var path = $"Circuit[{circuit.Reference}]";

                if (circuit.ProtectionId is { } protectionId && !protectionIds.Contains(protectionId))
                {
                    issues.Add(new ValidationIssue($"{path}.ProtectionId", "La protection référencée n'existe pas dans le projet."));
                }

                if (circuit.CableId is { } cableId && !cableIds.Contains(cableId))
                {
                    issues.Add(new ValidationIssue($"{path}.CableId", "Le câble référencé n'existe pas dans le projet."));
                }

                if (circuit.DestinationBoardId is { } destinationBoardId && !boardIds.Contains(destinationBoardId))
                {
                    issues.Add(new ValidationIssue($"{path}.DestinationBoardId", "Le tableau de destination référencé n'existe pas dans le projet."));
                }

                foreach (var loadId in circuit.LoadIds)
                {
                    if (!loadIds.Contains(loadId))
                    {
                        issues.Add(new ValidationIssue($"{path}.LoadIds", $"La charge référencée '{loadId}' n'existe pas dans le projet."));
                    }
                }
            }
        }

        return issues;
    }
}
