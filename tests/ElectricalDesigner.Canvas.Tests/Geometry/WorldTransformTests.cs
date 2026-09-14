using ElectricalDesigner.Canvas.Geometry;
using ElectricalDesigner.TestKit;

namespace ElectricalDesigner.Canvas.Tests.Geometry;

public class WorldTransformTests
{
    [Fact]
    public void Identity_TransformeUnPointSansChangement()
    {
        var transform = WorldTransform.Identity;
        var world = new Point2D(42, 17);

        var screen = transform.WorldToScreen(world);

        Assert.Equal(42.0, screen.X);
        Assert.Equal(17.0, screen.Y);
    }

    [Fact]
    public void WorldToScreen_PuisScreenToWorld_EstUneAllerRetourExact()
    {
        var transform = new WorldTransform(2.5, new Point2D(10, -5));
        var original = new Point2D(123.456, -78.9);

        var roundTrip = transform.ScreenToWorld(transform.WorldToScreen(original));

        Assert.True(Math.Abs(roundTrip.X - original.X) < 1e-9);
        Assert.True(Math.Abs(roundTrip.Y - original.Y) < 1e-9);
    }

    [Fact]
    public void ZoomAt_GardeLePointMondeSousLeCurseurAuMemePointEcran()
    {
        var transform = new WorldTransform(1.0, new Point2D(0, 0));
        var cursorScreen = new Point2D(300, 200);
        var worldUnderCursorBefore = transform.ScreenToWorld(cursorScreen);

        var zoomed = transform.ZoomAt(cursorScreen, 2.0);
        var screenAfter = zoomed.WorldToScreen(worldUnderCursorBefore);

        // C'est la définition même d'un "zoom centré sur le curseur" (guide, étape G03).
        Assert.True(Math.Abs(screenAfter.X - cursorScreen.X) < 1e-9);
        Assert.True(Math.Abs(screenAfter.Y - cursorScreen.Y) < 1e-9);
        Assert.Equal(2.0, zoomed.Zoom);
    }

    [Fact]
    public void ZoomAt_EstBorneParMinEtMaxZoom()
    {
        var transform = WorldTransform.Identity;

        var zoomedOutTooFar = transform.ZoomAt(Point2D.Zero, 0.0001);
        var zoomedInTooFar = transform.ZoomAt(Point2D.Zero, 100000.0);

        Assert.Equal(WorldTransform.MinZoom, zoomedOutTooFar.Zoom);
        Assert.Equal(WorldTransform.MaxZoom, zoomedInTooFar.Zoom);
    }

    [Fact]
    public void PanByScreenDelta_DeplaceLaVueDansLeSensOppose()
    {
        // Faire glisser le contenu de 100px vers la droite à l'écran équivaut à
        // déplacer la "caméra" vers la gauche en coordonnées monde.
        var transform = WorldTransform.Identity;

        var panned = transform.PanByScreenDelta(new Point2D(100, 0));

        Assert.Equal(-100.0, panned.PanWorld.X);
    }

    [Fact]
    public void PanByScreenDelta_TientCompteDuZoomActuel()
    {
        var transform = new WorldTransform(2.0, Point2D.Zero);

        var panned = transform.PanByScreenDelta(new Point2D(100, 0));

        // À zoom x2, un glissement de 100px écran ne correspond qu'à 50 unités monde.
        Assert.Equal(-50.0, panned.PanWorld.X);
    }

    [Fact]
    public void Constructeur_AvecZoomNegatifOuNul_Echoue()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new WorldTransform(0, Point2D.Zero));
        Assert.Throws<ArgumentOutOfRangeException>(() => new WorldTransform(-1, Point2D.Zero));
    }
}
