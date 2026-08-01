namespace PastryWorld.Editor.Interfaces;

public interface ICommand
{
    /// <summary>
    /// User-friendly name shown in tooltips or history panels
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Perform or re-applies the action
    /// </summary>
    void Execute();

    /// <summary>
    /// Reverts the action.
    /// </summary>
    void Undo();
}