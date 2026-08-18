using ImGuiNET;
using PastryWorld.Core.Enums;
using PastryWorld.Core.Animation;
using PastryWorld.Core.Sprite;
using ImVector4 = System.Numerics.Vector4;
using ImVector2 = System.Numerics.Vector2;
using System;
using Microsoft.Xna.Framework;
using PastryWorld.Editor.Commands;
using PastryWorld.Tools;

namespace PastryWorld.Editor.Animation;


public class AnimationEditor
{
    private readonly AnimationManager _animationManager;
    private readonly AnimationSet _animationSet;
    private readonly JsonAnimationSerializer _animSerializer;
    private string _animationSetName;
    private static readonly FacingDirection[] ClockwiseOrder =
    {
        FacingDirection.North, FacingDirection.East, FacingDirection.South, FacingDirection.West
    };
    private AnimationSet _set;
    private FacingDirection _currentDirection = FacingDirection.South;
    private readonly AnimationTimelinePanel _timeline;
    private SpriteManifest _activeManifest;
    private string _newPartName = "";
    public AnimationEditor(AnimationSet animSet, CommandManager command)
    {
        _animSerializer = new JsonAnimationSerializer();
        _timeline = new AnimationTimelinePanel();
        _animationSet = animSet;
        _animationManager = new AnimationManager(animSet, command, _animSerializer, _animationSetName);
    }

    public void SetAnimationSet(AnimationSet set)
    {
        _set = set;
        _currentDirection = FacingDirection.South;
        _timeline.SetClip(_set.GetActiveClip(_currentDirection));
    }

    public void Draw()
    {
        
    }

    public void Update()
    {
        
    }
    
    public void SetSpriteManifest(SpriteManifest manifest) => _activeManifest = manifest;

    private void DrawGui()
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
        DrawFileDropDown();
        ImGui.InputText("##name", ref _set.Name, 64);
        ImGui.SameLine();
        ImGui.Text("Animation Name");

        if (ImGui.Button("Save (Ctrl+S)"))
        {
            _animationManager.SaveCurrentAnimation();
        }

        ImGui.SameLine();
        if (ImGui.Button("+ New"))
        {
            SetAnimationSet(new AnimationSet());
        }

        ImGui.SameLine();
        if (ImGui.Button("Refresh List"))
        {
            _animationManager.RefreshAnimationList();
        }

        if (!string.IsNullOrEmpty(_animationManager.StatusMessage))
        {
            ImVector4 color = _animationManager.IsStatusError
                ? new ImVector4(1f, 0.4f, 0.4f, 1f)
                : new ImVector4(0.4f, 1f, 0.4f, 1f);

            ImGui.TextColored(color, _animationManager.StatusMessage);
        }
    }

    private void DrawFileDropDown()
    {
        string currentAnimationName = _animationManager.AnimationName ?? "";

        if (ImGui.InputText("Animation Name", ref currentAnimationName, 64))
        {
            _animationManager.AnimationName = currentAnimationName;
            _animationSet.Name = currentAnimationName;
        }

        if (_animationManager.AvailableAnimationFiles.Count > 0)
        {
            string currentPreview = _animationManager.SelectedAnimationIndex < _animationManager.AvailableAnimationFiles.Count
                ? _animationManager.AvailableAnimationFiles[_animationManager.SelectedAnimationIndex]
                : "Select Animation...";

            if (ImGui.BeginCombo("Load Existing", currentPreview))
            {
                for (int i = 0; i < _animationManager.AvailableAnimationFiles.Count; i++)
                {
                    bool isSelected = (_animationManager.SelectedAnimationIndex == i);

                    if (ImGui.Selectable(_animationManager.AvailableAnimationFiles[i], isSelected))
                    {
                        _animationManager.SelectedAnimationIndex = i;
                        _animationManager.LoadAnimation(_animationManager.AvailableAnimationFiles[i]);
                    }

                    if (isSelected) ImGui.SetItemDefaultFocus();
                }
                ImGui.EndCombo();
            }
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

        DrawGui();
        EditorStatusBar.Draw();
    }
}