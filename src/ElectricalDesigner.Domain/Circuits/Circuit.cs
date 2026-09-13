using ElectricalDesigner.Domain.Boards;
using ElectricalDesigner.Domain.Cabling;
using ElectricalDesigner.Domain.Common;
using ElectricalDesigner.Domain.Installations;
using ElectricalDesigner.Domain.Loads;
using ElectricalDesigner.Domain.Protections;
using ElectricalDesigner.Domain.Symbols;

namespace ElectricalDesigner.Domain.Circuits;

/// <summary>
/// Circuit élémentaire (cahier §9 ; attributs minimaux définis au guide §5.2 :
/// id, reference, name, phase, voltage, frequency, protectionId, cableId,
/// sourceBoardId, destinationBoardId, loads[], positionRepresentations[],
/// calculationData, notes).
///
/// Un circuit appartient toujours à exactement un tableau source
/// (<see cref="SourceBoardId"/>) — c'est ce tableau qui possède la collection
/// de circuits (voir <see cref="DistributionBoard.Circuits"/>). Un circuit
/// peut optionnellement alimenter un second tableau
/// (<see cref="DestinationBoardId"/>), ce qui modélise la chaîne
/// tableau → circuit → sous-tableau décrite au guide §7.3 (topologie).
/// </summary>
public sealed class Circuit : AuditableEntity
{
    public EntityId<Circuit> Id { get; }

    /// <summary>Repère du circuit selon le système RGIE (cahier §10), ex. "B".</summary>
    public string Reference { get; private set; }

    public string Name { get; private set; }

    public CurrentType Phase { get; private set; }

    public double VoltageV { get; private set; }

    public double FrequencyHz { get; private set; }

    public EntityId<ProtectionDevice>? ProtectionId { get; private set; }

    public EntityId<Cable>? CableId { get; private set; }

    public EntityId<DistributionBoard> SourceBoardId { get; }

    public EntityId<DistributionBoard>? DestinationBoardId { get; private set; }

    private readonly List<EntityId<Load>> _loadIds = [];
    public IReadOnlyList<EntityId<Load>> LoadIds => _loadIds;

    private readonly List<EntityId<ElectricalSymbolInstance>> _positionRepresentationIds = [];
    public IReadOnlyList<EntityId<ElectricalSymbolInstance>> PositionRepresentationIds => _positionRepresentationIds;

    /// <summary>Réservé au moteur de calcul (Phase 8). <c>null</c> tant qu'aucun calcul n'a été exécuté.</summary>
    public CircuitCalculationSnapshot? CalculationData { get; private set; }

    public string? Notes { get; private set; }

    private Circuit(
        EntityId<Circuit> id,
        string reference,
        string name,
        CurrentType phase,
        double voltageV,
        double frequencyHz,
        EntityId<DistributionBoard> sourceBoardId,
        string? notes,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc)
        : base(createdAtUtc, updatedAtUtc)
    {
        Id = id;
        Reference = reference;
        Name = name;
        Phase = phase;
        VoltageV = voltageV;
        FrequencyHz = frequencyHz;
        SourceBoardId = sourceBoardId;
        Notes = notes;
    }

    public static Circuit Create(
        EntityId<Circuit> id,
        string reference,
        string name,
        CurrentType phase,
        double voltageV,
        double frequencyHz,
        EntityId<DistributionBoard> sourceBoardId,
        DateTimeOffset nowUtc,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(reference))
        {
            throw new ArgumentException("La référence du circuit est obligatoire.", nameof(reference));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Le nom du circuit est obligatoire.", nameof(name));
        }

        if (voltageV <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(voltageV), "La tension doit être positive.");
        }

        if (sourceBoardId.IsEmpty)
        {
            throw new ArgumentException("Un circuit doit appartenir à un tableau source.", nameof(sourceBoardId));
        }

        if (phase == CurrentType.Dc && frequencyHz != 0)
        {
            throw new ArgumentException("Un circuit DC n'a pas de fréquence non nulle.", nameof(frequencyHz));
        }

        return new Circuit(
            id, reference.Trim(), name.Trim(), phase, voltageV, frequencyHz, sourceBoardId, notes,
            nowUtc, nowUtc);
    }

    public void AssignProtection(EntityId<ProtectionDevice> protectionId, DateTimeOffset nowUtc)
    {
        ProtectionId = protectionId;
        Touch(nowUtc);
    }

    public void AssignCable(EntityId<Cable> cableId, DateTimeOffset nowUtc)
    {
        CableId = cableId;
        Touch(nowUtc);
    }

    public void SetDestinationBoard(EntityId<DistributionBoard>? destinationBoardId, DateTimeOffset nowUtc)
    {
        if (destinationBoardId == SourceBoardId)
        {
            throw new ArgumentException("Un circuit ne peut pas alimenter son propre tableau source.", nameof(destinationBoardId));
        }

        DestinationBoardId = destinationBoardId;
        Touch(nowUtc);
    }

    public void AddLoad(EntityId<Load> loadId, DateTimeOffset nowUtc)
    {
        if (_loadIds.Contains(loadId))
        {
            return;
        }

        _loadIds.Add(loadId);
        Touch(nowUtc);
    }

    public void RemoveLoad(EntityId<Load> loadId, DateTimeOffset nowUtc)
    {
        if (_loadIds.Remove(loadId))
        {
            Touch(nowUtc);
        }
    }

    /// <summary>
    /// Enregistre qu'une instance de symbole (schéma ou plan) représente ce circuit
    /// (préparation de la Phase 5 — synchronisation schéma ↔ plan, guide §9).
    /// </summary>
    public void AddPositionRepresentation(EntityId<ElectricalSymbolInstance> symbolInstanceId, DateTimeOffset nowUtc)
    {
        if (_positionRepresentationIds.Contains(symbolInstanceId))
        {
            return;
        }

        _positionRepresentationIds.Add(symbolInstanceId);
        Touch(nowUtc);
    }

    public void UpdateNotes(string? notes, DateTimeOffset nowUtc)
    {
        Notes = notes;
        Touch(nowUtc);
    }

    /// <summary>
    /// Reconstruction depuis la persistance (préserve les horodatages exacts et
    /// réinjecte directement les références — protection, câble, tableau de
    /// destination, charges, représentations — sans passer par les mutateurs
    /// individuels, qui déclencheraient chacun un <see cref="AuditableEntity.Touch"/>
    /// et corrompraient <c>updatedAtUtc</c>.
    /// </summary>
    public static Circuit Restore(
        EntityId<Circuit> id,
        string reference,
        string name,
        CurrentType phase,
        double voltageV,
        double frequencyHz,
        EntityId<DistributionBoard> sourceBoardId,
        EntityId<ProtectionDevice>? protectionId,
        EntityId<Cable>? cableId,
        EntityId<DistributionBoard>? destinationBoardId,
        IEnumerable<EntityId<Load>> loadIds,
        IEnumerable<EntityId<ElectricalSymbolInstance>> positionRepresentationIds,
        CircuitCalculationSnapshot? calculationData,
        string? notes,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc)
    {
        var circuit = new Circuit(id, reference, name, phase, voltageV, frequencyHz, sourceBoardId, notes, createdAtUtc, updatedAtUtc)
        {
            ProtectionId = protectionId,
            CableId = cableId,
            DestinationBoardId = destinationBoardId,
            CalculationData = calculationData,
        };
        circuit._loadIds.AddRange(loadIds);
        circuit._positionRepresentationIds.AddRange(positionRepresentationIds);
        return circuit;
    }

    public IReadOnlyList<ValidationIssue> Validate()
    {
        var issues = new List<ValidationIssue>();
        var path = $"Circuit[{Reference}]";

        if (string.IsNullOrWhiteSpace(Reference))
        {
            issues.Add(new ValidationIssue(path, "La référence est obligatoire."));
        }

        if (string.IsNullOrWhiteSpace(Name))
        {
            issues.Add(new ValidationIssue($"{path}.Name", "Le nom est obligatoire."));
        }

        if (VoltageV <= 0)
        {
            issues.Add(new ValidationIssue($"{path}.VoltageV", "La tension doit être positive."));
        }

        if (Phase != CurrentType.Dc && FrequencyHz <= 0)
        {
            issues.Add(new ValidationIssue($"{path}.FrequencyHz", "Un circuit AC doit avoir une fréquence positive."));
        }

        if (SourceBoardId.IsEmpty)
        {
            issues.Add(new ValidationIssue($"{path}.SourceBoardId", "Un circuit doit appartenir à un tableau source."));
        }

        return issues;
    }
}
