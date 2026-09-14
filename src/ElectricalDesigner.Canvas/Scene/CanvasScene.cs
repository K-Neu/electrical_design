using ElectricalDesigner.Canvas.Geometry;

namespace ElectricalDesigner.Canvas.Scene;

/// <summary>
/// Scène 2D générique : la collection d'objets graphiques d'une page (guide §6).
/// Ne connaît rien du métier — voir <see cref="CanvasObject"/>. Réutilisable telle
/// quelle par l'éditeur de schéma (Phase 3) et l'éditeur de plan (Phase 4), une
/// scène par page.
/// </summary>
public sealed class CanvasScene
{
    private readonly Dictionary<CanvasObjectId, CanvasObject> _objects = [];

    public GridSettings Grid { get; set; } = GridSettings.Default;

    public IReadOnlyCollection<CanvasObject> Objects => _objects.Values;

    public int Count => _objects.Count;

    public void Add(CanvasObject canvasObject)
    {
        ArgumentNullException.ThrowIfNull(canvasObject);

        if (!_objects.TryAdd(canvasObject.Id, canvasObject))
        {
            throw new ArgumentException($"Un objet avec l'identifiant '{canvasObject.Id}' existe déjà dans la scène.", nameof(canvasObject));
        }
    }

    public bool Remove(CanvasObjectId id) => _objects.Remove(id);

    public bool Contains(CanvasObjectId id) => _objects.ContainsKey(id);

    public CanvasObject Get(CanvasObjectId id) =>
        _objects.TryGetValue(id, out var found)
            ? found
            : throw new KeyNotFoundException($"Aucun objet canvas avec l'identifiant '{id}'.");

    public bool TryGet(CanvasObjectId id, out CanvasObject? canvasObject) => _objects.TryGetValue(id, out canvasObject);

    /// <summary>
    /// Hit-test ponctuel (étape G15, sous-ensemble Phase 2) : renvoie les objets
    /// visibles dont la boîte englobante contient <paramref name="worldPoint"/>,
    /// de l'objet le plus "au-dessus" (le plus récemment ajouté) au plus en dessous.
    /// Le hit-testing précis sur formes pivotées est différé à la Phase 3, quand
    /// de vraies formes de symboles existeront (voir docs/04_Canvas.md).
    /// </summary>
    public IEnumerable<CanvasObject> HitTest(Point2D worldPoint) =>
        _objects.Values
            .Where(o => o.Visible && o.Bounds.Contains(worldPoint))
            .Reverse();

    /// <summary>Sélection rectangulaire ("rubber-band", étape G10) : objets visibles dont la boîte englobante intersecte <paramref name="worldRect"/>.</summary>
    public IEnumerable<CanvasObject> QueryRect(Rect2D worldRect) =>
        _objects.Values.Where(o => o.Visible && o.Bounds.Intersects(worldRect));

    public void Clear() => _objects.Clear();
}
