namespace ElectricalDesigner.Domain.Cabling;

/// <summary>
/// Catégorie générale du mode de pose. La table complète des modes de
/// référence (nécessaire au calcul du courant admissible, cahier §21.3)
/// sera introduite en Phase 8 (moteur de calcul) ; ce niveau de détail
/// suffit pour représenter un câble en Phase 1.
/// </summary>
public enum InstallationMethod
{
    Embedded,
    Surface,
    Conduit,
    CableTray,
    Buried,
    Free,
}
