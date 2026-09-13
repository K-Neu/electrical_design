namespace ElectricalDesigner.Domain.Installations;

/// <summary>
/// Le RGIE distingue fortement les installations domestiques des installations
/// non-domestiques (cahier §2.2, §10) : le système de repérage, le contenu
/// documentaire minimal et certaines règles de validation en dépendent.
/// </summary>
public enum InstallationCategory
{
    Domestic,
    NonDomestic,
}
