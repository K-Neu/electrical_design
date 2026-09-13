namespace ElectricalDesigner.Domain.Projects;

/// <summary>Cahier §8.1 : "statut : brouillon, à vérifier, prêt au contrôle, contrôlé, archivé."</summary>
public enum ProjectStatus
{
    Draft,
    ToReview,
    ReadyForInspection,
    Inspected,
    Archived,
}
