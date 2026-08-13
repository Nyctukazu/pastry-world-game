using ImGuiNET;
using PastryWorld.Core.Enums;
using PastryWorld.Core.Animation;
using PastryWorld.Core.Sprite;
using ImVector4 = System.Numerics.Vector4;
using ImVector2 = System.Numerics.Vector2;
using System;
using Microsoft.Xna.Framework;

namespace PastryWorld.Editor.Animation;


public class AnimationEditor
{
  
    private static readonly FacingDirection[] ClockwiseOrder =
    {
        FacingDirection.North, FacingDirection.East, FacingDirection.South, FacingDirection.West
    };
    private AnimationSet _set;
    private FacingDirection _currentDirection = FacingDirection.South;
    private readonly AnimationTimelinePanel _timeline;
    private SpriteManifest _activeManifest;
    private string _newPartName = "";
    public AnimationEditor()
    {
        _timeline = new AnimationTimelinePanel();
    }

    public void SetAnimationSet(AnimationSet set)
    {
        _set = set;
        _currentDirection = FacingDirection.South;
        _timeline.SetClip(_set.GetActiveClip(_currentDirection));
    }
    
    public void SetSpriteManifest(SpriteManifest manifest) => _activeManifest = manifest;

    private void Draw()
    {
        if (_set == null)
        {
            ImGui.Text("No animation loaded.");
            if (ImGui.Button("+ New Animation"))
            {
                SetAnimationSet(new AnimationSet());
            }
            return;
        }

        DrawSetHeader();
        ImGui.Separator();
        DrawModeToggle();
        ImGui.Separator();

        if (_set.Mode == AnimationMode.Directional)
        {
            DrawDirectionRotator();
        }
        else
        {
            DrawSingleFacingNotice();
        }

        ImGui.Separator();
        DrawRigList();
        ImGui.Separator();
        DrawSpritePalette();
    }

    private void DrawModeToggle()
    {
        bool directional = _set.Mode == AnimationMode.Directional;
        if (ImGui.RadioButton("Directional (4-way)", directional))
        {
            SetMode(AnimationMode.Directional);
        }
        ImGui.SameLine();
        if (ImGui.RadioButton("Single Facing", !directional))
        {
            SetMode(AnimationMode.SingleFacing);
        }
    }

    private void SetMode(AnimationMode mode)
    {
        _set.Mode = mode;
        _timeline.SetClip(_set.GetActiveClip(_currentDirection));
    }

    private void DrawSingleFacingNotice()
    {
        ImGui.TextDisabled("Plays the same regardless of the character's actual facing.");
        ImGui.TextDisabled("Editing the one clip below.");
    }

    private void DrawSetHeader()
    {
        ImGui.InputText("##name", ref _set.Name, 64);
        ImGui.SameLine();
        ImGui.Text("Animation Name");

        if (ImGui.Button("Save"))
        {
            
        }

        ImGui.SameLine();
        if (ImGui.Button("Load Existing"))
        {
            
        }

        ImGui.SameLine();
        if (ImGui.Button("+ New"))
        {
            SetAnimationSet(new AnimationSet());
        }
    }

    private void DrawDirectionRotator()
    {
        ImGui.Text("Facing:");
        ImGui.SameLine();
        ImGui.TextColored(new ImVector4(0.9f, 0.7f, 0.2f, 1f), _currentDirection.ToString());

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

    private void DirectionButton(FacingDirection dir, string label)
    {
        bool active = _currentDirection == dir;
        if (active)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, new ImVector4(0.25f, 0.55f, 0.9f, 1f));
        }
        
        if (ImGui.Button(label, new ImVector2(28, 24)))
        {
            SetDirection(dir);
        }

        if (active)
        {
            ImGui.PopStyleColor();
        }
    }

    private void Rotate(int step)
    {
        int idx = Array.IndexOf(ClockwiseOrder, _currentDirection);
        idx = (idx + step + ClockwiseOrder.Length) % ClockwiseOrder.Length;
        SetDirection(ClockwiseOrder[idx]);
    }

    private void SetDirection(FacingDirection dir)
    {
        _currentDirection = dir;
        _timeline.SetClip(_set.GetActiveClip(dir));
    }

    private void DrawRigList()
    {
        ImGui.Text("Rig Parts- (shared across all 4 directions)");
        foreach (var part in _set.PartNames)
        {
            ImGui.BulletText(part);
        }

        ImGui.SetNextItemWidth(140);
        ImGui.InputText("##newpart", ref _newPartName, 32);
        ImGui.SameLine();
        if (ImGui.Button("+ Add Part") && !string.IsNullOrWhiteSpace(_newPartName))
        {
            _set.AddPart(_newPartName);
            _newPartName = "";
        }
    }

    private void DrawSpritePalette()
    {
        ImGui.Text("Sprite Palette");
        if (_activeManifest == null)
        {
            ImGui.TextDisabled("No sprite sheet loaded - use the Sprite Cutter tool.");
            return;
        }

        foreach (var slice in _activeManifest.Sprites)
        {
            ImGui.Button(slice.Id, new ImVector2(64, 20));

        }
    }

    public void DrawAnimationOptions()
    {

        Draw();
        EditorStatusBar.Draw();
    }
}