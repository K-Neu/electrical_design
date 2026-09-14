using ElectricalDesigner.Canvas.Scene;
using ElectricalDesigner.Canvas.Selection;
using ElectricalDesigner.TestKit;

namespace ElectricalDesigner.Canvas.Tests.Selection;

public class SelectionManagerTests
{
    [Fact]
    public void Select_RemplaceLaSelectionPrecedente()
    {
        var selection = new SelectionManager();
        var first = CanvasObjectId.New();
        var second = CanvasObjectId.New();

        selection.Select(first);
        selection.Select(second);

        Assert.Equal(1, selection.Count);
        Assert.True(selection.IsSelected(second));
        Assert.False(selection.IsSelected(first));
    }

    [Fact]
    public void SelectRange_MetEnPlaceUneMultiSelection()
    {
        var selection = new SelectionManager();
        var ids = Enumerable.Range(0, 5).Select(_ => CanvasObjectId.New()).ToList();

        selection.SelectRange(ids);

        Assert.Equal(5, selection.Count);
        Assert.True(ids.All(selection.IsSelected));
    }

    [Fact]
    public void Toggle_ObjetNonSelectionne_LAjoute()
    {
        var selection = new SelectionManager();
        var id = CanvasObjectId.New();

        selection.Toggle(id);

        Assert.True(selection.IsSelected(id));
    }

    [Fact]
    public void Toggle_ObjetDejaSelectionne_LeRetire()
    {
        var selection = new SelectionManager();
        var id = CanvasObjectId.New();
        selection.Add(id);

        selection.Toggle(id);

        Assert.False(selection.IsSelected(id));
    }

    [Fact]
    public void Clear_ViideLaSelection()
    {
        var selection = new SelectionManager();
        selection.AddRange([CanvasObjectId.New(), CanvasObjectId.New()]);

        selection.Clear();

        Assert.True(selection.IsEmpty);
    }
}
