using ElectricalDesigner.Canvas.Scene;

namespace ElectricalDesigner.Canvas.Commands;

/// <summary>
/// Suppression d'un objet (guide §10 : "DeleteSymbolCommand"). L'instance
/// retirée de la scène par <see cref="Execute"/> reste en mémoire (le
/// dictionnaire interne de <see cref="CanvasScene"/> ne fait que perdre sa
/// référence) : <see cref="Undo"/> peut donc la réinsérer telle quelle, sans
/// avoir besoin de cloner un instantané au moment de la construction de la
/// commande.
/// </summary>
public sealed class RemoveObjectCommand(CanvasObjectId id) : ICanvasCommand
{
    private readonly CanvasObjectId _id = id;
    private CanvasObject? _removed;

    public string Description => $"Supprimer l'objet {_id}";

    public void Execute(CanvasScene scene)
    {
        _removed = scene.Get(_id);
        scene.Remove(_id);
    }

    public void Undo(CanvasScene scene)
    {
        if (_removed is not null)
        {
            scene.Add(_removed);
        }
    }
}
