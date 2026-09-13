namespace ElectricalDesigner.Domain.Circuits;

/// <summary>
/// Réserve la place, dans <see cref="Circuit.CalculationData"/>, pour les résultats
/// du moteur de calcul électrique (guide §12.2, Phase 8) : valeur, unité, statut,
/// formule utilisée, entrées, hypothèses, avertissements, version du moteur.
///
/// Intentionnellement vide en Phase 1 : aucun calcul n'existe encore. On garde
/// néanmoins ce type distinct (plutôt qu'un <c>object?</c> non typé) afin que
/// l'attribut <c>calculationData</c> exigé par le guide §5.2 existe bien dans le
/// modèle dès maintenant, avec un point d'extension propre pour la Phase 8.
/// </summary>
public sealed record CircuitCalculationSnapshot;
