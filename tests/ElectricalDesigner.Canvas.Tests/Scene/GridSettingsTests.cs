using ElectricalDesigner.Canvas.Geometry;
using ElectricalDesigner.Canvas.Scene;
using ElectricalDesigner.TestKit;

namespace ElectricalDesigner.Canvas.Tests.Scene;

public class GridSettingsTests
{
    [Fact]
    public void Snap_AccrocheAuPasDeGrilleLePlusProche()
    {
        var grid = new GridSettings { SpacingWorld = 10, SnapEnabled = true, Enabled = true };

        var snapped = grid.Snap(new Point2D(23, 47));

        Assert.Equal(20.0, snapped.X);
        Assert.Equal(50.0, snapped.Y);
    }

    [Fact]
    public void Snap_AvecAccrochageDesactive_NeModifiePasLePoint()
    {
        var grid = new GridSettings { SpacingWorld = 10, SnapEnabled = false, Enabled = true };
        var original = new Point2D(23, 47);

        var snapped = grid.Snap(original);

        Assert.Equal(original.X, snapped.X);
        Assert.Equal(original.Y, snapped.Y);
    }
}
