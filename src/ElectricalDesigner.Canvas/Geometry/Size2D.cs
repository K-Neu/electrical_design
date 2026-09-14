namespace ElectricalDesigner.Canvas.Geometry;

/// <summary>Taille d'un objet graphique (guide §5.4 : width/height).</summary>
public readonly record struct Size2D(double Width, double Height)
{
    public static Size2D operator *(Size2D size, double scalar) => new(size.Width * scalar, size.Height * scalar);
}
