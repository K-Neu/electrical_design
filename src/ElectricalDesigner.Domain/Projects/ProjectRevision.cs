using ElectricalDesigner.Domain.Common;

namespace ElectricalDesigner.Domain.Projects;

/// <summary>
/// Entrée d'historique de version (guide §8.3) : "L'historique doit stocker :
/// auteur, date, résumé, hash de version, statut et commentaire."
/// Ex. de version publiée : <c>PROJECT-2026-000123 / v03 / 2026-09-08</c>.
///
/// Une révision est un fait historique immuable une fois créée : contrairement
/// aux autres entités, elle n'expose aucune méthode de mutation.
/// <see cref="ContentHash"/> est calculé par la couche Infrastructure au moment
/// de la sauvegarde (le Domain n'a pas accès à la sérialisation) — il est donc
/// optionnel ici et fourni par l'appelant.
/// </summary>
public sealed class ProjectRevision : AuditableEntity
{
    public EntityId<ProjectRevision> Id { get; }

    public int VersionNumber { get; }

    public string Author { get; }

    public string Summary { get; }

    public string? ContentHash { get; }

    public ProjectStatus StatusAtPublication { get; }

    public string? Comment { get; }

    private ProjectRevision(
        EntityId<ProjectRevision> id,
        int versionNumber,
        string author,
        string summary,
        string? contentHash,
        ProjectStatus statusAtPublication,
        string? comment,
        DateTimeOffset publishedAtUtc)
        : base(publishedAtUtc, publishedAtUtc)
    {
        Id = id;
        VersionNumber = versionNumber;
        Author = author;
        Summary = summary;
        ContentHash = contentHash;
        StatusAtPublication = statusAtPublication;
        Comment = comment;
    }

    public static ProjectRevision Create(
        EntityId<ProjectRevision> id,
        int versionNumber,
        string author,
        string summary,
        ProjectStatus statusAtPublication,
        DateTimeOffset publishedAtUtc,
        string? contentHash = null,
        string? comment = null)
    {
        if (versionNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(versionNumber), "Le numéro de version doit être positif.");
        }

        if (string.IsNullOrWhiteSpace(author))
        {
            throw new ArgumentException("L'auteur de la révision est obligatoire.", nameof(author));
        }

        if (string.IsNullOrWhiteSpace(summary))
        {
            throw new ArgumentException("Le résumé de la révision est obligatoire.", nameof(summary));
        }

        return new ProjectRevision(id, versionNumber, author.Trim(), summary.Trim(), contentHash, statusAtPublication, comment, publishedAtUtc);
    }
}
