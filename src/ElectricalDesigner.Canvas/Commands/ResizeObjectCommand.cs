using ElectricalDesigner.Canvas.Geometry;
using ElectricalDesigner.Canvas.Scene;

namespace ElectricalDesigner.Canvas.Commands;

/// <summary>Redimensionnement absolu d'un objet (guide §6 étape G09).</summary>
public sealed class ResizeObjectCommand(CanvasObjectId id, Size2D newSize) : ICanvasCommand
{
    private readonly CanvasObjectId _id = id;
    private readonly Size2D _newSize = newSize;
    private Size2D _previousSize;

    public string Description => $"Redimensionner l'objet {_id}";

    public void Execute(CanvasScene scene)
    {
        var canvasObject = scene.Get(_id);
        if (canvasObject.Locked)
        {
            return;
        }

        _previousSize = canvasObject.Size;
        canvasObject.Resize(_newSize);
    }

    public void Undo(CanvasScene scene)
    {
        var canvasObject = scene.Get(_id);
        if (!canvasObject.Locked)
        {
            canvasObject.Resize(_previousSize);
        }
    }
}
