using ElectricalDesigner.Canvas.Scene;

namespace ElectricalDesigner.Canvas.Commands;

/// <summary>Rotation relative d'un objet (guide §6 étape G08 ; guide §10 : "RotateSymbolCommand").</summary>
public sealed class RotateObjectCommand(CanvasObjectId id, double deltaDegrees) : ICanvasCommand
{
    private readonly CanvasObjectId _id = id;
    private readonly double _deltaDegrees = deltaDegrees;

    public string Description => $"Faire pivoter l'objet {_id} de {_deltaDegrees}°";

    public void Execute(CanvasScene scene)
    {
        var canvasObject = scene.Get(_id);
        if (!canvasObject.Locked)
        {
            canvasObject.RotateBy(_deltaDegrees);
        }
    }

    public void Undo(CanvasScene scene)
    {
        var canvasObject = scene.Get(_id);
        if (!canvasObject.Locked)
        {
            canvasObject.RotateBy(-_deltaDegrees);
        }
    }
}
