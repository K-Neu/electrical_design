using ElectricalDesigner.Domain.Circuits;
using ElectricalDesigner.Domain.Common;
using ElectricalDesigner.Domain.Installations;
using ElectricalDesigner.Domain.Protections;
using ElectricalDesigner.Domain.Sources;

namespace ElectricalDesigner.Domain.Boards;

/// <summary>
/// Tableau de répartition/manœuvre avec identifiant, tension, source, protection
/// amont et paramètres de court-circuit si disponibles (cahier §9).
///
/// Un tableau est alimenté soit directement par une <see cref="Source"/> (tableau
/// principal), soit par un autre tableau via un circuit de ce dernier (sous-tableau).
/// Les deux origines sont mutuellement exclusives : chaque méthode <see cref="SetDirectSource"/>
/// / <see cref="SetUpstreamBoard"/> efface l'autre pour garantir cet invariant par
/// construction plutôt que par vérification a posteriori.
/// </summary>
public sealed class DistributionBoard : AuditableEntity
{
    public EntityId<DistributionBoard> Id { get; }

    /// <summary>Repère du tableau (ex. "TGBT"). Doit être unique au sein d'un projet.</summary>
    public string Reference { get; private set; }

    public string Name { get; private set; }

    public double VoltageV { get; private set; }

    public CurrentType PhaseConfiguration { get; private set; }

    public EntityId<Source>? SourceId { get; private set; }

    public EntityId<DistributionBoard>? UpstreamBoardId { get; private set; }

    public EntityId<ProtectionDevice>? UpstreamProtectionId { get; private set; }

    /// <summary>Courant de court-circuit présumé en kA, si connu (cahier §21.7, §10 — installations non domestiques).</summary>
    public double? PresumedShortCircuitCurrentKa { get; private set; }

    private readonly List<Circuit> _circuits = [];
    public IReadOnlyList<Circuit> Circuits => _circuits;

    public string? Notes { get; private set; }

    private DistributionBoard(
        EntityId<DistributionBoard> id,
        string reference,
        string name,
        double voltageV,
        CurrentType phaseConfiguration,
        string? notes,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc)
        : base(createdAtUtc, updatedAtUtc)
    {
        Id = id;
        Reference = reference;
        Name = name;
        VoltageV = voltageV;
        PhaseConfiguration = phaseConfiguration;
        Notes = notes;
    }

    public static DistributionBoard Create(
        EntityId<DistributionBoard> id,
        string reference,
        string name,
        double voltageV,
        CurrentType phaseConfiguration,
        DateTimeOffset nowUtc,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(reference))
        {
            throw new ArgumentException("La référence du tableau est obligatoire.", nameof(reference));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Le nom du tableau est obligatoire.", nameof(name));
        }

        if (voltageV <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(voltageV), "La tension doit être positive.");
        }

        return new DistributionBoard(id, reference.Trim(), name.Trim(), voltageV, phaseConfiguration, notes, nowUtc, nowUtc);
    }

    public void SetDirectSource(EntityId<Source> sourceId, DateTimeOffset nowUtc)
    {
        SourceId = sourceId;
        UpstreamBoardId = null;
        UpstreamProtectionId = null;
        Touch(nowUtc);
    }

    public void SetUpstreamBoard(EntityId<DistributionBoard> upstreamBoardId, EntityId<ProtectionDevice>? upstreamProtectionId, DateTimeOffset nowUtc)
    {
        if (upstreamBoardId == Id)
        {
            throw new ArgumentException("Un tableau ne peut pas être alimenté par lui-même.", nameof(upstreamBoardId));
        }

        UpstreamBoardId = upstreamBoardId;
        UpstreamProtectionId = upstreamProtectionId;
        SourceId = null;
        Touch(nowUtc);
    }

    public void SetPresumedShortCircuitCurrent(double? presumedShortCircuitCurrentKa, DateTimeOffset nowUtc)
    {
        if (presumedShortCircuitCurrentKa is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(presumedShortCircuitCurrentKa), "Le courant de court-circuit ne peut pas être négatif.");
        }

        PresumedShortCircuitCurrentKa = presumedShortCircuitCurrentKa;
        Touch(nowUtc);
    }

    /// <summary>
    /// Crée un nouveau circuit rattaché à ce tableau. La référence doit être
    /// unique au sein du tableau (guide §10 — détection de doublon).
    /// </summary>
    public Circuit AddCircuit(
        EntityId<Circuit> id,
        string reference,
        string name,
        CurrentType phase,
        double voltageV,
        double frequencyHz,
        DateTimeOffset nowUtc,
        string? notes = null)
    {
        if (_circuits.Any(c => string.Equals(c.Reference, reference.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException($"Un circuit avec la référence '{reference}' existe déjà sur le tableau '{Reference}'.", nameof(reference));
        }

        var circuit = Circuit.Create(id, reference, name, phase, voltageV, frequencyHz, Id, nowUtc, notes);
        _circuits.Add(circuit);
        Touch(nowUtc);
        return circuit;
    }

    public void UpdateNotes(string? notes, DateTimeOffset nowUtc)
    {
        Notes = notes;
        Touch(nowUtc);
    }

    /// <summary>
    /// Reconstruction depuis la persistance (préserve les horodatages exacts).
    /// Les circuits doivent avoir été reconstruits au préalable via
    /// <see cref="Circuit.Restore"/> (ils portent déjà <c>sourceBoardId = id</c>).
    /// </summary>
    public static DistributionBoard Restore(
        EntityId<DistributionBoard> id,
        string reference,
        string name,
        double voltageV,
        CurrentType phaseConfiguration,
        EntityId<Sources.Source>? sourceId,
        EntityId<DistributionBoard>? upstreamBoardId,
        EntityId<ProtectionDevice>? upstreamProtectionId,
        double? presumedShortCircuitCurrentKa,
        IEnumerable<Circuit> circuits,
        string? notes,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc)
    {
        var board = new DistributionBoard(id, reference, name, voltageV, phaseConfiguration, notes, createdAtUtc, updatedAtUtc)
        {
            SourceId = sourceId,
            UpstreamBoardId = upstreamBoardId,
            UpstreamProtectionId = upstreamProtectionId,
            PresumedShortCircuitCurrentKa = presumedShortCircuitCurrentKa,
        };
        board._circuits.AddRange(circuits);
        return board;
    }

    public IReadOnlyList<ValidationIssue> Validate()
    {
        var issues = new List<ValidationIssue>();
        var path = $"Board[{Reference}]";

        if (string.IsNullOrWhiteSpace(Reference))
        {
            issues.Add(new ValidationIssue(path, "La référence est obligatoire."));
        }

        if (VoltageV <= 0)
        {
            issues.Add(new ValidationIssue($"{path}.VoltageV", "La tension doit être positive."));
        }

        if (SourceId is null && UpstreamBoardId is null)
        {
            issues.Add(new ValidationIssue(path, "Le tableau n'a ni source directe ni tableau amont : son origine d'alimentation est inconnue."));
        }

        var duplicateReferences = _circuits
            .GroupBy(c => c.Reference, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);

        foreach (var duplicate in duplicateReferences)
        {
            issues.Add(new ValidationIssue(path, $"Référence de circuit dupliquée : '{duplicate}'."));
        }

        foreach (var circuit in _circuits)
        {
            issues.AddRange(circuit.Validate());
        }

        return issues;
    }
}
