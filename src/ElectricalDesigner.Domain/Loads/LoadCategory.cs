namespace ElectricalDesigner.Domain.Loads;

/// <summary>Sous-ensemble des catégories de la bibliothèque de symboles (cahier §11.1) pertinent pour une charge.</summary>
public enum LoadCategory
{
    Lighting,
    Socket,
    FixedAppliance,
    Heating,
    Motor,
    Hvac,
    HomeAutomation,
    Other,
}
