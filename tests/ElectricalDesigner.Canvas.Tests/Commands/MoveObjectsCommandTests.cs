using ElectricalDesigner.Canvas.Commands;
using ElectricalDesigner.Canvas.Geometry;
using ElectricalDesigner.Canvas.Scene;
using ElectricalDesigner.TestKit;

namespace ElectricalDesigner.Canvas.Tests.Commands;

public class MoveObjectsCommandTests
{
    [Fact]
    public void Execute_DeplaceTousLesObjetsDeLaSelectionDUnMemeDelta()
    {
        var scene = new CanvasScene();
        var a = CanvasObject.Create(CanvasObjectId.New(), new Point2D(0, 0), new Size2D(10, 10));
        var b = CanvasObject.Create(CanvasObjectId.New(), new Point2D(100, 100), new Size2D(10, 10));
        scene.Add(a);
        scene.Add(b);
        var command = new MoveObjectsCommand([a.Id, b.Id], new Point2D(5, -5));

        command.Execute(scene);

        Assert.Equal(5.0, scene.Get(a.Id).Position.X);
        Assert.Equal(105.0, scene.Get(b.Id).Position.X);
    }

    [Fact]
    public void Execute_IgnoreLesObjetsVerrouilles()
    {
        var scene = new CanvasScene();
        var locked = CanvasObject.Create(CanvasObjectId.New(), new Point2D(0, 0), new Size2D(10, 10));
        locked.SetLocked(true);
        scene.Add(locked);
        var command = new MoveObjectsCommand([locked.Id], new Point2D(50, 50));

        command.Execute(scene);

        Assert.Equal(0.0, scene.Get(locked.Id).Position.X);
    }

    [Fact]
    public void Undo_RestitueLaPositionExacte()
    {
        var scene = new CanvasScene();
        var obj = CanvasObject.Create(CanvasObjectId.New(), new Point2D(12.5, -3.25), new Size2D(10, 10));
        scene.Add(obj);
        var command = new MoveObjectsCommand([obj.Id], new Point2D(7.75, 100));

        command.Execute(scene);
        command.Undo(scene);

        Assert.Equal(12.5, scene.Get(obj.Id).Position.X);
        Assert.Equal(-3.25, scene.Get(obj.Id).Position.Y);
    }
}
