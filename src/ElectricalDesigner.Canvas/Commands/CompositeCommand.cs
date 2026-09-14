using ElectricalDesigner.Canvas.Scene;

namespace ElectricalDesigner.Canvas.Commands;

/// <summary>
/// Regroupe plusieurs commandes en une seule entrée d'historique (guide §10 :
/// "actions groupables"). Utilisé notamment par le collage (étape G12), où
/// coller N objets doit s'annuler en une seule fois, pas N fois.
/// </summary>
public sealed class CompositeCommand(IReadOnlyList<ICanvasCommand> commands, string description) : ICanvasCommand
{
    private readonly IReadOnlyList<ICanvasCommand> _commands = commands ?? throw new ArgumentNullException(nameof(commands));

    public string Description { get; } = description;

    public void Execute(CanvasScene scene)
    {
        foreach (var command in _commands)
        {
            command.Execute(scene);
        }
    }

    public void Undo(CanvasScene scene)
    {
        // Ordre inverse : la dernière commande exécutée doit être la première annulée.
        for (var i = _commands.Count - 1; i >= 0; i--)
        {
            _commands[i].Undo(scene);
        }
    }
}
