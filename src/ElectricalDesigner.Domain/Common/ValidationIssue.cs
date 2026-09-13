namespace ElectricalDesigner.Domain.Common;

/// <summary>
/// Signale un problème de validation structurelle du modèle électrique
/// (ex. "référence de câble manquante", "protection introuvable").
///
/// À NE PAS confondre avec <c>RuleFinding</c> (guide §Phase 7, cahier §20) :
/// ce dernier viendra du moteur de règles RGIE en Phase 7 et porte une sévérité
/// réglementaire (INFO/WARNING/ERROR/BLOCKING) avec une référence légale.
/// <see cref="ValidationIssue"/> ne concerne que la cohérence interne du modèle
/// (invariants), indépendamment de toute réglementation.
/// </summary>
/// <param name="Path">Chemin logique de l'objet concerné (ex. "Board[TGBT].Circuit[B]").</param>
/// <param name="Message">Message explicite, en français, destiné à un utilisateur du logiciel.</param>
public sealed record ValidationIssue(string Path, string Message)
{
    public override string ToString() => $"{Path}: {Message}";
}
