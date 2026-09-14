using ElectricalDesigner.Canvas.Geometry;
using ElectricalDesigner.Canvas.Scene;

namespace ElectricalDesigner.Canvas.Clipboard;

/// <summary>
/// Copier/coller (guide §6, étape G12). Le presse-papiers ne mute jamais la
/// scène lui-même : il expose des <see cref="CanvasObject"/> déjà clonés avec
/// de nouveaux identifiants, que l'appelant insère via
/// <see cref="Commands.AddObjectCommand"/> (généralement enveloppées dans un
/// <see cref="Commands.CompositeCommand"/> pour qu'un collage multiple ne
/// produise qu'une seule entrée d'historique).
/// </summary>
public sealed class CanvasClipboard
{
    private IReadOnlyList<CanvasObject> _copied = [];

    public bool HasContent => _copied.Count > 0;

    public int Count => _copied.Count;

    /// <summary>Copie un instantané des objets donnés (guide §12.1 cahier : "copie de groupes de symboles").</summary>
    public void Copy(IEnumerable<CanvasObject> objects)
    {
        ArgumentNullException.ThrowIfNull(objects);
        // Clone à l'identique (même id) : un instantané de copie n'est pas encore
        // destiné à être inséré tel quel — c'est Paste() qui régénère des identifiants.
        _copied = objects.Select(o => o.CloneAsNew(Point2D.Zero, preserveBusinessLink: true)).ToList();
    }

    /// <summary>
    /// Produit de NOUVEAUX objets (nouveaux identifiants), décalés de
    /// <paramref name="offset"/> par rapport à la copie d'origine — comportement
    /// standard "coller avec léger décalage" pour distinguer visuellement la
    /// copie de la source.
    /// </summary>
    public IReadOnlyList<CanvasObject> Paste(Point2D offset)
    {
        if (!HasContent)
        {
            return [];
        }

        return _copied.Select(o => o.CloneAsNew(offset)).ToList();
    }

    public void Clear() => _copied = [];
}
