using ElectricalDesigner.Canvas.Commands;
using ElectricalDesigner.Canvas.Geometry;
using ElectricalDesigner.Canvas.Scene;
using ElectricalDesigner.TestKit;

namespace ElectricalDesigner.Canvas.Tests.Commands;

public class CommandHistoryTests
{
    [Fact]
    public void Execute_AjouteUneEntreeAnnulable()
    {
        var scene = new CanvasScene();
        var history = new CommandHistory();
        var obj = CanvasObject.Create(CanvasObjectId.New(), Point2D.Zero, new Size2D(10, 10));

        history.Execute(new AddObjectCommand(obj), scene);

        Assert.True(scene.Contains(obj.Id));
        Assert.True(history.CanUndo);
        Assert.False(history.CanRedo);
    }

    [Fact]
    public void Undo_AnnuleLaDerniereCommande()
    {
        var scene = new CanvasScene();
        var history = new CommandHistory();
        var obj = CanvasObject.Create(CanvasObjectId.New(), Point2D.Zero, new Size2D(10, 10));
        history.Execute(new AddObjectCommand(obj), scene);

        history.Undo(scene);

        Assert.False(scene.Contains(obj.Id));
        Assert.False(history.CanUndo);
        Assert.True(history.CanRedo);
    }

    [Fact]
    public void Redo_ReapliqueLaCommandeAnnulee()
    {
        var scene = new CanvasScene();
        var history = new CommandHistory();
        var obj = CanvasObject.Create(CanvasObjectId.New(), Point2D.Zero, new Size2D(10, 10));
        history.Execute(new AddObjectCommand(obj), scene);
        history.Undo(scene);

        history.Redo(scene);

        Assert.True(scene.Contains(obj.Id));
    }

    [Fact]
    public void Execute_ApresUnUndo_ViideLaPileDeRedo()
    {
        var scene = new CanvasScene();
        var history = new CommandHistory();
        var first = CanvasObject.Create(CanvasObjectId.New(), Point2D.Zero, new Size2D(10, 10));
        var second = CanvasObject.Create(CanvasObjectId.New(), new Point2D(20, 20), new Size2D(10, 10));
        history.Execute(new AddObjectCommand(first), scene);
        history.Undo(scene);

        history.Execute(new AddObjectCommand(second), scene);

        Assert.False(history.CanRedo);
    }

    [Fact]
    public void Undo_SansHistorique_Echoue()
    {
        var history = new CommandHistory();
        var scene = new CanvasScene();

        Assert.Throws<InvalidOperationException>(() => history.Undo(scene));
    }

    [Fact]
    public void SequenceLongue_CreerDeplacerAnnulerAnnuler_RestitueLEtatInitial()
    {
        // Test de propriété (cahier §34.2) : create -> move -> undo -> undo restitue l'état vide.
        var scene = new CanvasScene();
        var history = new CommandHistory();
        var obj = CanvasObject.Create(CanvasObjectId.New(), new Point2D(0, 0), new Size2D(10, 10));

        history.Execute(new AddObjectCommand(obj), scene);
        history.Execute(new MoveObjectsCommand([obj.Id], new Point2D(50, 50)), scene);

        Assert.Equal(50.0, scene.Get(obj.Id).Position.X);

        history.Undo(scene); // annule le déplacement
        Assert.Equal(0.0, scene.Get(obj.Id).Position.X);

        history.Undo(scene); // annule la création
        Assert.Equal(0, scene.Count);
    }
}
