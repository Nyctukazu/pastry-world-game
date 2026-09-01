using ImGuiNET;
using PastryWorld.Core.Enums;
using PastryWorld.Core.Animation;
using PastryWorld.Core.Sprite;
using ImVector4 = System.Numerics.Vector4;
using ImVector2 = System.Numerics.Vector2;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;
using XnaRectangle = Microsoft.Xna.Framework.Rectangle;
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
    private Texture2D _spriteSheetTexture;
    private XnaVector2 _characterOrigin = XnaVector2.Zero;
    private MouseState _prevMouse;
    private Color _canvasBackgroundColor = new Color(30, 30, 34);
    private readonly Color _axisColor = new Color(255, 255, 255, 90);
    public Action OnCenterViewRequested;


    public AnimationEditor(AnimationSet animSet, CommandManager command)
    {
        _animSerializer = new JsonAnimationSerializer();
        _timeline = new AnimationTimelinePanel();
        _animationSet = animSet;
        _animationManager = new AnimationManager(animSet, command, _animSerializer, _animationSetName);
        SetAnimationSet(animSet);
        _animationManager.RefreshAnimationList();
    }

    public void SetAnimationSet(AnimationSet set)
    {
        _set = set;
        _currentDirection = FacingDirection.South;
        _timeline.SetClip(_set.GetActiveClip(_currentDirection));
    }
    
    public void SetSpriteManifest(SpriteManifest manifest) => _activeManifest = manifest;

    public void SetSpriteTexture(Texture2D texture) => _spriteSheetTexture = texture;

    public void UpdateWorld(GameTime gameTime, XnaVector2 worldMouse)
    {
        if (_set == null) return;

        _timeline.Tick(gameTime);

        MouseState mouse = Mouse.GetState();
        bool leftJustPressed = mouse.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released;

        if (leftJustPressed && _timeline.SelectedLayer != null)
        {
            var clip = _set.GetActiveClip(_currentDirection);
            var layer = clip.Layers.Find(l => l.PartName == _timeline.SelectedLayer);

            if (layer != null && layer.Keyframes.TryGetValue(_timeline.SelectedFrame, out var kf))
            {
                kf.X = (int)(worldMouse.X - _characterOrigin.X);
                kf.Y = (int)(worldMouse.Y - _characterOrigin.Y);
            }
        }

        _prevMouse = mouse;
    }

    public void DrawWorld(SpriteBatch spriteBatch, XnaRectangle visibleWorldBounds, Texture2D pixel)
    {
        if (_set == null) return;

        spriteBatch.Draw(pixel, visibleWorldBounds, _canvasBackgroundColor);
        DrawAxes(spriteBatch, visibleWorldBounds, pixel);

        if (_spriteSheetTexture == null) return;

        var clip = _set.GetActiveClip(_currentDirection);
        int currentFrame = _timeline.CurrentFrame;

        if (_timeline.OnionSkinEnabled)
        {
            for (int i = 1; i <= _timeline.OnionFramesBefore; i++)
            {
                DrawComposite(spriteBatch, clip, currentFrame - i, new Color(2, 55, 100, 100) * 0.25f);

            }

            for (int i = 1; i <= _timeline.OnionFramesAfter; i++)
            {
                DrawComposite(spriteBatch, clip, currentFrame + i, new Color(100, 180, 255) * 0.25f);
            }
        }

        DrawComposite(spriteBatch, clip, currentFrame, Color.White);
    }

    private void DrawAxes(SpriteBatch spriteBatch, XnaRectangle visibleWorldBounds, Texture2D pixel)
    {
        spriteBatch.Draw(pixel, new XnaRectangle(0, visibleWorldBounds.Top, 1, visibleWorldBounds.Height), _axisColor);
        spriteBatch.Draw(pixel, new XnaRectangle(visibleWorldBounds.Left, 0, visibleWorldBounds.Width, 1), _axisColor);
    }

    private void DrawComposite(SpriteBatch spriteBatch, AnimationClip clip, int frame, Color tint)
    {
        if (frame < 0 || frame >= clip.FrameCount) return;

        var layers = new List <PartLayer>(clip.Layers);
        layers.Sort((a, b) => a.SortOrder.CompareTo(b.SortOrder));

        foreach (var layer in layers)
        {
            if (!layer.Visible) continue;

            var kf = GetHeldKeyframe(layer, frame);
            if (kf?.SpriteId == null) continue;

            var slice = _activeManifest?.Sprites.Find(s => s.Id == kf.SpriteId);

            if (slice == null) continue;

            var effects = SpriteEffects.None; 
            if (kf.FlipX) effects |= SpriteEffects.FlipHorizontally;
            if (kf.FlipY) effects |= SpriteEffects.FlipVertically;

            var origin = new XnaVector2(slice.PivotX, slice.PivotY);
            var position = _characterOrigin + new XnaVector2(kf.X, kf.Y);

            spriteBatch.Draw(_spriteSheetTexture, position, slice.Rect, tint, kf.Rotation, origin, 1f, effects, 0f);
        }
    }

    private static PartKeyframe GetHeldKeyframe(PartLayer layer, int frame)
    {
        for (int f = frame; f >= 0; f--)
        {
            if (layer.Keyframes.TryGetValue(f, out var kf))
            {
                return kf;
            }
        }
        return null;
    }
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
        ImGui.Separator();
        DrawCanvasSettings();
    }

    private void DrawCanvasSettings()
    {
        ImGui.Text("Canvas");
        
        ImVector4 colorEdit = new ImVector4(
            _canvasBackgroundColor.R / 255f,
            _canvasBackgroundColor.G / 255f,
            _canvasBackgroundColor.B / 255f, 
            1f 
        );

        if (ImGui.ColorEdit4("Background", ref colorEdit))
        {
            _canvasBackgroundColor = new Color(colorEdit.X, colorEdit.Y, colorEdit.Z);
        }

        if (ImGui.Button("Center View (0, 0)"))
        {
            OnCenterViewRequested?.Invoke();
        }
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