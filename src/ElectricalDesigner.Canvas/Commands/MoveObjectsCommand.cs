using ElectricalDesigner.Canvas.Geometry;
using ElectricalDesigner.Canvas.Scene;

namespace ElectricalDesigner.Canvas.Commands;

/// <summary>
/// Déplacement d'un ou plusieurs objets par un même delta (guide §6 étape G07 ;
/// guide §10 : "MoveSymbolCommand"). Un seul <see cref="MoveObjectsCommand"/>
/// pour toute une sélection multiple (étape G10) produit une seule entrée
/// d'historique pour un déplacement groupé — exactement l'avantage "actions
/// groupables" mentionné au guide §10. Les objets verrouillés sont ignorés,
/// silencieusement, aussi bien à l'exécution qu'à l'annulation.
/// </summary>
public sealed class MoveObjectsCommand(IReadOnlyList<CanvasObjectId> ids, Point2D delta) : ICanvasCommand
{
    private readonly IReadOnlyList<CanvasObjectId> _ids = ids ?? throw new ArgumentNullException(nameof(ids));
    private readonly Point2D _delta = delta;

    public string Description => _ids.Count == 1
        ? $"Déplacer l'objet {_ids[0]}"
        : $"Déplacer {_ids.Count} objets";

    public void Execute(CanvasScene scene)
    {
        foreach (var id in _ids)
        {
            var canvasObject = scene.Get(id);
            if (!canvasObject.Locked)
            {
                canvasObject.MoveBy(_delta);
            }
        }
    }

    public void Undo(CanvasScene scene)
    {
        foreach (var id in _ids)
        {
            var canvasObject = scene.Get(id);
            if (!canvasObject.Locked)
            {
                canvasObject.MoveBy(_delta * -1);
            }
        }
    }
}
