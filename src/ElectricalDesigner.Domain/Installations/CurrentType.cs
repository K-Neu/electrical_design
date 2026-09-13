namespace ElectricalDesigner.Domain.Installations;

/// <summary>
/// Cahier §21.2 : "Le moteur doit distinguer explicitement DC / AC monophasé / AC
/// triphasé. Ne jamais déduire silencieusement le nombre de phases à partir d'un
/// texte libre." Cette énumération existe précisément pour empêcher cela : toute
/// donnée électrique du modèle doit porter un <see cref="CurrentType"/> explicite.
/// </summary>
public enum CurrentType
{
    Dc,
    AcSinglePhase,
    AcThreePhase,
}
