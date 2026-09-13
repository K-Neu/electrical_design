namespace ElectricalDesigner.Domain.Common;

/// <summary>
/// Base commune à toutes les entités métier : porte les horodatages
/// <c>createdAt</c> / <c>updatedAt</c> exigés par le guide de développement §5.1.
///
/// Choix de conception : le "temps" n'est jamais lu directement depuis l'horloge
/// système à l'intérieur du Domain (pas de <c>DateTimeOffset.UtcNow</c> caché).
/// Chaque mutation reçoit explicitement <c>nowUtc</c> en paramètre. Cela rend le
/// Domain entièrement déterministe et testable : un test peut simuler n'importe
/// quel instant sans dépendre de l'horloge de la machine qui exécute les tests.
/// </summary>
public abstract class AuditableEntity
{
    public DateTimeOffset CreatedAtUtc { get; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    protected AuditableEntity(DateTimeOffset createdAtUtc, DateTimeOffset updatedAtUtc)
    {
        if (updatedAtUtc < createdAtUtc)
        {
            throw new ArgumentOutOfRangeException(
                nameof(updatedAtUtc),
                "La date de dernière modification ne peut pas précéder la date de création.");
        }

        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
    }

    /// <summary>Met à jour l'horodatage de dernière modification. Appelé par toute méthode de mutation d'une sous-classe.</summary>
    protected void Touch(DateTimeOffset nowUtc)
    {
        if (nowUtc < UpdatedAtUtc)
        {
            throw new ArgumentOutOfRangeException(
                nameof(nowUtc),
                "Impossible de faire remonter l'horodatage de modification dans le temps.");
        }

        UpdatedAtUtc = nowUtc;
    }
}
