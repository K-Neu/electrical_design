using ElectricalDesigner.Canvas.Geometry;

namespace ElectricalDesigner.Canvas.Scene;

/// <summary>
/// Objet graphique générique (guide §6, étape G05 ; propriétés graphiques
/// exactes du guide §5.4 : x, y, width, height, rotation, scale, visible,
/// locked, layer). Volontairement ignorant de toute sémantique électrique —
/// c'est la "vérité graphique" du guide §1.3, strictement séparée de la
/// "vérité électrique" portée par <c>ElectricalDesigner.Domain</c>.
///
/// <see cref="BusinessObjectId"/> est le SEUL point de contact prévu avec le
/// métier : un <c>Guid</c> optionnel et opaque, jamais un type ou une
/// référence de projet vers Domain (voir docs/04_Canvas.md et
/// docs/01_Architecture.md §3/§7). Il sera renseigné en Phase 3/4 pour relier
/// un objet canvas à un <c>ElectricalSymbolInstance</c>.
///
/// Les mutations passent normalement par le pattern Command (guide §10,
/// <see cref="Commands.ICanvasCommand"/>) pour bénéficier de l'undo/redo ;
/// les setters ci-dessous restent néanmoins publics pour que les commandes
/// elles-mêmes (et les tests) puissent les appeler directement sans
/// indirection superflue.
/// </summary>
public sealed class CanvasObject
{
    public CanvasObjectId Id { get; }

    public Point2D Position { get; private set; }

    public Size2D Size { get; private set; }

    /// <summary>Angle de rotation en degrés, autour du centre de l'objet.</summary>
    public double RotationDegrees { get; private set; }

    public double Scale { get; private set; }

    public bool Visible { get; private set; }

    public bool Locked { get; private set; }

    /// <summary>Nom de calque libre (ex. "default", "fond", "annotations"). Guide §5.4/§6.</summary>
    public string Layer { get; private set; }

    /// <summary>Lien optionnel vers une entité métier (Phase 3/4) — jamais une référence de type.</summary>
    public Guid? BusinessObjectId { get; private set; }

    private CanvasObject(
        CanvasObjectId id,
        Point2D position,
        Size2D size,
        double rotationDegrees,
        double scale,
        bool visible,
        bool locked,
        string layer,
        Guid? businessObjectId)
    {
        Id = id;
        Position = position;
        Size = size;
        RotationDegrees = NormalizeDegrees(rotationDegrees);
        Scale = scale;
        Visible = visible;
        Locked = locked;
        Layer = layer;
        BusinessObjectId = businessObjectId;
    }

    public static CanvasObject Create(
        CanvasObjectId id,
        Point2D position,
        Size2D size,
        double rotationDegrees = 0,
        double scale = 1.0,
        bool visible = true,
        bool locked = false,
        string layer = "default",
        Guid? businessObjectId = null)
    {
        if (id.IsEmpty)
        {
            throw new ArgumentException("Un objet canvas doit avoir un identifiant non vide.", nameof(id));
        }

        if (size.Width < 0 || size.Height < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(size), "La largeur et la hauteur ne peuvent pas être négatives.");
        }

        if (scale <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(scale), "L'échelle doit être positive.");
        }

        if (string.IsNullOrWhiteSpace(layer))
        {
            throw new ArgumentException("Le calque est obligatoire.", nameof(layer));
        }

        return new CanvasObject(id, position, size, rotationDegrees, scale, visible, locked, layer.Trim(), businessObjectId);
    }

    /// <summary>Reconstruction depuis la persistance (guide §5.1 — aucun horodatage à préserver ici, l'objet est purement graphique).</summary>
    public static CanvasObject Restore(
        CanvasObjectId id,
        Point2D position,
        Size2D size,
        double rotationDegrees,
        double scale,
        bool visible,
        bool locked,
        string layer,
        Guid? businessObjectId) =>
        new(id, position, size, rotationDegrees, scale, visible, locked, layer, businessObjectId);

    // ---------------------------------------------------------------
    // Mutateurs (appelés par les Commands — voir Commands/)
    // ---------------------------------------------------------------

    public void MoveTo(Point2D position) => Position = position;

    public void MoveBy(Point2D delta) => Position += delta;

    public void SetRotation(double degrees) => RotationDegrees = NormalizeDegrees(degrees);

    public void RotateBy(double degrees) => RotationDegrees = NormalizeDegrees(RotationDegrees + degrees);

    public void Resize(Size2D size)
    {
        if (size.Width < 0 || size.Height < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(size), "La largeur et la hauteur ne peuvent pas être négatives.");
        }

        Size = size;
    }

    public void SetScale(double scale)
    {
        if (scale <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(scale), "L'échelle doit être positive.");
        }

        Scale = scale;
    }

    public void SetVisible(bool visible) => Visible = visible;

    public void SetLocked(bool locked) => Locked = locked;

    public void SetLayer(string layer)
    {
        if (string.IsNullOrWhiteSpace(layer))
        {
            throw new ArgumentException("Le calque est obligatoire.", nameof(layer));
        }

        Layer = layer.Trim();
    }

    public void LinkToBusinessObject(Guid? businessObjectId) => BusinessObjectId = businessObjectId;

    /// <summary>
    /// Boîte englobante non tournée en coordonnées monde. Sert de base au
    /// hit-testing simple (étape G15, sous-ensemble Phase 2 — voir
    /// docs/04_Canvas.md pour la limite assumée sur les objets pivotés).
    /// </summary>
    public Rect2D Bounds => new(Position.X, Position.Y, Size.Width * Scale, Size.Height * Scale);

    /// <summary>Copie indépendante avec un NOUVEL identifiant, décalée de <paramref name="offset"/> (copier/coller, étape G12).</summary>
    public CanvasObject CloneAsNew(Point2D offset, bool preserveBusinessLink = false) =>
        new(
            CanvasObjectId.New(),
            Position + offset,
            Size,
            RotationDegrees,
            Scale,
            Visible,
            locked: false, // Un objet collé n'hérite jamais du verrouillage de sa source.
            Layer,
            preserveBusinessLink ? BusinessObjectId : null);

    private static double NormalizeDegrees(double degrees)
    {
        var normalized = degrees % 360.0;
        return normalized < 0 ? normalized + 360.0 : normalized;
    }
}
