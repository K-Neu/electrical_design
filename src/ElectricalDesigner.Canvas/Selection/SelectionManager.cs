using ElectricalDesigner.Canvas.Scene;

namespace ElectricalDesigner.Canvas.Selection;

/// <summary>
/// Sélection simple et multiple (guide §6, étapes G06 et G10). Ne stocke que
/// des identifiants, jamais de référence directe vers <see cref="CanvasObject"/> :
/// la sélection reste valide même si la scène change de représentation en
/// mémoire (ex. après un rechargement), tant que les identifiants persistent.
/// </summary>
public sealed class SelectionManager
{
    private readonly HashSet<CanvasObjectId> _selectedIds = [];

    public IReadOnlySet<CanvasObjectId> SelectedIds => _selectedIds;

    public bool IsEmpty => _selectedIds.Count == 0;

    public int Count => _selectedIds.Count;

    public bool IsSelected(CanvasObjectId id) => _selectedIds.Contains(id);

    /// <summary>Sélection simple (étape G06) : remplace toute sélection précédente.</summary>
    public void Select(CanvasObjectId id)
    {
        _selectedIds.Clear();
        _selectedIds.Add(id);
    }

    /// <summary>Multi-sélection (étape G10) : remplace la sélection par l'ensemble donné (ex. rubber-band).</summary>
    public void SelectRange(IEnumerable<CanvasObjectId> ids)
    {
        _selectedIds.Clear();
        _selectedIds.UnionWith(ids);
    }

    /// <summary>Ajoute à la sélection courante sans la remplacer (ex. Ctrl+clic).</summary>
    public void Add(CanvasObjectId id) => _selectedIds.Add(id);

    public void AddRange(IEnumerable<CanvasObjectId> ids) => _selectedIds.UnionWith(ids);

    /// <summary>Bascule la présence d'un objet dans la sélection (ex. Ctrl+clic sur un objet déjà sélectionné).</summary>
    public void Toggle(CanvasObjectId id)
    {
        if (!_selectedIds.Remove(id))
        {
            _selectedIds.Add(id);
        }
    }

    public void Deselect(CanvasObjectId id) => _selectedIds.Remove(id);

    public void Clear() => _selectedIds.Clear();
}
