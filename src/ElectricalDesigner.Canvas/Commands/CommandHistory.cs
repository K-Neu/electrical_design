namespace ElectricalDesigner.Canvas.Commands;

/// <summary>
/// Historique global d'annulation/rétablissement (guide §6 étape G13, §10).
/// Toute exécution via <see cref="Execute"/> vide la pile de rétablissement —
/// comportement standard d'un historique linéaire : on ne peut pas "redo"
/// une branche qui vient d'être remplacée par une nouvelle action.
/// </summary>
public sealed class CommandHistory
{
    private readonly Stack<ICanvasCommand> _undoStack = new();
    private readonly Stack<ICanvasCommand> _redoStack = new();

    public bool CanUndo => _undoStack.Count > 0;

    public bool CanRedo => _redoStack.Count > 0;

    public int UndoCount => _undoStack.Count;

    public int RedoCount => _redoStack.Count;

    public void Execute(ICanvasCommand command, Scene.CanvasScene scene)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(scene);

        command.Execute(scene);
        _undoStack.Push(command);
        _redoStack.Clear();
    }

    public void Undo(Scene.CanvasScene scene)
    {
        if (!CanUndo)
        {
            throw new InvalidOperationException("Aucune action à annuler.");
        }

        var command = _undoStack.Pop();
        command.Undo(scene);
        _redoStack.Push(command);
    }

    public void Redo(Scene.CanvasScene scene)
    {
        if (!CanRedo)
        {
            throw new InvalidOperationException("Aucune action à rétablir.");
        }

        var command = _redoStack.Pop();
        command.Execute(scene);
        _undoStack.Push(command);
    }

    public void Clear()
    {
        _undoStack.Clear();
        _redoStack.Clear();
    }
}
