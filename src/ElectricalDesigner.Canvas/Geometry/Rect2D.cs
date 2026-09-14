namespace ElectricalDesigner.Canvas.Geometry;

/// <summary>
/// Rectangle axis-aligned en coordonnées monde. Sert de boîte englobante pour
/// le hit-testing simple (étape G15, sous-ensemble Phase 2 — voir
/// docs/04_Canvas.md pour les limites assumées) et pour la sélection
/// rectangulaire (rubber-band) en multi-sélection (étape G10).
/// </summary>
public readonly record struct Rect2D(double X, double Y, double Width, double Height)
{
    public double Left => X;

    public double Top => Y;

    public double Right => X + Width;

    public double Bottom => Y + Height;

    public Point2D TopLeft => new(X, Y);

    public Point2D Center => new(X + (Width / 2), Y + (Height / 2));

    public static Rect2D FromPoints(Point2D a, Point2D b)
    {
        var x = Math.Min(a.X, b.X);
        var y = Math.Min(a.Y, b.Y);
        var width = Math.Abs(a.X - b.X);
        var height = Math.Abs(a.Y - b.Y);
        return new Rect2D(x, y, width, height);
    }

    public bool Contains(Point2D point) =>
        point.X >= Left && point.X <= Right && point.Y >= Top && point.Y <= Bottom;

    public bool Intersects(Rect2D other) =>
        Left <= other.Right && Right >= other.Left && Top <= other.Bottom && Bottom >= other.Top;
}
