using ElectricalDesigner.Canvas.Clipboard;
using ElectricalDesigner.Canvas.Commands;
using ElectricalDesigner.Canvas.Geometry;
using ElectricalDesigner.Canvas.Persistence;
using ElectricalDesigner.Canvas.Scene;
using ElectricalDesigner.Canvas.Selection;
using ElectricalDesigner.TestKit;

namespace ElectricalDesigner.Canvas.Tests.Integration;

/// <summary>
/// Matérialise très exactement le critère de sortie de la Phase 2 (guide §Phase 2) :
/// "placer 100 objets sur une scène, les déplacer, les sélectionner, zoomer,
/// annuler et sauvegarder leurs coordonnées sans perte."
/// </summary>
public class Phase2ExitCriteriaTests
{
    [Fact]
    public void ScenarioCritereDeSortiePhase2_Complet()
    {
        var scene = new CanvasScene();
        var history = new CommandHistory();
        var selection = new SelectionManager();
        var clipboard = new CanvasClipboard();
        var transform = WorldTransform.Identity;

        // --- 1. Placer 100 objets sur une scène ---
        var ids = new List<CanvasObjectId>();
        for (var i = 0; i < 100; i++)
        {
            var obj = CanvasObject.Create(
                CanvasObjectId.New(),
                new Point2D(i * 10, i * 5),
                new Size2D(20, 20));
            history.Execute(new AddObjectCommand(obj), scene);
            ids.Add(obj.Id);
        }

        Assert.Equal(100, scene.Count);

        // --- 2. Les sélectionner (multi-sélection, étape G10) ---
        selection.SelectRange(ids.Take(50));
        Assert.Equal(50, selection.Count);

        // --- 3. Les déplacer (un seul déplacement groupé pour toute la sélection) ---
        var delta = new Point2D(1000, 1000);
        history.Execute(new MoveObjectsCommand(selection.SelectedIds.ToList(), delta), scene);

        foreach (var id in selection.SelectedIds)
        {
            Assert.Equal(1000.0 <= scene.Get(id).Position.X, true);
        }

        // --- 4. Zoomer (centré sur le curseur) ---
        var cursor = new Point2D(400, 300);
        var worldUnderCursorBefore = transform.ScreenToWorld(cursor);
        transform = transform.ZoomAt(cursor, 1.5);
        var worldUnderCursorAfter = transform.ScreenToWorld(cursor);
        Assert.True(Math.Abs(worldUnderCursorBefore.X - worldUnderCursorAfter.X) < 1e-9);

        // --- copier/coller un sous-ensemble pendant qu'on y est (étape G12) ---
        var toCopy = ids.Take(3).Select(scene.Get);
        clipboard.Copy(toCopy);
        var pasted = clipboard.Paste(new Point2D(5, 5));
        var pasteCommand = new CompositeCommand(
            pasted.Select(p => (ICanvasCommand)new AddObjectCommand(p)).ToList(),
            "Coller 3 objets");
        history.Execute(pasteCommand, scene);
        Assert.Equal(103, scene.Count);

        // --- 5. Annuler (undo/redo, étape G13) ---
        history.Undo(scene); // annule le collage
        Assert.Equal(100, scene.Count);

        history.Undo(scene); // annule le déplacement groupé
        foreach (var id in ids)
        {
            Assert.True(scene.Get(id).Position.X < 1000.0);
        }

        // --- 6. Sauvegarder leurs coordonnées sans perte ---
        var filePath = Path.Combine(
            Path.GetTempPath(), "electrical-designer-canvas-tests", Guid.NewGuid().ToString("n"), "phase2-scene.json");

        try
        {
            SceneSerializer.SaveToFile(scene, filePath);
            var reloaded = SceneSerializer.LoadFromFile(filePath);

            Assert.Equal(scene.Count, reloaded.Count);
            foreach (var original in scene.Objects)
            {
                var reloadedObj = reloaded.Get(original.Id);
                Assert.Equal(original.Position.X, reloadedObj.Position.X);
                Assert.Equal(original.Position.Y, reloadedObj.Position.Y);
                Assert.Equal(original.RotationDegrees, reloadedObj.RotationDegrees);
            }
        }
        finally
        {
            var directory = Path.GetDirectoryName(filePath);
            if (directory is not null && Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }
}
