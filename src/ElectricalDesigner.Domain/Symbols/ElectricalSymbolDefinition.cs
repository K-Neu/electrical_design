using ElectricalDesigner.Domain.Common;

namespace ElectricalDesigner.Domain.Symbols;

/// <summary>
/// Décrit un symbole (guide §5.3 : "La définition décrit le symbole. L'instance
/// décrit son utilisation dans un projet."). Le rendu graphique réel (icône,
/// points de connexion) sera construit avec la bibliothèque en Phase 3 — cette
/// classe pose dès la Phase 1 la distinction structurelle définition/instance,
/// sans laquelle un symbole personnalisé (cahier §11.4) ne pourrait pas exister
/// proprement plus tard.
/// </summary>
public sealed class ElectricalSymbolDefinition : AuditableEntity
{
    public EntityId<ElectricalSymbolDefinition> Id { get; }

    public string Name { get; private set; }

    public SymbolCategory Category { get; private set; }

    public string? Description { get; private set; }

    private ElectricalSymbolDefinition(
        EntityId<ElectricalSymbolDefinition> id,
        string name,
        SymbolCategory category,
        string? description,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc)
        : base(createdAtUtc, updatedAtUtc)
    {
        Id = id;
        Name = name;
        Category = category;
        Description = description;
    }

    public static ElectricalSymbolDefinition Create(
        EntityId<ElectricalSymbolDefinition> id,
        string name,
        SymbolCategory category,
        DateTimeOffset nowUtc,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Le nom du symbole est obligatoire.", nameof(name));
        }

        return new ElectricalSymbolDefinition(id, name.Trim(), category, description, nowUtc, nowUtc);
    }

    /// <summary>Reconstruction depuis la persistance (préserve les horodatages exacts).</summary>
    public static ElectricalSymbolDefinition Restore(
        EntityId<ElectricalSymbolDefinition> id,
        string name,
        SymbolCategory category,
        string? description,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc) =>
        new(id, name, category, description, createdAtUtc, updatedAtUtc);

    public IReadOnlyList<ValidationIssue> Validate()
    {
        var issues = new List<ValidationIssue>();

        if (string.IsNullOrWhiteSpace(Name))
        {
            issues.Add(new ValidationIssue($"SymbolDefinition[{Id}]", "Le nom est obligatoire."));
        }

        return issues;
    }
}
