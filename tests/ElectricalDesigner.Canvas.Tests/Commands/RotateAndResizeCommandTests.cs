using ElectricalDesigner.Canvas.Commands;
using ElectricalDesigner.Canvas.Geometry;
using ElectricalDesigner.Canvas.Scene;
using ElectricalDesigner.TestKit;

namespace ElectricalDesigner.Canvas.Tests.Commands;

public class RotateAndResizeCommandTests
{
    [Fact]
    public void RotateObjectCommand_UndoRestitueLAngleInitial()
    {
        var scene = new CanvasScene();
        var obj = CanvasObject.Create(CanvasObjectId.New(), Point2D.Zero, new Size2D(10, 10));
        obj.SetRotation(45);
        scene.Add(obj);
        var command = new RotateObjectCommand(obj.Id, 90);

        command.Execute(scene);
        Assert.Equal(135.0, scene.Get(obj.Id).RotationDegrees);

        command.Undo(scene);
        Assert.Equal(45.0, scene.Get(obj.Id).RotationDegrees);
    }

    [Fact]
    public void ResizeObjectCommand_UndoRestitueLaTailleInitiale()
    {
        var scene = new CanvasScene();
        var obj = CanvasObject.Create(CanvasObjectId.New(), Point2D.Zero, new Size2D(10, 20));
        scene.Add(obj);
        var command = new ResizeObjectCommand(obj.Id, new Size2D(100, 200));

        command.Execute(scene);
        Assert.Equal(100.0, scene.Get(obj.Id).Size.Width);

        command.Undo(scene);
        Assert.Equal(10.0, scene.Get(obj.Id).Size.Width);
        Assert.Equal(20.0, scene.Get(obj.Id).Size.Height);
    }

    [Fact]
    public void ChangePropertyCommand_ModifieEtRestitueLeCalque()
    {
        var scene = new CanvasScene();
        var obj = CanvasObject.Create(CanvasObjectId.New(), Point2D.Zero, new Size2D(10, 10), layer: "default");
        scene.Add(obj);
        var command = new ChangePropertyCommand<string>(
            obj.Id, "Layer", o => o.Layer, (o, v) => o.SetLayer(v), "annotations");

        command.Execute(scene);
        Assert.Equal("annotations", scene.Get(obj.Id).Layer);

        command.Undo(scene);
        Assert.Equal("default", scene.Get(obj.Id).Layer);
    }

    [Fact]
    public void RemoveObjectCommand_UndoReinserreLObjetIdentique()
    {
        var scene = new CanvasScene();
        var obj = CanvasObject.Create(CanvasObjectId.New(), new Point2D(1, 2), new Size2D(3, 4));
        scene.Add(obj);
        var command = new RemoveObjectCommand(obj.Id);

        command.Execute(scene);
        Assert.False(scene.Contains(obj.Id));

        command.Undo(scene);
        Assert.True(scene.Contains(obj.Id));
        Assert.Equal(1.0, scene.Get(obj.Id).Position.X);
    }

    [Fact]
    public void CompositeCommand_AnnuleDansLOrdreInverse()
    {
        var scene = new CanvasScene();
        var a = CanvasObject.Create(CanvasObjectId.New(), Point2D.Zero, new Size2D(10, 10));
        var b = CanvasObject.Create(CanvasObjectId.New(), Point2D.Zero, new Size2D(10, 10));
        var composite = new CompositeCommand([new AddObjectCommand(a), new AddObjectCommand(b)], "Ajouter 2 objets");

        composite.Execute(scene);
        Assert.Equal(2, scene.Count);

        composite.Undo(scene);
        Assert.Equal(0, scene.Count);
    }
}
