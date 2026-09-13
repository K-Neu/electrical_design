namespace ElectricalDesigner.Domain.Symbols;

/// <summary>
/// Catégories de la bibliothèque de symboles (cahier §11.1). La bibliothèque
/// elle-même (icônes, points de connexion, rendu) sera construite en Phase 3 ;
/// cette énumération existe dès la Phase 1 pour que
/// <see cref="ElectricalSymbolDefinition"/> puisse être catégorisée.
/// </summary>
public enum SymbolCategory
{
    General,
    Boards,
    Supply,
    Protections,
    ResidualCurrentDevices,
    Switches,
    Sockets,
    Lighting,
    FixedAppliances,
    Heating,
    Motors,
    Sources,
    Photovoltaic,
    Batteries,
    ChargingStations,
    HomeAutomation,
    Telecommunication,
    Security,
    Annotations,
    Custom,
}
