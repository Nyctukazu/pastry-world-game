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
    private readonly AnimationRigPanel _rigPanel;
    private readonly AnimationFilePanel _filePanel;
    private readonly SpritePalettePanel _palettePanel;
    private readonly AnimationCompositeRenderer _compositeRenderer;
    private string _animationSetName;
    private static readonly FacingDirection[] ClockwiseOrder =
    {
        FacingDirection.North, FacingDirection.East, FacingDirection.South, FacingDirection.West
    };
    private AnimationSet _set;
    private FacingDirection _currentDirection = FacingDirection.South;
    private readonly AnimationTimelinePanel _timeline;
    private SpriteManifest _activeManifest;
    private Texture2D _spriteSheetTexture;
    private XnaVector2 _characterOrigin = XnaVector2.Zero;
    private MouseState _prevMouse;
    private Color _canvasBackgroundColor = new Color(30, 30, 34);
    public Action OnCenterViewRequested;
    public AnimationSet AnimationSet => _animationSet;
    public FacingDirection CurrentDirection => _currentDirection;
    public AnimationTimelinePanel Timeline => _timeline;
    public SpriteManifest ActiveManifest => _activeManifest;
    public Texture2D SpriteSheetTexture => _spriteSheetTexture;
    public Color CanvasBackgroundColor =>_canvasBackgroundColor;
    public AnimationSet Set => _set;

    public AnimationEditor(AnimationSet animSet, CommandManager command)
    {
        _animSerializer = new JsonAnimationSerializer();
        _timeline = new AnimationTimelinePanel();
        _animationSet = animSet;
        _animationManager = new AnimationManager(animSet, command, _animSerializer, _animationSetName);
        SetAnimationSet(animSet);
        _rigPanel = new AnimationRigPanel(ClockwiseOrder, this);
        _filePanel = new AnimationFilePanel(_animationManager, this);
        _palettePanel = new SpritePalettePanel(this);
        _compositeRenderer = new AnimationCompositeRenderer(this);


        _animationManager.RefreshAnimationList();
    }

    public void Draw(SpriteBatch spriteBatch, XnaRectangle visibleWorldBounds, Texture2D pixel)
    {
        _compositeRenderer.DrawWorld(spriteBatch, visibleWorldBounds, pixel);
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
        _filePanel.Update();
        _prevMouse = mouse;

        var io = ImGui.GetIO();

        if (!io.WantTextInput && io.KeyCtrl && ImGui.IsKeyPressed(ImGuiKey.S))
        {
            _animationManager.SaveCurrentAnimation();
        }
    }

    public void SetDirection(FacingDirection dir)
    {
        _currentDirection = dir;
        _timeline.SetClip(_set.GetActiveClip(dir));
    }
    public void SetMode(AnimationMode mode)
    {
        _set.Mode = mode;
        _timeline.SetClip(_set.GetActiveClip(_currentDirection));
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

        _filePanel.DrawSetHeader();
        ImGui.Separator();
        _rigPanel.DrawModeToggle();
        ImGui.Separator();

        if (_set.Mode == AnimationMode.Directional)
        {
            _rigPanel.DrawDirectionRotator();
        }
        else
        {
            _rigPanel.DrawSingleFacingNotice();
        }

        ImGui.Separator();
        _rigPanel.DrawRigList();
        ImGui.Separator();
        _palettePanel.DrawSpritePalette();
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

    public void DrawAnimationOptions()
    {

        DrawGui();
        EditorStatusBar.Draw();
    }

    public void ResetToNewAnimation()
    {
        _set.CopyFrom(new AnimationSet());
        _currentDirection = FacingDirection.South;
        _timeline.SetClip(_set.GetActiveClip(_currentDirection));
    }
}