using ElectricalDesigner.Domain.Circuits;
using ElectricalDesigner.Domain.Common;

namespace ElectricalDesigner.Domain.Symbols;

/// <summary>
/// Instance d'un <see cref="ElectricalSymbolDefinition"/> dans un projet (guide §5.3).
///
/// Ne porte volontairement AUCUNE propriété graphique (x, y, rotation, calque...) :
/// ces propriétés (guide §5.4) appartiennent à la représentation dans une page
/// (<see cref="Plans.SchematicPage"/> / <see cref="Plans.PositionPlan"/>), qui
/// n'existera concrètement qu'à partir du moteur graphique (Phase 2) et de
/// l'éditeur de schéma/plan (Phase 3/4). Séparer ainsi "instance métier" et
/// "placement graphique" est une application directe de la règle des trois
/// vérités (guide §1.3) : une modification graphique ne doit jamais pouvoir
/// modifier une caractéristique électrique par accident.
/// </summary>
public sealed class ElectricalSymbolInstance : AuditableEntity
{
    public EntityId<ElectricalSymbolInstance> Id { get; }

    public EntityId<ElectricalSymbolDefinition> SymbolDefinitionId { get; }

    /// <summary>Circuit électrique représenté par ce symbole, si applicable (guide §5.5 : circuitId).</summary>
    public EntityId<Circuit>? CircuitId { get; private set; }

    /// <summary>Repère affiché (ex. "B3"), indépendant de la référence du circuit.</summary>
    public string Label { get; private set; }

    public string? Notes { get; private set; }

    private ElectricalSymbolInstance(
        EntityId<ElectricalSymbolInstance> id,
        EntityId<ElectricalSymbolDefinition> symbolDefinitionId,
        string label,
        string? notes,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc)
        : base(createdAtUtc, updatedAtUtc)
    {
        Id = id;
        SymbolDefinitionId = symbolDefinitionId;
        Label = label;
        Notes = notes;
    }

    public static ElectricalSymbolInstance Create(
        EntityId<ElectricalSymbolInstance> id,
        EntityId<ElectricalSymbolDefinition> symbolDefinitionId,
        string label,
        DateTimeOffset nowUtc,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            throw new ArgumentException("Le repère affiché est obligatoire.", nameof(label));
        }

        return new ElectricalSymbolInstance(id, symbolDefinitionId, label.Trim(), notes, nowUtc, nowUtc);
    }

    public void LinkToCircuit(EntityId<Circuit> circuitId, DateTimeOffset nowUtc)
    {
        CircuitId = circuitId;
        Touch(nowUtc);
    }

    public IReadOnlyList<ValidationIssue> Validate()
    {
        var issues = new List<ValidationIssue>();

        if (string.IsNullOrWhiteSpace(Label))
        {
            issues.Add(new ValidationIssue($"SymbolInstance[{Id}]", "Le repère affiché est obligatoire."));
        }

        return issues;
    }
}
