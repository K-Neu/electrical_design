using ElectricalDesigner.Domain.Common;

namespace ElectricalDesigner.Domain.Loads;

/// <summary>
/// Charge ou appareil fixe avec puissance, courant, cos phi, catégorie et
/// circuit d'alimentation (cahier §9, entité Load/FixedAppliance).
///
/// Choix de conception : contrairement au cahier qui suggère une propriété
/// "circuit d'alimentation" directement sur la charge, ce modèle Phase 1
/// place la relation du côté de <c>Circuit.LoadIds</c> (guide §5.2 : Circuit
/// porte <c>loads[]</c>). Une seule source de vérité pour cette relation
/// évite un état incohérent (une charge qui prétendrait appartenir à un
/// circuit qui ne la référence pas, ou inversement).
/// </summary>
public sealed class Load : AuditableEntity
{
    public EntityId<Load> Id { get; }

    public string Label { get; private set; }

    public LoadCategory Category { get; private set; }

    /// <summary>Puissance active en watts.</summary>
    public double PowerW { get; private set; }

    /// <summary>Facteur de puissance (cos phi), entre 0 (exclu) et 1.</summary>
    public double PowerFactor { get; private set; }

    public string? Notes { get; private set; }

    private Load(
        EntityId<Load> id,
        string label,
        LoadCategory category,
        double powerW,
        double powerFactor,
        string? notes,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc)
        : base(createdAtUtc, updatedAtUtc)
    {
        Id = id;
        Label = label;
        Category = category;
        PowerW = powerW;
        PowerFactor = powerFactor;
        Notes = notes;
    }

    public static Load Create(
        EntityId<Load> id,
        string label,
        LoadCategory category,
        double powerW,
        DateTimeOffset nowUtc,
        double powerFactor = 1.0,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            throw new ArgumentException("Le libellé de la charge est obligatoire.", nameof(label));
        }

        if (powerW <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(powerW), "La puissance doit être positive.");
        }

        if (powerFactor is <= 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(powerFactor), "Le facteur de puissance doit être compris entre 0 (exclu) et 1.");
        }

        return new Load(id, label.Trim(), category, powerW, powerFactor, notes, nowUtc, nowUtc);
    }

    /// <summary>Reconstruction depuis la persistance (préserve les horodatages exacts).</summary>
    public static Load Restore(
        EntityId<Load> id,
        string label,
        LoadCategory category,
        double powerW,
        double powerFactor,
        string? notes,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc) =>
        new(id, label, category, powerW, powerFactor, notes, createdAtUtc, updatedAtUtc);

    public IReadOnlyList<ValidationIssue> Validate()
    {
        var issues = new List<ValidationIssue>();
        var path = $"Load[{Label}]";

        if (string.IsNullOrWhiteSpace(Label))
        {
            issues.Add(new ValidationIssue(path, "Le libellé est obligatoire."));
        }

        if (PowerW <= 0)
        {
            issues.Add(new ValidationIssue($"{path}.PowerW", "La puissance doit être positive."));
        }

        if (PowerFactor is <= 0 or > 1)
        {
            issues.Add(new ValidationIssue($"{path}.PowerFactor", "Le facteur de puissance doit être compris entre 0 (exclu) et 1."));
        }

        return issues;
    }
}
