using ElectricalDesigner.Domain.Projects;

namespace ElectricalDesigner.Application.Persistence;

/// <summary>
/// Port de persistance du projet (inversion de dépendance, guide §3.2/§3.4) :
/// Application définit CE dont elle a besoin, Infrastructure fournit le COMMENT
/// (aujourd'hui : fichier <c>.elecproj</c> ZIP+JSON, voir
/// <c>docs/03_File_Format.md</c>). Ni Domain ni Application ne savent comment
/// le projet est physiquement stocké.
/// </summary>
public interface IProjectFileRepository
{
    /// <summary>Sauvegarde atomique (guide §11.1) : fichier temporaire → flush → vérification → remplacement.</summary>
    void Save(Project project, string filePath);

    /// <summary>Charge un projet depuis un fichier <c>.elecproj</c>, en vérifiant l'intégrité du contenu.</summary>
    Project Load(string filePath);
}
