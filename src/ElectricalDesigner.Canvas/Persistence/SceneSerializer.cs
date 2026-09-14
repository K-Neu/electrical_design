using System.Text.Json;
using ElectricalDesigner.Canvas.Geometry;
using ElectricalDesigner.Canvas.Scene;

namespace ElectricalDesigner.Canvas.Persistence;

/// <summary>
/// Sérialisation JSON d'une <see cref="CanvasScene"/> — démontre le critère de
/// sortie de la Phase 2 (guide §Phase 2) : "sauvegarder leurs coordonnées sans
/// perte". Volontairement indépendante du format <c>.elecproj</c> (voir
/// docs/03_File_Format.md, section "Persistance du canvas") : en Phase 2, une
/// scène est un outil générique, pas encore rattachée à une page de projet.
/// </summary>
public static class SceneSerializer
{
    private const int CurrentFormatVersion = 1;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };

    public static string ToJson(CanvasScene scene)
    {
        ArgumentNullException.ThrowIfNull(scene);

        var dto = new SceneDto
        {
            FormatVersion = CurrentFormatVersion,
            Grid = new GridSettingsDto
            {
                Enabled = scene.Grid.Enabled,
                SpacingWorld = scene.Grid.SpacingWorld,
                SnapEnabled = scene.Grid.SnapEnabled,
            },
            Objects = scene.Objects.Select(o => new CanvasObjectDto
            {
                Id = o.Id.Value,
                X = o.Position.X,
                Y = o.Position.Y,
                Width = o.Size.Width,
                Height = o.Size.Height,
                RotationDegrees = o.RotationDegrees,
                Scale = o.Scale,
                Visible = o.Visible,
                Locked = o.Locked,
                Layer = o.Layer,
                BusinessObjectId = o.BusinessObjectId,
            }).ToList(),
        };

        return JsonSerializer.Serialize(dto, JsonOptions);
    }

    public static CanvasScene FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        var dto = JsonSerializer.Deserialize<SceneDto>(json, JsonOptions)
            ?? throw new InvalidDataException("Le JSON de scène n'a pas pu être désérialisé.");

        if (dto.FormatVersion != CurrentFormatVersion)
        {
            throw new NotSupportedException(
                $"Format de scène non supporté (version {dto.FormatVersion}, attendu {CurrentFormatVersion}).");
        }

        var scene = new CanvasScene
        {
            Grid = new GridSettings
            {
                Enabled = dto.Grid.Enabled,
                SpacingWorld = dto.Grid.SpacingWorld,
                SnapEnabled = dto.Grid.SnapEnabled,
            },
        };

        foreach (var objectDto in dto.Objects)
        {
            scene.Add(CanvasObject.Restore(
                new CanvasObjectId(objectDto.Id),
                new Point2D(objectDto.X, objectDto.Y),
                new Size2D(objectDto.Width, objectDto.Height),
                objectDto.RotationDegrees,
                objectDto.Scale,
                objectDto.Visible,
                objectDto.Locked,
                objectDto.Layer,
                objectDto.BusinessObjectId));
        }

        return scene;
    }

    /// <summary>Sauvegarde atomique, même principe que <c>ProjectFileRepository.Save</c> (guide §11.1) : fichier temporaire puis remplacement.</summary>
    public static void SaveToFile(CanvasScene scene, string filePath)
    {
        var json = ToJson(scene);
        var directory = Path.GetDirectoryName(Path.GetFullPath(filePath));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var tempFilePath = filePath + ".tmp";
        File.WriteAllText(tempFilePath, json);
        File.Move(tempFilePath, filePath, overwrite: true);
    }

    public static CanvasScene LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Fichier de scène introuvable.", filePath);
        }

        return FromJson(File.ReadAllText(filePath));
    }
}
