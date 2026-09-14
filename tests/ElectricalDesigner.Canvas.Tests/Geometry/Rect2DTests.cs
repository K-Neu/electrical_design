using ElectricalDesigner.Canvas.Geometry;
using ElectricalDesigner.TestKit;

namespace ElectricalDesigner.Canvas.Tests.Geometry;

public class Rect2DTests
{
    [Fact]
    public void Contains_PointADansLeRectangle_RetourneTrue()
    {
        var rect = new Rect2D(0, 0, 100, 50);

        Assert.True(rect.Contains(new Point2D(50, 25)));
    }

    [Fact]
    public void Contains_PointEnDehors_RetourneFalse()
    {
        var rect = new Rect2D(0, 0, 100, 50);

        Assert.False(rect.Contains(new Point2D(150, 25)));
    }

    [Fact]
    public void Intersects_RectanglesChevauchants_RetourneTrue()
    {
        var a = new Rect2D(0, 0, 100, 100);
        var b = new Rect2D(50, 50, 100, 100);

        Assert.True(a.Intersects(b));
    }

    [Fact]
    public void Intersects_RectanglesDisjoints_RetourneFalse()
    {
        var a = new Rect2D(0, 0, 10, 10);
        var b = new Rect2D(100, 100, 10, 10);

        Assert.False(a.Intersects(b));
    }

    [Fact]
    public void FromPoints_ConstruitUnRectangleNormalise_QuelQueSoitLOrdreDesPoints()
    {
        var rect = Rect2D.FromPoints(new Point2D(80, 60), new Point2D(20, 10));

        Assert.Equal(20.0, rect.X);
        Assert.Equal(10.0, rect.Y);
        Assert.Equal(60.0, rect.Width);
        Assert.Equal(50.0, rect.Height);
    }
}
