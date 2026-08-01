using ImGuiNET;
using PastryWorld.Editor.Commands;

namespace PastryWorld.Editor;
public class EditorToolbar
{
    private readonly CommandManager _commandManager;

    public EditorToolbar(CommandManager commandManager)
    {
        _commandManager = commandManager;
    }

    public void DrawToolbarGui()
    {
        _commandManager.ProcessShortcuts();

        if (!_commandManager.CanUndo) ImGui.BeginDisabled();
        
        if (ImGui.Button("Undo (Ctrl+Z)"))
        {
            _commandManager.Undo();
        }
        if (_commandManager.CanUndo && ImGui.IsItemHovered())
        {
            ImGui.SetTooltip($"Undo: {_commandManager.LastUndoName}");
        }
        
        if (!_commandManager.CanUndo) ImGui.EndDisabled();

        ImGui.SameLine();

        if (!_commandManager.CanRedo) ImGui.BeginDisabled();
        
        if (ImGui.Button("Redo (Ctrl+Y)"))
        {
            _commandManager.Redo();
        }
        if (_commandManager.CanRedo && ImGui.IsItemHovered())
        {
            ImGui.SetTooltip($"Redo: {_commandManager.LastRedoName}");
        }
        
        if (!_commandManager.CanRedo) ImGui.EndDisabled();
    }
}