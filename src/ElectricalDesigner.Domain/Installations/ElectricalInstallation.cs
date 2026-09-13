using ElectricalDesigner.Domain.Common;

namespace ElectricalDesigner.Domain.Installations;

/// <summary>
/// Décrit le type d'installation, l'alimentation, le schéma de mise à la terre,
/// les tensions et les paramètres généraux (cahier §9, entité ElectricalInstallation).
/// </summary>
public sealed class ElectricalInstallation : AuditableEntity
{
    public EntityId<ElectricalInstallation> Id { get; }

    public InstallationCategory Category { get; private set; }

    public CurrentType CurrentType { get; private set; }

    public EarthingSystem EarthingSystem { get; private set; }

    /// <summary>Tension nominale en volts (ex. 230 pour un monophasé domestique belge, 400 pour un triphasé).</summary>
    public double NominalVoltageV { get; private set; }

    /// <summary>Fréquence nominale en hertz (50 Hz en Belgique ; 0 pour une installation purement DC).</summary>
    public double FrequencyHz { get; private set; }

    public string? Notes { get; private set; }

    private ElectricalInstallation(
        EntityId<ElectricalInstallation> id,
        InstallationCategory category,
        CurrentType currentType,
        EarthingSystem earthingSystem,
        double nominalVoltageV,
        double frequencyHz,
        string? notes,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc)
        : base(createdAtUtc, updatedAtUtc)
    {
        Id = id;
        Category = category;
        CurrentType = currentType;
        EarthingSystem = earthingSystem;
        NominalVoltageV = nominalVoltageV;
        FrequencyHz = frequencyHz;
        Notes = notes;
    }

    public static ElectricalInstallation Create(
        EntityId<ElectricalInstallation> id,
        InstallationCategory category,
        CurrentType currentType,
        EarthingSystem earthingSystem,
        double nominalVoltageV,
        double frequencyHz,
        DateTimeOffset nowUtc,
        string? notes = null)
    {
        if (nominalVoltageV <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(nominalVoltageV), "La tension nominale doit être positive.");
        }

        if (frequencyHz < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(frequencyHz), "La fréquence ne peut pas être négative.");
        }

        if (currentType == CurrentType.Dc && frequencyHz != 0)
        {
            throw new ArgumentException("Une installation DC n'a pas de fréquence non nulle.", nameof(frequencyHz));
        }

        return new ElectricalInstallation(
            id, category, currentType, earthingSystem, nominalVoltageV, frequencyHz, notes, nowUtc, nowUtc);
    }

    /// <summary>
    /// Reconstruit l'entité à partir de données déjà persistées (ex. relecture
    /// d'un fichier .elecproj), en préservant exactement <paramref name="createdAtUtc"/>
    /// et <paramref name="updatedAtUtc"/> — contrairement à <see cref="Create"/>, qui
    /// représente la création d'une toute nouvelle installation et n'a donc qu'un
    /// seul instant "maintenant" pour les deux horodatages. Utilisé uniquement par
    /// la couche Infrastructure (mapping de persistance).
    /// </summary>
    public static ElectricalInstallation Restore(
        EntityId<ElectricalInstallation> id,
        InstallationCategory category,
        CurrentType currentType,
        EarthingSystem earthingSystem,
        double nominalVoltageV,
        double frequencyHz,
        string? notes,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc) =>
        new(id, category, currentType, earthingSystem, nominalVoltageV, frequencyHz, notes, createdAtUtc, updatedAtUtc);

    public void UpdateVoltageAndFrequency(double nominalVoltageV, double frequencyHz, DateTimeOffset nowUtc)
    {
        if (nominalVoltageV <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(nominalVoltageV), "La tension nominale doit être positive.");
        }

        NominalVoltageV = nominalVoltageV;
        FrequencyHz = frequencyHz;
        Touch(nowUtc);
    }

    public void UpdateNotes(string? notes, DateTimeOffset nowUtc)
    {
        Notes = notes;
        Touch(nowUtc);
    }

    /// <summary>Validation structurelle (guide Phase 1, livrable "validation du modèle") — pas de règle RGIE ici.</summary>
    public IReadOnlyList<ValidationIssue> Validate()
    {
        var issues = new List<ValidationIssue>();

        if (NominalVoltageV <= 0)
        {
            issues.Add(new ValidationIssue("Installation.NominalVoltageV", "La tension nominale doit être positive."));
        }

        if (CurrentType != CurrentType.Dc && FrequencyHz <= 0)
        {
            issues.Add(new ValidationIssue("Installation.FrequencyHz", "Une installation AC doit avoir une fréquence positive."));
        }

        return issues;
    }
}
