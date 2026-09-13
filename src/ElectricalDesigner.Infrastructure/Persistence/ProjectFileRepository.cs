using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ElectricalDesigner.Application.Persistence;
using ElectricalDesigner.Domain.Projects;
using ElectricalDesigner.Infrastructure.Persistence.Dtos;

namespace ElectricalDesigner.Infrastructure.Persistence;

/// <summary>
/// Implémentation du format documenté dans <c>docs/03_File_Format.md</c> : un
/// conteneur ZIP versionné contenant au minimum <c>manifest.json</c> et
/// <c>project.json</c>. Les sous-dossiers <c>pages/</c>, <c>assets/</c>,
/// <c>calculations/</c>, <c>history/</c> prévus dans le format cible
/// n'ont pas encore de contenu réel en Phase 1 (pas de canvas, pas de calculs) :
/// ils seront ajoutés au fur et à mesure des phases correspondantes.
/// </summary>
public sealed class ProjectFileRepository : IProjectFileRepository
{
    private const int CurrentFormatVersion = 1;
    private const string ManifestEntryName = "manifest.json";
    private const string ProjectEntryName = "project.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };

    public void Save(Project project, string filePath)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var dto = ProjectDtoMapper.ToDto(project);
        var projectJsonBytes = JsonSerializer.SerializeToUtf8Bytes(dto, JsonOptions);
        var contentHash = ComputeHash(projectJsonBytes);

        var manifest = new ManifestDto
        {
            FormatVersion = CurrentFormatVersion,
            ApplicationVersion = GetApplicationVersion(),
            RegulatoryPackVersion = null, // Aucun RulePack actif avant la Phase 7/11.
            ProjectId = project.Id.Value,
            SavedAtUtc = DateTimeOffset.UtcNow,
            ContentHash = contentHash,
        };
        var manifestBytes = JsonSerializer.SerializeToUtf8Bytes(manifest, JsonOptions);

        // Sauvegarde atomique (guide §11.1) : fichier temporaire -> flush -> vérification -> remplacement.
        var directory = Path.GetDirectoryName(Path.GetFullPath(filePath));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var tempFilePath = filePath + ".tmp";

        try
        {
            using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create))
            {
                WriteEntry(archive, ManifestEntryName, manifestBytes);
                WriteEntry(archive, ProjectEntryName, projectJsonBytes);
            }

            VerifyArchiveReadable(tempFilePath, contentHash);

            File.Move(tempFilePath, filePath, overwrite: true);
        }
        finally
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }

    public Project Load(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Fichier projet introuvable.", filePath);
        }

        using var archive = ZipFile.OpenRead(filePath);

        var manifest = ReadEntry<ManifestDto>(archive, ManifestEntryName)
            ?? throw new InvalidDataException($"'{ManifestEntryName}' est absent ou invalide dans '{filePath}'.");

        if (manifest.FormatVersion != CurrentFormatVersion)
        {
            // Point d'extension Phase 10/11 : c'est ici que viendra la logique de
            // migration v1 -> v2 -> ... (guide §30). En Phase 1, une seule version
            // existe, donc tout écart est traité comme une erreur.
            throw new NotSupportedException(
                $"Format de fichier projet non supporté (version {manifest.FormatVersion}, attendu {CurrentFormatVersion}). "
                + "Aucune migration n'est encore implémentée (Phase 1).");
        }

        var projectEntry = archive.GetEntry(ProjectEntryName)
            ?? throw new InvalidDataException($"'{ProjectEntryName}' est absent de '{filePath}'.");

        var projectJsonBytes = ReadAllBytes(projectEntry);

        if (manifest.ContentHash is not null)
        {
            var actualHash = ComputeHash(projectJsonBytes);
            if (!string.Equals(actualHash, manifest.ContentHash, StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    $"Intégrité compromise : le hash de '{ProjectEntryName}' ne correspond pas à celui du manifeste. "
                    + "Le fichier a peut-être été modifié ou corrompu en dehors de l'application.");
            }
        }

        var dto = JsonSerializer.Deserialize<ProjectDto>(projectJsonBytes, JsonOptions)
            ?? throw new InvalidDataException($"'{ProjectEntryName}' n'a pas pu être désérialisé dans '{filePath}'.");

        return ProjectDtoMapper.ToDomain(dto);
    }

    private static void WriteEntry(ZipArchive archive, string entryName, byte[] content)
    {
        var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
        using var entryStream = entry.Open();
        entryStream.Write(content, 0, content.Length);
    }

    private static void VerifyArchiveReadable(string path, string expectedProjectHash)
    {
        using var archive = ZipFile.OpenRead(path);

        var manifestEntry = archive.GetEntry(ManifestEntryName)
            ?? throw new InvalidDataException("Vérification post-écriture échouée : manifest.json absent.");
        var projectEntry = archive.GetEntry(ProjectEntryName)
            ?? throw new InvalidDataException("Vérification post-écriture échouée : project.json absent.");

        _ = manifestEntry; // Présence déjà suffisante pour ce composant du manifeste.

        var actualHash = ComputeHash(ReadAllBytes(projectEntry));
        if (!string.Equals(actualHash, expectedProjectHash, StringComparison.Ordinal))
        {
            throw new InvalidDataException("Vérification post-écriture échouée : contenu relu différent du contenu écrit.");
        }
    }

    private static TDto? ReadEntry<TDto>(ZipArchive archive, string entryName)
    {
        var entry = archive.GetEntry(entryName);
        if (entry is null)
        {
            return default;
        }

        var bytes = ReadAllBytes(entry);
        return JsonSerializer.Deserialize<TDto>(bytes, JsonOptions);
    }

    private static byte[] ReadAllBytes(ZipArchiveEntry entry)
    {
        using var entryStream = entry.Open();
        using var memoryStream = new MemoryStream();
        entryStream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }

    private static string ComputeHash(byte[] content)
    {
        var hashBytes = SHA256.HashData(content);
        var builder = new StringBuilder(hashBytes.Length * 2);
        foreach (var b in hashBytes)
        {
            builder.Append(b.ToString("x2"));
        }

        return builder.ToString();
    }

    private static string GetApplicationVersion() =>
        typeof(ProjectFileRepository).Assembly.GetName().Version?.ToString() ?? "0.0.0";
}
