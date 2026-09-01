using MonoGame.ImGuiNet;
using ImGuiNET;
using Microsoft.Xna.Framework;
using XnaMatrix = Microsoft.Xna.Framework.Matrix;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;
using PastryWorld.Editor.Interfaces;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ImVector2 = System.Numerics.Vector2;
using ImVector4 = System.Numerics.Vector4;
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
using PastryWorld.Core.Animation;


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
    public bool IsAnimationToolActive => _toolRailPanel.IsAnimationToolActive;
    private XnaRectangle _lastDestinationRect;
    private float _lastScale = 1;


    public WorldEditorSystem(ImGuiRenderer imGuiRenderer, 
                            Camera2D camera, 
                            MapData mapData, 
                            TileRegistry registry,
                            AnimationSet animationSet
                            )
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
        _animationEditor = new AnimationEditor(animationSet, _command);
        

        _toolRailPanel = new ToolRailPanel(_levelEditor, _entityEditor, _objectEditor, _animationEditor, _editorCamera);

        _animationEditor.OnCenterViewRequested = CenterViewOnOrigin;
        
    }

    public void LoadContent()
    {
    }

    public void CenterViewOnOrigin()
    {
        float scale = _lastScale <= 0 ? 1 : _lastScale;

        XnaVector2 halfViewportCanvasPixels = new XnaVector2(
            _lastDestinationRect.Width / scale / 2f,
            _lastDestinationRect.Height / scale / 2f
        );

        XnaVector2 baseTransformedOrigin = XnaVector2.Transform(XnaVector2.Zero, _camera.GetViewMatrix());

        XnaVector2 targetPosition = baseTransformedOrigin - (halfViewportCanvasPixels / _editorCamera.Zoom);

        _editorCamera.PanTo(targetPosition, 0.4f);
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
    public void Update(GameTime gameTime, XnaRectangle destinationRect, float scale)
    {
        if (!IsActive) return;

        _lastDestinationRect = destinationRect;
        _lastScale = scale <= 0 ? 1 : scale;

        _editorCamera.UpdateInput(destinationRect, scale);
        _editorCamera.Update(gameTime);
        
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

        _toolRailPanel.Update(gameTime, worldPos);
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

        if (_toolRailPanel.IsAnimationToolActive)
        {
            DrawOriginIndicator();
        }

        _imGuiRenderer.EndLayout();
    }

    private void DrawOriginIndicator()
    {
        XnaMatrix combinedView = _camera.GetViewMatrix() * _editorCamera.GetViewMatrix();
        XnaVector2 canvasOrigin = XnaVector2.Transform(XnaVector2.Zero, combinedView);

        ImVector2 screenOrigin = new ImVector2(
            _lastDestinationRect.X + canvasOrigin.X * _lastScale,
            _lastDestinationRect.Y + canvasOrigin.Y * _lastScale

        );

        bool onScreen = screenOrigin.X >= _lastDestinationRect.Left 
                        && screenOrigin.X <= _lastDestinationRect.Right
                        && screenOrigin.Y >= _lastDestinationRect.Top 
                        && screenOrigin.Y <= _lastDestinationRect.Bottom;

        var drawList = ImGui.GetForegroundDrawList();


        if (onScreen)
        {
            drawList.AddCircle(screenOrigin, 5f, ImGui.GetColorU32(new ImVector4(1f, 1f, 1f, 0.85f)), 12, 1);
            return;
        }

        const float margin = 24f;
        ImVector2 center = new ImVector2(
            (_lastDestinationRect.Left + _lastDestinationRect.Right) / 2f,
            (_lastDestinationRect.Top + _lastDestinationRect.Bottom) / 2f);

        ImVector2 dir = screenOrigin - center;
        if (dir.LengthSquared() < 0.001f) dir = new ImVector2(0, -1);
        dir /= dir.Length();

        float halfW = _lastDestinationRect.Width / 2f - margin;
        float halfH = _lastDestinationRect.Height / 2f - margin;
        float scale = Math.Min(
            MathF.Abs(dir.X) > 0.0001f ? halfW / MathF.Abs(dir.X) : float.MaxValue,
            MathF.Abs(dir.Y) > 0.0001f ? halfH / MathF.Abs(dir.Y) : float.MaxValue
        );

        ImVector2 arrowTip = center + dir * scale;
        float angle = MathF.Atan2(dir.Y, dir.X);

        ImVector2 mousePos = ImGui.GetIO().MousePos;
        bool hovered = ImVector2.Distance(mousePos, arrowTip) <= 14f;

        uint arrowColor = ImGui.GetColorU32(hovered
            ? new ImVector4(1f, 0.95f, 0.5f, 1f)
            : new ImVector4(1f, 0.85f, 0.2f, 1f)
        );

        DrawArrowTriangle(drawList, arrowTip, angle, 10f, arrowColor);

        if (hovered)
        {
            drawList.AddCircle(arrowTip, 14f, ImGui.GetColorU32(new ImVector4(1f, 1f, 1f, 0.35f)), 16, 1.5f);
        }

        if (hovered && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
        {
            CenterViewOnOrigin();
        }
    }

    private void DrawArrowTriangle(ImDrawListPtr drawList, ImVector2 tip, float angle, float size, uint color)
    {
        ImVector2 Rotate(ImVector2 v) => new ImVector2(
            v.X * MathF.Cos(angle) - v.Y * MathF.Sin(angle),
            v.X * MathF.Sin(angle) + v.Y * MathF.Cos(angle)
        );

        ImVector2 p1 = tip;
        ImVector2 p2 = tip + Rotate(new ImVector2(-size, size * 0.6f));
        ImVector2 p3 = tip + Rotate(new ImVector2(-size, -size * 0.6f));
        drawList.AddTriangleFilled(p1, p2, p3, color);
    }

    public XnaMatrix GetFinalViewMatrix(XnaMatrix baseViewMatrix)
    {
        return baseViewMatrix * _editorCamera.GetViewMatrix();
    }

}