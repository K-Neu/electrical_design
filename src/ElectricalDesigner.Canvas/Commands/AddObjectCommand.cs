using ElectricalDesigner.Canvas.Scene;

namespace ElectricalDesigner.Canvas.Commands;

/// <summary>Création d'un objet (guide §10 : "CreateSymbolCommand").</summary>
public sealed class AddObjectCommand(CanvasObject canvasObject) : ICanvasCommand
{
    private readonly CanvasObject _canvasObject = canvasObject ?? throw new ArgumentNullException(nameof(canvasObject));

    public string Description => $"Ajouter l'objet {_canvasObject.Id}";

    public void Execute(CanvasScene scene) => scene.Add(_canvasObject);

    public void Undo(CanvasScene scene) => scene.Remove(_canvasObject.Id);
}
