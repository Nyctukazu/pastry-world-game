using MonoGame.ImGuiNet;
using ImGuiNET;
using Microsoft.Xna.Framework;
using XnaMatrix = Microsoft.Xna.Framework.Matrix;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;
using PastryWorld.Editor.Interfaces;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XnaRectangle = Microsoft.Xna.Framework.Rectangle;
using System.Linq;

using PastryWorld;
using PastryWorld.Editor.Level;
using PastryWorld.Editor.Entity;
using PastryWorld.Editor.Animation;
using PastryWorld.Engine;
using System.Runtime.InteropServices;
using PastryWorld.Editor.Commands;
using PastryWorld.Core.Level;
using System.Collections.Generic;
using PastryWorld.Maps;
using System.IO;
using System;
using System.Reflection.Metadata.Ecma335;


namespace PastryWorld.Editor;

public class WorldEditorSystem : IEditorSystem
{
    private readonly CommandManager _command;
    private readonly MapData _mapData;
    private readonly TileRegistry _registry;
    private readonly ToolRailPanel _toolRailPanel;
    private readonly ImGuiRenderer _imGuiRenderer;
    private readonly LevelEditor _levelEditor;
    private readonly EntityEditor _entityEditor;
    private readonly SmartObjectEditor _objectEditor;
    private readonly AnimationEditor _animationEditor;
    private Texture2D _spritesheetTexture;
    public bool IsActive { get; set; } = false;
    private Camera2D _camera;
    private EditorCamera _editorCamera;
    private TilePalette _palette;
    private readonly List<(TileGroup Group, Texture2D Texture, IntPtr ImGuiId)> _loadedTilesets = new();
    public float CurrentZoom => _editorCamera.Zoom;


    public WorldEditorSystem(ImGuiRenderer imGuiRenderer, Camera2D camera, MapData mapData, TileRegistry registry)
    {
        _imGuiRenderer = imGuiRenderer;
        
        _editorCamera = new EditorCamera();
        _camera = camera;

        _command = new CommandManager();
        _mapData = mapData;
        _registry = registry;
        _palette = new TilePalette(registry);

        _levelEditor = new LevelEditor(_mapData, _command, _registry, _palette);
        _entityEditor = new EntityEditor();
        _objectEditor = new SmartObjectEditor();
        _animationEditor = new AnimationEditor();

        _toolRailPanel = new ToolRailPanel(_levelEditor, _entityEditor, _objectEditor, _animationEditor);
        
        
    }

    public void LoadContent()
    {
    }

    public void RegisterTilesetGroup(TileGroup group, Texture2D texture, IntPtr imGuiTextureId)
    {
        _loadedTilesets.Add((group, texture, imGuiTextureId));

        _palette.AddGroup(group, texture, imGuiTextureId);
    }

    public Texture2D? GetTextureForGroup(int groupId)
    {
        var match = _loadedTilesets.FirstOrDefault(t => t.Group.Id == groupId);
        return match.Texture;
    }

    public void ToggleMode() => IsActive = !IsActive;
    public void Update(GameTime gameTime, XnaRectangle destinationRect, int scale)
    {
        if (!IsActive) return;

        _editorCamera.UpdateInput(destinationRect, scale);
        
        if (ImGui.GetIO().WantCaptureMouse)
        {
            return;
        }

        XnaMatrix combinedViewMatrix = _camera.GetViewMatrix() * _editorCamera.GetViewMatrix();
        XnaMatrix invertedView = XnaMatrix.Invert(combinedViewMatrix);

        MouseState mouseState = Mouse.GetState();
        if (scale <= 0) scale = 1;

        float canvasX = (mouseState.X - destinationRect.X) / (float)scale;
        float canvasY = (mouseState.Y - destinationRect.Y) / (float)scale;
        XnaVector2 canvasMouse = new XnaVector2(canvasX, canvasY);

        XnaVector2 worldPos = XnaVector2.Transform(canvasMouse, invertedView);

        _toolRailPanel.Update(worldPos);
    }

    public void DrawWorld(SpriteBatch spriteBatch, XnaRectangle viewportBounds, XnaMatrix viewMatrix, Texture2D pixel)
    {
        if (!IsActive) return;

        XnaMatrix finalViewMatrix = viewMatrix * _editorCamera.GetViewMatrix();

        if (viewMatrix.M11 == 0 && viewMatrix.M22 == 0) return;

        XnaMatrix invertedView = XnaMatrix.Invert(finalViewMatrix);

        if (float.IsNaN(invertedView.M11)) return; 

        XnaRectangle visibleWorldBounds = _editorCamera.GetVisibleWorldBounds(viewportBounds, invertedView);

        _toolRailPanel.Draw(spriteBatch, visibleWorldBounds, pixel);
    }

    public void DrawUI(GameTime gameTime)
    {
        if (!IsActive) return;
        _imGuiRenderer.BeginLayout(gameTime);
        _toolRailPanel.DrawGui();
        _imGuiRenderer.EndLayout();
    }

    public XnaMatrix GetFinalViewMatrix(XnaMatrix baseViewMatrix)
    {
        return baseViewMatrix * _editorCamera.GetViewMatrix();
    }
}