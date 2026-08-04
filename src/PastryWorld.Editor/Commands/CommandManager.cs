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

    private float _shortcutTimer = 0f;
    private const float RepeatDelay = 0.25f;

    private int _lastProcessedFrame = -1;

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

        int currentFrame = ImGui.GetFrameCount();
        if (_lastProcessedFrame == currentFrame) return;
        _lastProcessedFrame = currentFrame;

        if (_shortcutTimer > 0)
        {
            _shortcutTimer -= io.DeltaTime;
            return;
        }

        bool ctrl = io.KeyCtrl;
        bool shift = io.KeyShift;

        bool isZ = ImGui.IsKeyPressed(ImGuiKey.Z, false) || ImGui.IsKeyDown(ImGuiKey.Z);
        bool isY = ImGui.IsKeyPressed(ImGuiKey.Y, false) || ImGui.IsKeyDown(ImGuiKey.Y);

        bool triggerUndo = ctrl && !shift && isZ;
        bool triggerRedo = (ctrl && !shift && isY) || (ctrl && shift && isZ);

        if (triggerUndo)
        {
            Undo();
            _shortcutTimer = RepeatDelay;
        }
        else if (triggerRedo)
        {
            Redo();
            _shortcutTimer = RepeatDelay;
        }
    }

    /// <summary>
    /// Clears all undo and redo history. 
    /// Call this when loading or initializing a new map.
    /// </summary>
    public void Clear()
    {
        _undoStack.Clear();
        _redoStack.Clear();
    }
}