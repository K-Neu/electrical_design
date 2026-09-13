using ElectricalDesigner.Domain.Common;
using ElectricalDesigner.Domain.Symbols;

namespace ElectricalDesigner.Domain.Plans;

/// <summary>
/// Page logique du schéma unifilaire/multifilaire (cahier §9).
///
/// En Phase 1, cette classe ne contient que la structure minimale (identité,
/// titre, ordre de page, liste des symboles qui y figurent). Le contenu
/// graphique réel — placement, connexions, pagination automatique — sera
/// construit par l'éditeur de schéma en Phase 3 (guide §7).
/// </summary>
public sealed class SchematicPage : AuditableEntity
{
    public EntityId<SchematicPage> Id { get; }

    public string Title { get; private set; }

    public int PageOrder { get; private set; }

    private readonly List<EntityId<ElectricalSymbolInstance>> _symbolInstanceIds = [];
    public IReadOnlyList<EntityId<ElectricalSymbolInstance>> SymbolInstanceIds => _symbolInstanceIds;

    private SchematicPage(
        EntityId<SchematicPage> id,
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

    public static SchematicPage Create(EntityId<SchematicPage> id, string title, int pageOrder, DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Le titre de la page est obligatoire.", nameof(title));
        }

        if (pageOrder < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageOrder), "L'ordre de page ne peut pas être négatif.");
        }

        return new SchematicPage(id, title.Trim(), pageOrder, nowUtc, nowUtc);
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
    public static SchematicPage Restore(
        EntityId<SchematicPage> id,
        string title,
        int pageOrder,
        IEnumerable<EntityId<ElectricalSymbolInstance>> symbolInstanceIds,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc)
    {
        var page = new SchematicPage(id, title, pageOrder, createdAtUtc, updatedAtUtc);
        page._symbolInstanceIds.AddRange(symbolInstanceIds);
        return page;
    }

    public IReadOnlyList<ValidationIssue> Validate()
    {
        var issues = new List<ValidationIssue>();

        if (string.IsNullOrWhiteSpace(Title))
        {
            issues.Add(new ValidationIssue($"SchematicPage[{Id}]", "Le titre est obligatoire."));
        }

        return issues;
    }
}
