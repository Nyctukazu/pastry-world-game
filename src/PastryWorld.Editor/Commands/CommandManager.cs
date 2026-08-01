using System.Collections.Generic;
using System.Linq;
using ImGuiNET;
using PastryWorld.Editor.Interfaces;

namespace PastryWorld.Editor.Commands;

public class CommandManager
{
    private readonly LinkedList<ICommand> _undoStack = new();
    private readonly Stack<ICommand> _redoStack = new();
    private readonly int _maxHistory;

    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;

    public string LastUndoName => _undoStack.Last?.Value.Name ?? "";
    public string LastRedoName => _redoStack.Count > 0 ? _redoStack.Peek().Name : "";

    public CommandManager(int maxHistory = 100)
    {
        _maxHistory = maxHistory;
    }

    public void Execute(ICommand command)
    {
        command.Execute();
        _undoStack.AddLast(command);
        _redoStack.Clear();

        if (_undoStack.Count > _maxHistory)
        {
            _undoStack.RemoveFirst();
        }
    }

    public void Undo()
    {
        if (!CanUndo) return;

        var command = _undoStack.Last.Value;
        _undoStack.RemoveLast();

        command.Undo();
        _redoStack.Push(command);
    }

    public void Redo()
    {
        if (!CanRedo) return;

        var command = _redoStack.Pop();
        command.Execute();
        _undoStack.AddLast(command);
    }

    public void ProcessShortcuts()
    {
        var io = ImGui.GetIO();

        if (io.WantTextInput) return;

        bool ctrl = io.KeyCtrl;
        bool shift = io.KeyShift;

        if (ctrl && ImGui.IsKeyPressed(ImGuiKey.Z) && !shift)
        {
            Undo();
        }
        else if ((ctrl && ImGui.IsKeyPressed(ImGuiKey.Y)) || (ctrl && shift && ImGui.IsKeyPressed(ImGuiKey.Z)))
        {
            Redo();
        }
    }
}