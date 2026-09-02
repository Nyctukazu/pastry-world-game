using ImGuiNET;
using PastryWorld.Core.Enums;
using PastryWorld.Core.Animation;
using ImVector4 = System.Numerics.Vector4;
using ImVector2 = System.Numerics.Vector2;
using System;


namespace PastryWorld.Editor.Animation;

public class AnimationRigPanel
{

    private readonly FacingDirection[] ClockwiseOrder;
    private string _newPartName = "";
    private AnimationEditor _editor;


    public AnimationRigPanel(FacingDirection[] Clockwise, 
                            AnimationEditor editor)
    {
        ClockwiseOrder = Clockwise;
        _editor = editor;
    }
    public void DrawModeToggle()
    {
        bool directional = _editor.Set.Mode == AnimationMode.Directional;
        if (ImGui.RadioButton("Directional (4-way)", directional))
        {
            _editor.SetMode(AnimationMode.Directional);
        }
        ImGui.SameLine();
        if (ImGui.RadioButton("Single Facing", !directional))
        {
            _editor.SetMode(AnimationMode.SingleFacing);
        }
    }



    public void DrawDirectionRotator()
    {
        ImGui.Text("Facing:");
        ImGui.SameLine();
        ImGui.TextColored(new ImVector4(0.9f, 0.7f, 0.2f, 1f), _editor.CurrentDirection.ToString());

        if (ImGui.ArrowButton("##ccw", ImGuiDir.Left))
        {
            Rotate(-1);
        }
        
        ImGui.SameLine();
        if (ImGui.ArrowButton("##cw", ImGuiDir.Right))
        {
            Rotate(1);
        }

        ImGui.SameLine();
        ImGui.Text("  (rotates through N -> E -> S -> W)");

        DirectionButton(FacingDirection.North, "N");
        ImGui.SameLine();
        DirectionButton(FacingDirection.East, "E");
        ImGui.SameLine();
        DirectionButton(FacingDirection.South, "S");
        ImGui.SameLine();
        DirectionButton(FacingDirection.West, "W");
    }

    public void DirectionButton(FacingDirection dir, string label)
    {
        bool active = _editor.CurrentDirection == dir;
        if (active)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, new ImVector4(0.25f, 0.55f, 0.9f, 1f));
        }
        
        if (ImGui.Button(label, new ImVector2(28, 24)))
        {
            _editor.SetDirection(dir);
        }

        if (active)
        {
            ImGui.PopStyleColor();
        }
    }

    private void Rotate(int step)
    {
        int idx = Array.IndexOf(ClockwiseOrder, _editor.CurrentDirection);
        idx = (idx + step + ClockwiseOrder.Length) % ClockwiseOrder.Length;
        _editor.SetDirection(ClockwiseOrder[idx]);
    }

    
    public void DrawSingleFacingNotice()
    {
        ImGui.TextDisabled("Plays the same regardless of the character's actual facing.");
        ImGui.TextDisabled("Editing the one clip below.");
    }
    public void DrawRigList()
    {
        ImGui.Text("Rig Parts- (shared across all 4 directions)");
        foreach (var part in _editor.Set.PartNames)
        {
            ImGui.BulletText(part);
        }

        ImGui.SetNextItemWidth(140);
        ImGui.InputText("##newpart", ref _newPartName, 32);
        ImGui.SameLine();
        if (ImGui.Button("+ Add Part") && !string.IsNullOrWhiteSpace(_newPartName))
        {
            _editor.Set.AddPart(_newPartName);
            _newPartName = "";
        }
    }
}