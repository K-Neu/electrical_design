using ElectricalDesigner.Canvas.Geometry;
using ElectricalDesigner.Canvas.Persistence;
using ElectricalDesigner.Canvas.Scene;
using ElectricalDesigner.TestKit;

namespace ElectricalDesigner.Canvas.Tests.Persistence;

public class SceneSerializerTests
{
    [Fact]
    public void ToJson_PuisFromJson_PreserveToutesLesProprietes()
    {
        var scene = new CanvasScene { Grid = new GridSettings { SpacingWorld = 25, SnapEnabled = false } };
        var obj = CanvasObject.Create(
            CanvasObjectId.New(), new Point2D(12.5, -7.25), new Size2D(30, 40),
            rotationDegrees: 123, scale: 1.5, visible: false, locked: true, layer: "annotations",
            businessObjectId: Guid.NewGuid());
        scene.Add(obj);

        var json = SceneSerializer.ToJson(scene);
        var reloaded = SceneSerializer.FromJson(json);

        var reloadedObj = reloaded.Get(obj.Id);
        Assert.Equal(obj.Position.X, reloadedObj.Position.X);
        Assert.Equal(obj.Position.Y, reloadedObj.Position.Y);
        Assert.Equal(obj.Size.Width, reloadedObj.Size.Width);
        Assert.Equal(obj.RotationDegrees, reloadedObj.RotationDegrees);
        Assert.Equal(obj.Scale, reloadedObj.Scale);
        Assert.Equal(obj.Visible, reloadedObj.Visible);
        Assert.Equal(obj.Locked, reloadedObj.Locked);
        Assert.Equal(obj.Layer, reloadedObj.Layer);
        Assert.Equal(obj.BusinessObjectId, reloadedObj.BusinessObjectId);
        Assert.Equal(25.0, reloaded.Grid.SpacingWorld);
        Assert.False(reloaded.Grid.SnapEnabled);
    }

    [Fact]
    public void SaveToFile_PuisLoadFromFile_PreserveLesCoordonneesSansPerte()
    {
        var scene = new CanvasScene();
        for (var i = 0; i < 100; i++)
        {
            scene.Add(CanvasObject.Create(
                CanvasObjectId.New(), new Point2D(i * 1.1, i * -2.3), new Size2D(5, 5)));
        }

        var filePath = Path.Combine(Path.GetTempPath(), "electrical-designer-canvas-tests", Guid.NewGuid().ToString("n"), "scene.json");

        try
        {
            SceneSerializer.SaveToFile(scene, filePath);
            var reloaded = SceneSerializer.LoadFromFile(filePath);

            Assert.Equal(100, reloaded.Count);
            foreach (var original in scene.Objects)
            {
                var reloadedObj = reloaded.Get(original.Id);
                Assert.Equal(original.Position.X, reloadedObj.Position.X);
                Assert.Equal(original.Position.Y, reloadedObj.Position.Y);
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

    [Fact]
    public void LoadFromFile_AvecFichierInexistant_LeveFileNotFoundException()
    {
        var missingPath = Path.Combine(Path.GetTempPath(), "electrical-designer-canvas-tests", Guid.NewGuid().ToString("n"), "absent.json");

        Assert.Throws<FileNotFoundException>(() => SceneSerializer.LoadFromFile(missingPath));
    }
}
