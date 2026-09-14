namespace ElectricalDesigner.Canvas.Geometry;

/// <summary>
/// Point 2D en coordonnées monde OU écran selon le contexte d'utilisation
/// (guide §6, étape G01 : "Système de coordonnées monde"). Ce type ne porte
/// aucune sémantique de repère par lui-même ; c'est <see cref="WorldTransform"/>
/// qui fait explicitement la conversion entre les deux espaces (étape G02),
/// pour ne jamais mélanger un point écran et un point monde par accident.
/// </summary>
public readonly record struct Point2D(double X, double Y)
{
    public static readonly Point2D Zero = new(0, 0);

    public static Point2D operator +(Point2D a, Point2D b) => new(a.X + b.X, a.Y + b.Y);

    public static Point2D operator -(Point2D a, Point2D b) => new(a.X - b.X, a.Y - b.Y);

    public static Point2D operator *(Point2D p, double scalar) => new(p.X * scalar, p.Y * scalar);

    public static Point2D operator /(Point2D p, double scalar) => new(p.X / scalar, p.Y / scalar);

    public double DistanceTo(Point2D other)
    {
        var dx = X - other.X;
        var dy = Y - other.Y;
        return Math.Sqrt((dx * dx) + (dy * dy));
    }
}
