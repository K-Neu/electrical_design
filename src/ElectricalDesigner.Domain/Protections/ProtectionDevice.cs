using ElectricalDesigner.Domain.Common;

namespace ElectricalDesigner.Domain.Protections;

/// <summary>
/// Disjoncteur, fusible, différentiel, parafoudre ou protection spécialisée
/// (cahier §9). Les paramètres de calcul fin (pouvoir de coupure, courbe de
/// déclenchement, coordination/sélectivité) seront enrichis en Phase 8
/// (moteur de calcul) ; seuls les attributs structurels nécessaires à la
/// Phase 1 sont modélisés ici.
/// </summary>
public sealed class ProtectionDevice : AuditableEntity
{
    public EntityId<ProtectionDevice> Id { get; }

    /// <summary>Repère du dispositif (ex. "Q1", "DDR-A"). Doit être unique au sein d'un projet.</summary>
    public string Reference { get; private set; }

    public ProtectionType Type { get; private set; }

    /// <summary>Courant assigné (In) en ampères.</summary>
    public double RatedCurrentA { get; private set; }

    /// <summary>Pouvoir de coupure en kA, si connu (cahier §21.4).</summary>
    public double? BreakingCapacityKa { get; private set; }

    /// <summary>Sensibilité différentielle en mA — uniquement pour un <see cref="ProtectionType.ResidualCurrentDevice"/>.</summary>
    public double? ResidualCurrentSensitivityMa { get; private set; }

    public ResidualCurrentType? ResidualCurrentType { get; private set; }

    public string? ManufacturerReference { get; private set; }

    public string? Notes { get; private set; }

    private ProtectionDevice(
        EntityId<ProtectionDevice> id,
        string reference,
        ProtectionType type,
        double ratedCurrentA,
        double? breakingCapacityKa,
        double? residualCurrentSensitivityMa,
        ResidualCurrentType? residualCurrentType,
        string? manufacturerReference,
        string? notes,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc)
        : base(createdAtUtc, updatedAtUtc)
    {
        Id = id;
        Reference = reference;
        Type = type;
        RatedCurrentA = ratedCurrentA;
        BreakingCapacityKa = breakingCapacityKa;
        ResidualCurrentSensitivityMa = residualCurrentSensitivityMa;
        ResidualCurrentType = residualCurrentType;
        ManufacturerReference = manufacturerReference;
        Notes = notes;
    }

    public static ProtectionDevice Create(
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
        if (string.IsNullOrWhiteSpace(reference))
        {
            throw new ArgumentException("La référence de la protection est obligatoire.", nameof(reference));
        }

        if (ratedCurrentA <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(ratedCurrentA), "Le courant assigné doit être positif.");
        }

        if (type == ProtectionType.ResidualCurrentDevice && residualCurrentSensitivityMa is null)
        {
            throw new ArgumentException(
                "Un différentiel doit avoir une sensibilité résiduelle (mA).",
                nameof(residualCurrentSensitivityMa));
        }

        return new ProtectionDevice(
            id, reference.Trim(), type, ratedCurrentA, breakingCapacityKa,
            residualCurrentSensitivityMa, residualCurrentType, manufacturerReference, notes,
            nowUtc, nowUtc);
    }

    /// <summary>Reconstruction depuis la persistance — voir <see cref="Installations.ElectricalInstallation.Restore"/>.</summary>
    public static ProtectionDevice Restore(
        EntityId<ProtectionDevice> id,
        string reference,
        ProtectionType type,
        double ratedCurrentA,
        double? breakingCapacityKa,
        double? residualCurrentSensitivityMa,
        ResidualCurrentType? residualCurrentType,
        string? manufacturerReference,
        string? notes,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc) =>
        new(id, reference, type, ratedCurrentA, breakingCapacityKa, residualCurrentSensitivityMa,
            residualCurrentType, manufacturerReference, notes, createdAtUtc, updatedAtUtc);

    public IReadOnlyList<ValidationIssue> Validate()
    {
        var issues = new List<ValidationIssue>();
        var path = $"ProtectionDevice[{Reference}]";

        if (string.IsNullOrWhiteSpace(Reference))
        {
            issues.Add(new ValidationIssue(path, "La référence est obligatoire."));
        }

        if (RatedCurrentA <= 0)
        {
            issues.Add(new ValidationIssue($"{path}.RatedCurrentA", "Le courant assigné doit être positif."));
        }

        if (Type == ProtectionType.ResidualCurrentDevice && ResidualCurrentSensitivityMa is null)
        {
            issues.Add(new ValidationIssue(
                $"{path}.ResidualCurrentSensitivityMa",
                "Un différentiel doit avoir une sensibilité résiduelle renseignée."));
        }

        return issues;
    }
}
