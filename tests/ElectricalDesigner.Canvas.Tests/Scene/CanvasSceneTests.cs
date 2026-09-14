using ElectricalDesigner.Canvas.Geometry;
using ElectricalDesigner.Canvas.Scene;
using ElectricalDesigner.TestKit;

namespace ElectricalDesigner.Canvas.Tests.Scene;

public class CanvasSceneTests
{
    [Fact]
    public void Add_PuisGet_RetrouveLeMemeObjet()
    {
        var scene = new CanvasScene();
        var obj = CanvasObject.Create(CanvasObjectId.New(), Point2D.Zero, new Size2D(10, 10));

        scene.Add(obj);

        Assert.Equal(obj.Id, scene.Get(obj.Id).Id);
    }

    [Fact]
    public void Add_AvecIdentifiantDejaPresent_Echoue()
    {
        var scene = new CanvasScene();
        var id = CanvasObjectId.New();
        scene.Add(CanvasObject.Create(id, Point2D.Zero, new Size2D(10, 10)));

        Assert.Throws<ArgumentException>(() =>
            scene.Add(CanvasObject.Create(id, new Point2D(50, 50), new Size2D(5, 5))));
    }

    [Fact]
    public void Remove_ObjetPresent_RetourneTrueEtLeRetireDeLaScene()
    {
        var scene = new CanvasScene();
        var obj = CanvasObject.Create(CanvasObjectId.New(), Point2D.Zero, new Size2D(10, 10));
        scene.Add(obj);

        var removed = scene.Remove(obj.Id);

        Assert.True(removed);
        Assert.False(scene.Contains(obj.Id));
    }

    [Fact]
    public void HitTest_PointDansUnObjet_LeRetourne()
    {
        var scene = new CanvasScene();
        var obj = CanvasObject.Create(CanvasObjectId.New(), new Point2D(0, 0), new Size2D(100, 100));
        scene.Add(obj);

        var hits = scene.HitTest(new Point2D(50, 50)).ToList();

        Assert.Equal(1, hits.Count);
        Assert.Equal(obj.Id, hits[0].Id);
    }

    [Fact]
    public void HitTest_ObjetInvisible_NEstJamaisTouche()
    {
        var scene = new CanvasScene();
        var obj = CanvasObject.Create(CanvasObjectId.New(), Point2D.Zero, new Size2D(100, 100));
        obj.SetVisible(false);
        scene.Add(obj);

        var hits = scene.HitTest(new Point2D(50, 50)).ToList();

        Assert.Equal(0, hits.Count);
    }

    [Fact]
    public void QueryRect_ObjetsChevauchantLaZone_SontRetournes()
    {
        var scene = new CanvasScene();
        var inside = CanvasObject.Create(CanvasObjectId.New(), new Point2D(10, 10), new Size2D(5, 5));
        var outside = CanvasObject.Create(CanvasObjectId.New(), new Point2D(1000, 1000), new Size2D(5, 5));
        scene.Add(inside);
        scene.Add(outside);

        var results = scene.QueryRect(new Rect2D(0, 0, 50, 50)).Select(o => o.Id).ToList();

        Assert.True(results.Contains(inside.Id));
        Assert.False(results.Contains(outside.Id));
    }

    [Fact]
    public void Placer100Objets_LesTousRetrouver()
    {
        // Contribue au critère de sortie de la Phase 2 : "placer 100 objets sur une scène".
        var scene = new CanvasScene();
        var ids = new List<CanvasObjectId>();

        for (var i = 0; i < 100; i++)
        {
            var obj = CanvasObject.Create(CanvasObjectId.New(), new Point2D(i, i), new Size2D(1, 1));
            scene.Add(obj);
            ids.Add(obj.Id);
        }

        Assert.Equal(100, scene.Count);
        Assert.True(ids.All(scene.Contains));
    }
}
