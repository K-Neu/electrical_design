using ElectricalDesigner.Canvas.Geometry;

namespace ElectricalDesigner.Canvas.Scene;

/// <summary>Grille et accrochage (guide §6, étape G11).</summary>
public sealed record GridSettings
{
    public bool Enabled { get; init; } = true;

    /// <summary>Espacement de la grille, en unités monde.</summary>
    public double SpacingWorld { get; init; } = 10.0;

    public bool SnapEnabled { get; init; } = true;

    public static GridSettings Default { get; } = new();

    /// <summary>Accroche un point monde à la grille la plus proche, si l'accrochage est actif.</summary>
    public Point2D Snap(Point2D worldPoint)
    {
        if (!SnapEnabled || !Enabled || SpacingWorld <= 0)
        {
            return worldPoint;
        }

        var snappedX = Math.Round(worldPoint.X / SpacingWorld) * SpacingWorld;
        var snappedY = Math.Round(worldPoint.Y / SpacingWorld) * SpacingWorld;
        return new Point2D(snappedX, snappedY);
    }
}
