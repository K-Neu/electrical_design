using ElectricalDesigner.Canvas.Scene;

namespace ElectricalDesigner.Canvas.Commands;

/// <summary>
/// Pattern Command exact du guide §10 : "Command { execute(); undo(); }".
/// Chaque modification significative de la scène (création, suppression,
/// déplacement, rotation, redimensionnement, changement de propriété...)
/// passe par une implémentation de cette interface plutôt que de muter la
/// scène directement, afin de bénéficier de l'historique global (étape G13).
/// </summary>
public interface ICanvasCommand
{
    /// <summary>Description courte destinée à l'affichage (ex. "Déplacer 3 objets").</summary>
    string Description { get; }

    void Execute(CanvasScene scene);

    void Undo(CanvasScene scene);
}
