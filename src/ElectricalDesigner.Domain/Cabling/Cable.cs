using ElectricalDesigner.Domain.Common;

namespace ElectricalDesigner.Domain.Cabling;

/// <summary>
/// Type, nombre de conducteurs, section, matériau, isolation, pose, longueur
/// et repérage (cahier §9, entité Cable/Conduit/ConductorSet).
/// </summary>
public sealed class Cable : AuditableEntity
{
    public EntityId<Cable> Id { get; }

    /// <summary>Repère de la canalisation (ex. "C07"). Doit être unique au sein d'un projet.</summary>
    public string Reference { get; private set; }

    /// <summary>Nombre de conducteurs de phase (1 = monophasé/DC, 3 = triphasé).</summary>
    public int PhaseConductorCount { get; private set; }

    public bool HasNeutral { get; private set; }

    public bool HasProtectiveEarth { get; private set; }

    /// <summary>Section en mm² (identique pour tous les conducteurs en Phase 1 — sections distinctes N/PE en Phase 8).</summary>
    public double CrossSectionMm2 { get; private set; }

    public ConductorMaterial Material { get; private set; }

    public InsulationMaterial Insulation { get; private set; }

    public InstallationMethod InstallationMethod { get; private set; }

    public double LengthM { get; private set; }

    public string? Notes { get; private set; }

    private Cable(
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
        string? notes,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc)
        : base(createdAtUtc, updatedAtUtc)
    {
        Id = id;
        Reference = reference;
        PhaseConductorCount = phaseConductorCount;
        HasNeutral = hasNeutral;
        HasProtectiveEarth = hasProtectiveEarth;
        CrossSectionMm2 = crossSectionMm2;
        Material = material;
        Insulation = insulation;
        InstallationMethod = installationMethod;
        LengthM = lengthM;
        Notes = notes;
    }

    public static Cable Create(
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
        if (string.IsNullOrWhiteSpace(reference))
        {
            throw new ArgumentException("La référence du câble est obligatoire.", nameof(reference));
        }

        if (phaseConductorCount is < 1 or > 3)
        {
            throw new ArgumentOutOfRangeException(nameof(phaseConductorCount), "Le nombre de conducteurs de phase doit être 1, 2 ou 3.");
        }

        if (crossSectionMm2 <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(crossSectionMm2), "La section doit être positive.");
        }

        if (lengthM <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lengthM), "La longueur doit être positive.");
        }

        return new Cable(
            id, reference.Trim(), phaseConductorCount, hasNeutral, hasProtectiveEarth,
            crossSectionMm2, material, insulation, installationMethod, lengthM, notes,
            nowUtc, nowUtc);
    }

    /// <summary>Reconstruction depuis la persistance (préserve les horodatages exacts).</summary>
    public static Cable Restore(
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
        string? notes,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc) =>
        new(id, reference, phaseConductorCount, hasNeutral, hasProtectiveEarth, crossSectionMm2,
            material, insulation, installationMethod, lengthM, notes, createdAtUtc, updatedAtUtc);

    /// <summary>Notation usuelle du câble, ex. "3G2.5" pour 3 conducteurs de 2.5 mm² dont un de terre.</summary>
    public string ToShortNotation()
    {
        var conductorCount = PhaseConductorCount + (HasNeutral ? 1 : 0) + (HasProtectiveEarth ? 1 : 0);
        var suffix = HasProtectiveEarth ? "G" : (HasNeutral ? "N" : string.Empty);
        return $"{conductorCount}{suffix}{CrossSectionMm2:0.##}";
    }

    public IReadOnlyList<ValidationIssue> Validate()
    {
        var issues = new List<ValidationIssue>();
        var path = $"Cable[{Reference}]";

        if (string.IsNullOrWhiteSpace(Reference))
        {
            issues.Add(new ValidationIssue(path, "La référence est obligatoire."));
        }

        if (CrossSectionMm2 <= 0)
        {
            issues.Add(new ValidationIssue($"{path}.CrossSectionMm2", "La section doit être positive."));
        }

        if (LengthM <= 0)
        {
            issues.Add(new ValidationIssue($"{path}.LengthM", "La longueur doit être positive."));
        }

        return issues;
    }
}
