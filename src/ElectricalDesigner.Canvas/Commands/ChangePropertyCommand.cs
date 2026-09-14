using ElectricalDesigner.Canvas.Scene;

namespace ElectricalDesigner.Canvas.Commands;

/// <summary>
/// Commande générique de changement de propriété (guide §10 : "ChangePropertyCommand"),
/// réutilisable pour toute propriété simple de <see cref="CanvasObject"/> (calque,
/// verrouillage, visibilité, lien métier...) sans devoir écrire une classe de
/// commande dédiée pour chacune.
/// </summary>
public sealed class ChangePropertyCommand<TValue> : ICanvasCommand
{
    private readonly CanvasObjectId _id;
    private readonly Func<CanvasObject, TValue> _getter;
    private readonly Action<CanvasObject, TValue> _setter;
    private readonly TValue _newValue;
    private readonly string _propertyName;
    private TValue? _previousValue;

    public ChangePropertyCommand(
        CanvasObjectId id,
        string propertyName,
        Func<CanvasObject, TValue> getter,
        Action<CanvasObject, TValue> setter,
        TValue newValue)
    {
        _id = id;
        _propertyName = propertyName;
        _getter = getter ?? throw new ArgumentNullException(nameof(getter));
        _setter = setter ?? throw new ArgumentNullException(nameof(setter));
        _newValue = newValue;
    }

    public string Description => $"Modifier {_propertyName} de l'objet {_id}";

    public void Execute(CanvasScene scene)
    {
        var canvasObject = scene.Get(_id);
        _previousValue = _getter(canvasObject);
        _setter(canvasObject, _newValue);
    }

    public void Undo(CanvasScene scene)
    {
        var canvasObject = scene.Get(_id);
        _setter(canvasObject, _previousValue!);
    }
}
