using ElectricalDesigner.Domain.Common;
using ElectricalDesigner.Domain.Symbols;

namespace ElectricalDesigner.Domain.Plans;

/// <summary>
/// Page physique comportant fond, murs, portes, fenêtres, objets électriques et
/// annotations (cahier §9).
///
/// Comme <see cref="SchematicPage"/>, cette classe ne porte en Phase 1 que la
/// structure minimale. La géométrie (murs, portes, fond importé, échelle) sera
/// construite par l'éditeur de plan en Phase 4 (guide §8).
/// </summary>
public sealed class PositionPlan : AuditableEntity
{
    public EntityId<PositionPlan> Id { get; }

    public string Title { get; private set; }

    public int PageOrder { get; private set; }

    private readonly List<EntityId<ElectricalSymbolInstance>> _symbolInstanceIds = [];
    public IReadOnlyList<EntityId<ElectricalSymbolInstance>> SymbolInstanceIds => _symbolInstanceIds;

    private PositionPlan(
        EntityId<PositionPlan> id,
        string title,
        int pageOrder,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc)
        : base(createdAtUtc, updatedAtUtc)
    {
        Id = id;
        Title = title;
        PageOrder = pageOrder;
    }

    public static PositionPlan Create(EntityId<PositionPlan> id, string title, int pageOrder, DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Le titre de la page est obligatoire.", nameof(title));
        }

        if (pageOrder < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageOrder), "L'ordre de page ne peut pas être négatif.");
        }

        return new PositionPlan(id, title.Trim(), pageOrder, nowUtc, nowUtc);
    }

    public void AddSymbolInstance(EntityId<ElectricalSymbolInstance> symbolInstanceId, DateTimeOffset nowUtc)
    {
        if (_symbolInstanceIds.Contains(symbolInstanceId))
        {
            return;
        }

        _symbolInstanceIds.Add(symbolInstanceId);
        Touch(nowUtc);
    }

    /// <summary>Reconstruction depuis la persistance (préserve les horodatages exacts et le contenu de la page).</summary>
    public static PositionPlan Restore(
        EntityId<PositionPlan> id,
        string title,
        int pageOrder,
        IEnumerable<EntityId<ElectricalSymbolInstance>> symbolInstanceIds,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc)
    {
        var plan = new PositionPlan(id, title, pageOrder, createdAtUtc, updatedAtUtc);
        plan._symbolInstanceIds.AddRange(symbolInstanceIds);
        return plan;
    }

    public IReadOnlyList<ValidationIssue> Validate()
    {
        var issues = new List<ValidationIssue>();

        if (string.IsNullOrWhiteSpace(Title))
        {
            issues.Add(new ValidationIssue($"PositionPlan[{Id}]", "Le titre est obligatoire."));
        }

        return issues;
    }
}
