using ElectricalDesigner.Canvas.Clipboard;
using ElectricalDesigner.Canvas.Geometry;
using ElectricalDesigner.Canvas.Scene;
using ElectricalDesigner.TestKit;

namespace ElectricalDesigner.Canvas.Tests.Clipboard;

public class CanvasClipboardTests
{
    [Fact]
    public void Copy_PuisPaste_ProduitDeNouveauxIdentifiants()
    {
        var clipboard = new CanvasClipboard();
        var original = CanvasObject.Create(CanvasObjectId.New(), new Point2D(10, 10), new Size2D(5, 5));

        clipboard.Copy([original]);
        var pasted = clipboard.Paste(new Point2D(20, 0));

        Assert.Equal(1, pasted.Count);
        Assert.False(pasted[0].Id == original.Id);
        Assert.Equal(30.0, pasted[0].Position.X);
    }

    [Fact]
    public void Copy_GroupeDeSymboles_PreserveLeNombreAuCollage()
    {
        // Cahier §12.1 : "copie de groupes de symboles".
        var clipboard = new CanvasClipboard();
        var objects = Enumerable.Range(0, 10)
            .Select(i => CanvasObject.Create(CanvasObjectId.New(), new Point2D(i, i), new Size2D(1, 1)))
            .ToList();

        clipboard.Copy(objects);
        var pasted = clipboard.Paste(new Point2D(100, 100));

        Assert.Equal(10, pasted.Count);
    }

    [Fact]
    public void Paste_SansCopiePrealable_RetourneUneListeVide()
    {
        var clipboard = new CanvasClipboard();

        var pasted = clipboard.Paste(Point2D.Zero);

        Assert.Equal(0, pasted.Count);
    }

    [Fact]
    public void Paste_NHeritePasDuLienMetierDeLaSource()
    {
        var clipboard = new CanvasClipboard();
        var original = CanvasObject.Create(CanvasObjectId.New(), Point2D.Zero, new Size2D(5, 5));
        original.LinkToBusinessObject(Guid.NewGuid());

        clipboard.Copy([original]);
        var pasted = clipboard.Paste(Point2D.Zero);

        Assert.True(pasted[0].BusinessObjectId is null);
    }
}
