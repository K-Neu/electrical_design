namespace ElectricalDesigner.Canvas.Scene;

/// <summary>
/// Identifiant fortement typé d'un objet graphique (même principe que
/// <c>EntityId&lt;TEntity&gt;</c> côté Domain, guide §5.1) : la position
/// graphique n'est jamais utilisée comme identité, même ici où il n'y a pas
/// encore de notion métier. Volontairement dupliqué plutôt que réutilisé
/// depuis Domain, pour garder <c>ElectricalDesigner.Canvas</c> à zéro
/// dépendance (voir docs/04_Canvas.md).
/// </summary>
public readonly record struct CanvasObjectId(Guid Value)
{
    public static CanvasObjectId New() => new(Guid.NewGuid());

    public static readonly CanvasObjectId Empty = new(Guid.Empty);

    public bool IsEmpty => Value == Guid.Empty;

    public override string ToString() => Value.ToString();
}
