namespace ElectricalDesigner.Canvas.Persistence;

// ---------------------------------------------------------------------------
// DTOs de sérialisation d'une scène, distincts de CanvasObject/CanvasScene
// pour les mêmes raisons que ElectricalDesigner.Infrastructure.Persistence.Dtos
// (voir docs/03_File_Format.md) : le modèle en mémoire ne doit pas être couplé
// à la forme exacte du JSON persistant.
// ---------------------------------------------------------------------------

public sealed record CanvasObjectDto
{
    public required Guid Id { get; init; }
    public required double X { get; init; }
    public required double Y { get; init; }
    public required double Width { get; init; }
    public required double Height { get; init; }
    public required double RotationDegrees { get; init; }
    public required double Scale { get; init; }
    public required bool Visible { get; init; }
    public required bool Locked { get; init; }
    public required string Layer { get; init; }
    public Guid? BusinessObjectId { get; init; }
}

public sealed record GridSettingsDto
{
    public required bool Enabled { get; init; }
    public required double SpacingWorld { get; init; }
    public required bool SnapEnabled { get; init; }
}

public sealed record SceneDto
{
    public required int FormatVersion { get; init; }
    public required GridSettingsDto Grid { get; init; }
    public required IReadOnlyList<CanvasObjectDto> Objects { get; init; }
}
