using ElectricalDesigner.Domain.Boards;
using ElectricalDesigner.Domain.Cabling;
using ElectricalDesigner.Domain.Circuits;
using ElectricalDesigner.Domain.Common;
using ElectricalDesigner.Domain.Installations;
using ElectricalDesigner.Domain.Loads;
using ElectricalDesigner.Domain.Plans;
using ElectricalDesigner.Domain.Projects;
using ElectricalDesigner.Domain.Protections;
using ElectricalDesigner.Domain.Sources;
using ElectricalDesigner.Domain.Symbols;
using ElectricalDesigner.Infrastructure.Persistence.Dtos;

namespace ElectricalDesigner.Infrastructure.Persistence;

/// <summary>
/// Conversion bidirectionnelle entre l'agrégat <see cref="Project"/> (Domain) et
/// son arbre de DTOs (persistance). <see cref="ToDomain"/> passe systématiquement
/// par les méthodes <c>Restore</c> de chaque entité (jamais par <c>Create</c>),
/// afin de préserver exactement les horodatages persistés — voir la remarque
/// dans <see cref="Project.Restore"/>.
/// </summary>
public static class ProjectDtoMapper
{
    // -----------------------------------------------------------------
    // Domain -> DTO
    // -----------------------------------------------------------------

    public static ProjectDto ToDto(Project project) => new()
    {
        Id = project.Id.Value,
        Metadata = ToDto(project.Metadata),
        Installation = ToDto(project.Installation),
        Sources = project.Sources.Select(ToDto).ToList(),
        Boards = project.Boards.Select(ToDto).ToList(),
        ProtectionDevices = project.ProtectionDevices.Select(ToDto).ToList(),
        Cables = project.Cables.Select(ToDto).ToList(),
        Loads = project.Loads.Select(ToDto).ToList(),
        SymbolDefinitions = project.SymbolDefinitions.Select(ToDto).ToList(),
        SchematicPages = project.SchematicPages.Select(ToDto).ToList(),
        PositionPlans = project.PositionPlans.Select(ToDto).ToList(),
        Revisions = project.Revisions.Select(ToDto).ToList(),
        CreatedAtUtc = project.CreatedAtUtc,
        UpdatedAtUtc = project.UpdatedAtUtc,
    };

    private static ProjectMetadataDto ToDto(ProjectMetadata metadata) => new()
    {
        Name = metadata.Name,
        FileNumber = metadata.FileNumber,
        Address = metadata.Address,
        BuildingUnit = metadata.BuildingUnit,
        Author = metadata.Author,
        ExecutionManagerName = metadata.ExecutionManagerName,
        CompanyName = metadata.CompanyName,
        VatNumber = metadata.VatNumber,
        ClientName = metadata.ClientName,
        InspectionBody = metadata.InspectionBody,
        Language = metadata.Language.ToString(),
        Comments = metadata.Comments,
        Status = metadata.Status.ToString(),
    };

    private static InstallationDto ToDto(ElectricalInstallation installation) => new()
    {
        Id = installation.Id.Value,
        Category = installation.Category.ToString(),
        CurrentType = installation.CurrentType.ToString(),
        EarthingSystem = installation.EarthingSystem.ToString(),
        NominalVoltageV = installation.NominalVoltageV,
        FrequencyHz = installation.FrequencyHz,
        Notes = installation.Notes,
        CreatedAtUtc = installation.CreatedAtUtc,
        UpdatedAtUtc = installation.UpdatedAtUtc,
    };

    private static SourceDto ToDto(Source source) => new()
    {
        Id = source.Id.Value,
        Name = source.Name,
        SourceType = source.SourceType.ToString(),
        CurrentType = source.CurrentType.ToString(),
        NominalVoltageV = source.NominalVoltageV,
        Notes = source.Notes,
        CreatedAtUtc = source.CreatedAtUtc,
        UpdatedAtUtc = source.UpdatedAtUtc,
    };

    private static ProtectionDeviceDto ToDto(ProtectionDevice protection) => new()
    {
        Id = protection.Id.Value,
        Reference = protection.Reference,
        Type = protection.Type.ToString(),
        RatedCurrentA = protection.RatedCurrentA,
        BreakingCapacityKa = protection.BreakingCapacityKa,
        ResidualCurrentSensitivityMa = protection.ResidualCurrentSensitivityMa,
        ResidualCurrentType = protection.ResidualCurrentType?.ToString(),
        ManufacturerReference = protection.ManufacturerReference,
        Notes = protection.Notes,
        CreatedAtUtc = protection.CreatedAtUtc,
        UpdatedAtUtc = protection.UpdatedAtUtc,
    };

    private static CableDto ToDto(Cable cable) => new()
    {
        Id = cable.Id.Value,
        Reference = cable.Reference,
        PhaseConductorCount = cable.PhaseConductorCount,
        HasNeutral = cable.HasNeutral,
        HasProtectiveEarth = cable.HasProtectiveEarth,
        CrossSectionMm2 = cable.CrossSectionMm2,
        Material = cable.Material.ToString(),
        Insulation = cable.Insulation.ToString(),
        InstallationMethod = cable.InstallationMethod.ToString(),
        LengthM = cable.LengthM,
        Notes = cable.Notes,
        CreatedAtUtc = cable.CreatedAtUtc,
        UpdatedAtUtc = cable.UpdatedAtUtc,
    };

    private static LoadDto ToDto(Load load) => new()
    {
        Id = load.Id.Value,
        Label = load.Label,
        Category = load.Category.ToString(),
        PowerW = load.PowerW,
        PowerFactor = load.PowerFactor,
        Notes = load.Notes,
        CreatedAtUtc = load.CreatedAtUtc,
        UpdatedAtUtc = load.UpdatedAtUtc,
    };

    private static CircuitDto ToDto(Circuit circuit) => new()
    {
        Id = circuit.Id.Value,
        Reference = circuit.Reference,
        Name = circuit.Name,
        Phase = circuit.Phase.ToString(),
        VoltageV = circuit.VoltageV,
        FrequencyHz = circuit.FrequencyHz,
        ProtectionId = circuit.ProtectionId?.Value,
        CableId = circuit.CableId?.Value,
        DestinationBoardId = circuit.DestinationBoardId?.Value,
        LoadIds = circuit.LoadIds.Select(l => l.Value).ToList(),
        PositionRepresentationIds = circuit.PositionRepresentationIds.Select(p => p.Value).ToList(),
        Notes = circuit.Notes,
        CreatedAtUtc = circuit.CreatedAtUtc,
        UpdatedAtUtc = circuit.UpdatedAtUtc,
    };

    private static BoardDto ToDto(DistributionBoard board) => new()
    {
        Id = board.Id.Value,
        Reference = board.Reference,
        Name = board.Name,
        VoltageV = board.VoltageV,
        PhaseConfiguration = board.PhaseConfiguration.ToString(),
        SourceId = board.SourceId?.Value,
        UpstreamBoardId = board.UpstreamBoardId?.Value,
        UpstreamProtectionId = board.UpstreamProtectionId?.Value,
        PresumedShortCircuitCurrentKa = board.PresumedShortCircuitCurrentKa,
        Circuits = board.Circuits.Select(ToDto).ToList(),
        Notes = board.Notes,
        CreatedAtUtc = board.CreatedAtUtc,
        UpdatedAtUtc = board.UpdatedAtUtc,
    };

    private static SymbolDefinitionDto ToDto(ElectricalSymbolDefinition definition) => new()
    {
        Id = definition.Id.Value,
        Name = definition.Name,
        Category = definition.Category.ToString(),
        Description = definition.Description,
        CreatedAtUtc = definition.CreatedAtUtc,
        UpdatedAtUtc = definition.UpdatedAtUtc,
    };

    private static PageDto ToDto(SchematicPage page) => new()
    {
        Id = page.Id.Value,
        Title = page.Title,
        PageOrder = page.PageOrder,
        SymbolInstanceIds = page.SymbolInstanceIds.Select(s => s.Value).ToList(),
        CreatedAtUtc = page.CreatedAtUtc,
        UpdatedAtUtc = page.UpdatedAtUtc,
    };

    private static PageDto ToDto(PositionPlan plan) => new()
    {
        Id = plan.Id.Value,
        Title = plan.Title,
        PageOrder = plan.PageOrder,
        SymbolInstanceIds = plan.SymbolInstanceIds.Select(s => s.Value).ToList(),
        CreatedAtUtc = plan.CreatedAtUtc,
        UpdatedAtUtc = plan.UpdatedAtUtc,
    };

    private static ProjectRevisionDto ToDto(ProjectRevision revision) => new()
    {
        Id = revision.Id.Value,
        VersionNumber = revision.VersionNumber,
        Author = revision.Author,
        Summary = revision.Summary,
        ContentHash = revision.ContentHash,
        StatusAtPublication = revision.StatusAtPublication.ToString(),
        Comment = revision.Comment,
        PublishedAtUtc = revision.CreatedAtUtc,
    };

    // -----------------------------------------------------------------
    // DTO -> Domain
    // -----------------------------------------------------------------

    public static Project ToDomain(ProjectDto dto)
    {
        var installation = ToDomainInstallation(dto.Installation);
        var boards = dto.Boards.Select(ToDomainBoard).ToList();

        return Project.Restore(
            new EntityId<Project>(dto.Id),
            ToDomainMetadata(dto.Metadata),
            installation,
            dto.Sources.Select(ToDomainSource),
            boards,
            dto.ProtectionDevices.Select(ToDomainProtectionDevice),
            dto.Cables.Select(ToDomainCable),
            dto.Loads.Select(ToDomainLoad),
            dto.SymbolDefinitions.Select(ToDomainSymbolDefinition),
            dto.SchematicPages.Select(ToDomainSchematicPage),
            dto.PositionPlans.Select(ToDomainPositionPlan),
            dto.Revisions.Select(ToDomainRevision),
            dto.CreatedAtUtc,
            dto.UpdatedAtUtc);
    }

    private static ProjectMetadata ToDomainMetadata(ProjectMetadataDto dto) => new()
    {
        Name = dto.Name,
        FileNumber = dto.FileNumber,
        Address = dto.Address,
        BuildingUnit = dto.BuildingUnit,
        Author = dto.Author,
        ExecutionManagerName = dto.ExecutionManagerName,
        CompanyName = dto.CompanyName,
        VatNumber = dto.VatNumber,
        ClientName = dto.ClientName,
        InspectionBody = dto.InspectionBody,
        Language = Enum.Parse<ProjectLanguage>(dto.Language),
        Comments = dto.Comments,
        Status = Enum.Parse<ProjectStatus>(dto.Status),
    };

    private static ElectricalInstallation ToDomainInstallation(InstallationDto dto) => ElectricalInstallation.Restore(
        new EntityId<ElectricalInstallation>(dto.Id),
        Enum.Parse<InstallationCategory>(dto.Category),
        Enum.Parse<CurrentType>(dto.CurrentType),
        Enum.Parse<EarthingSystem>(dto.EarthingSystem),
        dto.NominalVoltageV,
        dto.FrequencyHz,
        dto.Notes,
        dto.CreatedAtUtc,
        dto.UpdatedAtUtc);

    private static Source ToDomainSource(SourceDto dto) => Source.Restore(
        new EntityId<Source>(dto.Id),
        dto.Name,
        Enum.Parse<SourceType>(dto.SourceType),
        Enum.Parse<CurrentType>(dto.CurrentType),
        dto.NominalVoltageV,
        dto.Notes,
        dto.CreatedAtUtc,
        dto.UpdatedAtUtc);

    private static ProtectionDevice ToDomainProtectionDevice(ProtectionDeviceDto dto) => ProtectionDevice.Restore(
        new EntityId<ProtectionDevice>(dto.Id),
        dto.Reference,
        Enum.Parse<ProtectionType>(dto.Type),
        dto.RatedCurrentA,
        dto.BreakingCapacityKa,
        dto.ResidualCurrentSensitivityMa,
        dto.ResidualCurrentType is null ? null : Enum.Parse<ResidualCurrentType>(dto.ResidualCurrentType),
        dto.ManufacturerReference,
        dto.Notes,
        dto.CreatedAtUtc,
        dto.UpdatedAtUtc);

    private static Cable ToDomainCable(CableDto dto) => Cable.Restore(
        new EntityId<Cable>(dto.Id),
        dto.Reference,
        dto.PhaseConductorCount,
        dto.HasNeutral,
        dto.HasProtectiveEarth,
        dto.CrossSectionMm2,
        Enum.Parse<ConductorMaterial>(dto.Material),
        Enum.Parse<InsulationMaterial>(dto.Insulation),
        Enum.Parse<InstallationMethod>(dto.InstallationMethod),
        dto.LengthM,
        dto.Notes,
        dto.CreatedAtUtc,
        dto.UpdatedAtUtc);

    private static Load ToDomainLoad(LoadDto dto) => Load.Restore(
        new EntityId<Load>(dto.Id),
        dto.Label,
        Enum.Parse<LoadCategory>(dto.Category),
        dto.PowerW,
        dto.PowerFactor,
        dto.Notes,
        dto.CreatedAtUtc,
        dto.UpdatedAtUtc);

    private static ElectricalSymbolDefinition ToDomainSymbolDefinition(SymbolDefinitionDto dto) => ElectricalSymbolDefinition.Restore(
        new EntityId<ElectricalSymbolDefinition>(dto.Id),
        dto.Name,
        Enum.Parse<SymbolCategory>(dto.Category),
        dto.Description,
        dto.CreatedAtUtc,
        dto.UpdatedAtUtc);

    private static SchematicPage ToDomainSchematicPage(PageDto dto) => SchematicPage.Restore(
        new EntityId<SchematicPage>(dto.Id),
        dto.Title,
        dto.PageOrder,
        dto.SymbolInstanceIds.Select(id => new EntityId<ElectricalSymbolInstance>(id)),
        dto.CreatedAtUtc,
        dto.UpdatedAtUtc);

    private static PositionPlan ToDomainPositionPlan(PageDto dto) => PositionPlan.Restore(
        new EntityId<PositionPlan>(dto.Id),
        dto.Title,
        dto.PageOrder,
        dto.SymbolInstanceIds.Select(id => new EntityId<ElectricalSymbolInstance>(id)),
        dto.CreatedAtUtc,
        dto.UpdatedAtUtc);

    private static ProjectRevision ToDomainRevision(ProjectRevisionDto dto) => ProjectRevision.Create(
        new EntityId<ProjectRevision>(dto.Id),
        dto.VersionNumber,
        dto.Author,
        dto.Summary,
        Enum.Parse<ProjectStatus>(dto.StatusAtPublication),
        dto.PublishedAtUtc,
        dto.ContentHash,
        dto.Comment);

    private static Circuit ToDomainCircuit(CircuitDto dto, EntityId<DistributionBoard> sourceBoardId) => Circuit.Restore(
        new EntityId<Circuit>(dto.Id),
        dto.Reference,
        dto.Name,
        Enum.Parse<CurrentType>(dto.Phase),
        dto.VoltageV,
        dto.FrequencyHz,
        sourceBoardId,
        dto.ProtectionId is null ? null : new EntityId<ProtectionDevice>(dto.ProtectionId.Value),
        dto.CableId is null ? null : new EntityId<Cable>(dto.CableId.Value),
        dto.DestinationBoardId is null ? null : new EntityId<DistributionBoard>(dto.DestinationBoardId.Value),
        dto.LoadIds.Select(id => new EntityId<Load>(id)),
        dto.PositionRepresentationIds.Select(id => new EntityId<ElectricalSymbolInstance>(id)),
        calculationData: null,
        dto.Notes,
        dto.CreatedAtUtc,
        dto.UpdatedAtUtc);

    private static DistributionBoard ToDomainBoard(BoardDto dto)
    {
        var boardId = new EntityId<DistributionBoard>(dto.Id);
        var circuits = dto.Circuits.Select(c => ToDomainCircuit(c, boardId)).ToList();

        return DistributionBoard.Restore(
            boardId,
            dto.Reference,
            dto.Name,
            dto.VoltageV,
            Enum.Parse<CurrentType>(dto.PhaseConfiguration),
            dto.SourceId is null ? null : new EntityId<Source>(dto.SourceId.Value),
            dto.UpstreamBoardId is null ? null : new EntityId<DistributionBoard>(dto.UpstreamBoardId.Value),
            dto.UpstreamProtectionId is null ? null : new EntityId<ProtectionDevice>(dto.UpstreamProtectionId.Value),
            dto.PresumedShortCircuitCurrentKa,
            circuits,
            dto.Notes,
            dto.CreatedAtUtc,
            dto.UpdatedAtUtc);
    }
}
