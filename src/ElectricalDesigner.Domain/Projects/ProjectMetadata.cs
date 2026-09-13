namespace ElectricalDesigner.Domain.Projects;

/// <summary>
/// Métadonnées administratives du projet (cahier §8.1) : numéro de dossier, nom,
/// adresse, unité/bâtiment, auteur, responsable d'exécution, entreprise, numéro
/// TVA, client/propriétaire, organisme de contrôle, langue, commentaires, statut.
///
/// Value object immuable (record) : toute évolution passe par une nouvelle
/// instance (<c>with</c>), jamais par une mutation en place. C'est
/// <see cref="Project"/> qui porte la mutation (et l'horodatage associé).
/// </summary>
public sealed record ProjectMetadata
{
    public required string Name { get; init; }

    public string? FileNumber { get; init; }

    public string? Address { get; init; }

    public string? BuildingUnit { get; init; }

    public string? Author { get; init; }

    public string? ExecutionManagerName { get; init; }

    public string? CompanyName { get; init; }

    public string? VatNumber { get; init; }

    public string? ClientName { get; init; }

    public string? InspectionBody { get; init; }

    public ProjectLanguage Language { get; init; } = ProjectLanguage.French;

    public string? Comments { get; init; }

    public ProjectStatus Status { get; init; } = ProjectStatus.Draft;

    /// <summary>Valide les invariants minimaux. Les champs administratifs optionnels
    /// manquants (TVA, responsable d'exécution...) ne sont PAS des erreurs ici :
    /// leur caractère obligatoire dépend du RGIE et sera vérifié par le futur
    /// moteur de règles (Phase 7/11), pas par cette validation structurelle.</summary>
    public IEnumerable<string> Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            yield return "Le nom du projet est obligatoire.";
        }
    }
}
