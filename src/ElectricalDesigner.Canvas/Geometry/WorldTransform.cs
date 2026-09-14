namespace ElectricalDesigner.Canvas.Geometry;

/// <summary>
/// Transformation monde ↔ écran (guide §6, étapes G02 à G04). Immuable et pure :
/// toute opération (zoom, panoramique) retourne une NOUVELLE transformation plutôt
/// que de muter l'état en place, ce qui rend le comportement entièrement
/// déterministe et testable sans dépendre d'un widget d'affichage réel.
///
/// Modèle retenu ("caméra") : <see cref="PanWorld"/> est le point monde actuellement
/// affiché au coin supérieur gauche de l'écran (écran 0,0), et <see cref="Zoom"/> est
/// le facteur d'échelle monde → écran.
///
/// ScreenX = (WorldX − PanX) × Zoom
/// ScreenY = (WorldY − PanY) × Zoom
/// </summary>
public readonly record struct WorldTransform
{
    public const double MinZoom = 0.01;
    public const double MaxZoom = 100.0;

    public double Zoom { get; }

    public Point2D PanWorld { get; }

    public WorldTransform(double zoom, Point2D panWorld)
    {
        if (zoom <= 0 || double.IsNaN(zoom) || double.IsInfinity(zoom))
        {
            throw new ArgumentOutOfRangeException(nameof(zoom), "Le zoom doit être un nombre positif fini.");
        }

        Zoom = Math.Clamp(zoom, MinZoom, MaxZoom);
        PanWorld = panWorld;
    }

    /// <summary>Transformation neutre : zoom 1:1, origine monde alignée sur l'origine écran (étape G01).</summary>
    public static WorldTransform Identity { get; } = new(1.0, Point2D.Zero);

    /// <summary>Étape G02 : transformation monde → écran.</summary>
    public Point2D WorldToScreen(Point2D worldPoint) =>
        new((worldPoint.X - PanWorld.X) * Zoom, (worldPoint.Y - PanWorld.Y) * Zoom);

    /// <summary>Étape G02 : transformation écran → monde (réciproque exacte de <see cref="WorldToScreen"/>).</summary>
    public Point2D ScreenToWorld(Point2D screenPoint) =>
        new((screenPoint.X / Zoom) + PanWorld.X, (screenPoint.Y / Zoom) + PanWorld.Y);

    /// <summary>
    /// Étape G03 : zoom centré sur le curseur. Le point monde actuellement sous
    /// <paramref name="screenPivot"/> reste sous ce même point écran après le zoom —
    /// c'est ce qui distingue un "zoom centré sur le curseur" d'un simple zoom
    /// centré sur l'origine.
    /// </summary>
    public WorldTransform ZoomAt(Point2D screenPivot, double factor)
    {
        if (factor <= 0 || double.IsNaN(factor) || double.IsInfinity(factor))
        {
            throw new ArgumentOutOfRangeException(nameof(factor), "Le facteur de zoom doit être un nombre positif fini.");
        }

        var worldPivotBefore = ScreenToWorld(screenPivot);
        var newZoom = Math.Clamp(Zoom * factor, MinZoom, MaxZoom);

        // Recalcule le panoramique pour que worldPivotBefore retombe exactement
        // sous screenPivot avec le nouveau zoom.
        var newPanX = worldPivotBefore.X - (screenPivot.X / newZoom);
        var newPanY = worldPivotBefore.Y - (screenPivot.Y / newZoom);

        return new WorldTransform(newZoom, new Point2D(newPanX, newPanY));
    }

    /// <summary>Étape G04 : panoramique, exprimé en delta écran (pixels) pour matcher un geste souris/tactile.</summary>
    public WorldTransform PanByScreenDelta(Point2D screenDelta)
    {
        var worldDelta = new Point2D(screenDelta.X / Zoom, screenDelta.Y / Zoom);
        return new WorldTransform(Zoom, PanWorld - worldDelta);
    }

    /// <summary>Panoramique exprimé directement en unités monde (ex. pour centrer sur un objet).</summary>
    public WorldTransform PanTo(Point2D newPanWorld) => new(Zoom, newPanWorld);
}
