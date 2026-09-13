namespace ElectricalDesigner.Infrastructure.Persistence.Dtos;

// ---------------------------------------------------------------------------
// DTOs de sérialisation, distincts des entités Domain (docs/03_File_Format.md).
//
// Pourquoi des DTOs séparés plutôt que sérialiser les entités Domain
// directement ? Deux raisons :
//  1. Le Domain reste totalement ignorant de la sérialisation (guide §3.1) —
//     aucune annotation JSON, aucune dépendance System.Text.Json dans Domain.
//  2. Le format de fichier peut évoluer indépendamment de la forme interne du
//     modèle métier (guide §30 : "toute évolution doit être accompagnée d'une
//     migration" — v1 → v2 → v3). Découpler les deux rend cette évolution
//     possible sans casser le modèle métier, et vice versa.
//
// Tous les identifiants sont de simples Guid ici : c'est un choix délibéré,
// propre à la couche Infrastructure — la sécurité de type apportée par
// EntityId<T> (Domain.Common) n'a de sens que dans le code métier.
// ---------------------------------------------------------------------------

public sealed record ManifestDto
{
    public required int FormatVersion { get; init; }
    public required string ApplicationVersion { get; init; }
    public string? RegulatoryPackVersion { get; init; }
    public required Guid ProjectId { get; init; }
    public required DateTimeOffset SavedAtUtc { get; init; }
    public string? ContentHash { get; init; }
}

public sealed record ProjectMetadataDto
{
    public required string Name { get; init; }
    public string? FileNumber { get; init; }
    public string? Address { get; init; }
    public string? BuildingUnit { get; init; }
    public string? Author { get; init; }
    public string? ExecutionManagerName { get; init; }
    public string? CompanyName { get; init; }
    public string? VatNumber { get; init; }
    public string? ClientName { get; init; }
    public string? InspectionBody { get; init; }
    public required string Language { get; init; }
    public string? Comments { get; init; }
    public required string Status { get; init; }
}

public sealed record InstallationDto
{
    public required Guid Id { get; init; }
    public required string Category { get; init; }
    public required string CurrentType { get; init; }
    public required string EarthingSystem { get; init; }
    public required double NominalVoltageV { get; init; }
    public required double FrequencyHz { get; init; }
    public string? Notes { get; init; }
    public required DateTimeOffset CreatedAtUtc { get; init; }
    public required DateTimeOffset UpdatedAtUtc { get; init; }
}

public sealed record SourceDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string SourceType { get; init; }
    public required string CurrentType { get; init; }
    public required double NominalVoltageV { get; init; }
    public string? Notes { get; init; }
    public required DateTimeOffset CreatedAtUtc { get; init; }
    public required DateTimeOffset UpdatedAtUtc { get; init; }
}

public sealed record ProtectionDeviceDto
{
    public required Guid Id { get; init; }
    public required string Reference { get; init; }
    public required string Type { get; init; }
    public required double RatedCurrentA { get; init; }
    public double? BreakingCapacityKa { get; init; }
    public double? ResidualCurrentSensitivityMa { get; init; }
    public string? ResidualCurrentType { get; init; }
    public string? ManufacturerReference { get; init; }
    public string? Notes { get; init; }
    public required DateTimeOffset CreatedAtUtc { get; init; }
    public required DateTimeOffset UpdatedAtUtc { get; init; }
}

public sealed record CableDto
{
    public required Guid Id { get; init; }
    public required string Reference { get; init; }
    public required int PhaseConductorCount { get; init; }
    public required bool HasNeutral { get; init; }
    public required bool HasProtectiveEarth { get; init; }
    public required double CrossSectionMm2 { get; init; }
    public required string Material { get; init; }
    public required string Insulation { get; init; }
    public required string InstallationMethod { get; init; }
    public required double LengthM { get; init; }
    public string? Notes { get; init; }
    public required DateTimeOffset CreatedAtUtc { get; init; }
    public required DateTimeOffset UpdatedAtUtc { get; init; }
}

public sealed record LoadDto
{
    public required Guid Id { get; init; }
    public required string Label { get; init; }
    public required string Category { get; init; }
    public required double PowerW { get; init; }
    public required double PowerFactor { get; init; }
    public string? Notes { get; init; }
    public required DateTimeOffset CreatedAtUtc { get; init; }
    public required DateTimeOffset UpdatedAtUtc { get; init; }
}

public sealed record CircuitDto
{
    public required Guid Id { get; init; }
    public required string Reference { get; init; }
    public required string Name { get; init; }
    public required string Phase { get; init; }
    public required double VoltageV { get; init; }
    public required double FrequencyHz { get; init; }
    public Guid? ProtectionId { get; init; }
    public Guid? CableId { get; init; }
    public Guid? DestinationBoardId { get; init; }
    public required IReadOnlyList<Guid> LoadIds { get; init; }
    public required IReadOnlyList<Guid> PositionRepresentationIds { get; init; }
    public string? Notes { get; init; }
    public required DateTimeOffset CreatedAtUtc { get; init; }
    public required DateTimeOffset UpdatedAtUtc { get; init; }
}

public sealed record BoardDto
{
    public required Guid Id { get; init; }
    public required string Reference { get; init; }
    public required string Name { get; init; }
    public required double VoltageV { get; init; }
    public required string PhaseConfiguration { get; init; }
    public Guid? SourceId { get; init; }
    public Guid? UpstreamBoardId { get; init; }
    public Guid? UpstreamProtectionId { get; init; }
    public double? PresumedShortCircuitCurrentKa { get; init; }
    public required IReadOnlyList<CircuitDto> Circuits { get; init; }
    public string? Notes { get; init; }
    public required DateTimeOffset CreatedAtUtc { get; init; }
    public required DateTimeOffset UpdatedAtUtc { get; init; }
}

public sealed record SymbolDefinitionDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Category { get; init; }
    public string? Description { get; init; }
    public required DateTimeOffset CreatedAtUtc { get; init; }
    public required DateTimeOffset UpdatedAtUtc { get; init; }
}

public sealed record PageDto
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required int PageOrder { get; init; }
    public required IReadOnlyList<Guid> SymbolInstanceIds { get; init; }
    public required DateTimeOffset CreatedAtUtc { get; init; }
    public required DateTimeOffset UpdatedAtUtc { get; init; }
}

public sealed record ProjectRevisionDto
{
    public required Guid Id { get; init; }
    public required int VersionNumber { get; init; }
    public required string Author { get; init; }
    public required string Summary { get; init; }
    public string? ContentHash { get; init; }
    public required string StatusAtPublication { get; init; }
    public string? Comment { get; init; }
    public required DateTimeOffset PublishedAtUtc { get; init; }
}

public sealed record ProjectDto
{
    public required Guid Id { get; init; }
    public required ProjectMetadataDto Metadata { get; init; }
    public required InstallationDto Installation { get; init; }
    public required IReadOnlyList<SourceDto> Sources { get; init; }
    public required IReadOnlyList<BoardDto> Boards { get; init; }
    public required IReadOnlyList<ProtectionDeviceDto> ProtectionDevices { get; init; }
    public required IReadOnlyList<CableDto> Cables { get; init; }
    public required IReadOnlyList<LoadDto> Loads { get; init; }
    public required IReadOnlyList<SymbolDefinitionDto> SymbolDefinitions { get; init; }
    public required IReadOnlyList<PageDto> SchematicPages { get; init; }
    public required IReadOnlyList<PageDto> PositionPlans { get; init; }
    public required IReadOnlyList<ProjectRevisionDto> Revisions { get; init; }
    public required DateTimeOffset CreatedAtUtc { get; init; }
    public required DateTimeOffset UpdatedAtUtc { get; init; }
}
