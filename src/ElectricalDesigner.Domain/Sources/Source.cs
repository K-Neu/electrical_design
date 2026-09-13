using ElectricalDesigner.Domain.Common;
using ElectricalDesigner.Domain.Installations;

namespace ElectricalDesigner.Domain.Sources;

/// <summary>
/// Réseau, transformateur, photovoltaïque, onduleur, batterie, générateur
/// (cahier §9, entité Source ; cahier §23 pour le PV/batteries/bornes de recharge).
/// </summary>
public sealed class Source : AuditableEntity
{
    public EntityId<Source> Id { get; }

    public string Name { get; private set; }

    public SourceType SourceType { get; private set; }

    public CurrentType CurrentType { get; private set; }

    public double NominalVoltageV { get; private set; }

    public string? Notes { get; private set; }

    private Source(
        EntityId<Source> id,
        string name,
        SourceType sourceType,
        CurrentType currentType,
        double nominalVoltageV,
        string? notes,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc)
        : base(createdAtUtc, updatedAtUtc)
    {
        Id = id;
        Name = name;
        SourceType = sourceType;
        CurrentType = currentType;
        NominalVoltageV = nominalVoltageV;
        Notes = notes;
    }

    public static Source Create(
        EntityId<Source> id,
        string name,
        SourceType sourceType,
        CurrentType currentType,
        double nominalVoltageV,
        DateTimeOffset nowUtc,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Le nom de la source est obligatoire.", nameof(name));
        }

        if (nominalVoltageV <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(nominalVoltageV), "La tension nominale doit être positive.");
        }

        return new Source(id, name.Trim(), sourceType, currentType, nominalVoltageV, notes, nowUtc, nowUtc);
    }

    /// <summary>Reconstruction depuis la persistance — voir <see cref="Installations.ElectricalInstallation.Restore"/>.</summary>
    public static Source Restore(
        EntityId<Source> id,
        string name,
        SourceType sourceType,
        CurrentType currentType,
        double nominalVoltageV,
        string? notes,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc) =>
        new(id, name, sourceType, currentType, nominalVoltageV, notes, createdAtUtc, updatedAtUtc);

    public void Rename(string name, DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Le nom de la source est obligatoire.", nameof(name));
        }

        Name = name.Trim();
        Touch(nowUtc);
    }

    public IReadOnlyList<ValidationIssue> Validate()
    {
        var issues = new List<ValidationIssue>();

        if (string.IsNullOrWhiteSpace(Name))
        {
            issues.Add(new ValidationIssue($"Source[{Id}].Name", "Le nom de la source est obligatoire."));
        }

        if (NominalVoltageV <= 0)
        {
            issues.Add(new ValidationIssue($"Source[{Id}].NominalVoltageV", "La tension nominale doit être positive."));
        }

        return issues;
    }
}
