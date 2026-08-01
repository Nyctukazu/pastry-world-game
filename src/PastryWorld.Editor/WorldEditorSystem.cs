using MonoGame.ImGuiNet;
using Microsoft.Xna.Framework;
using XnaMatrix = Microsoft.Xna.Framework.Matrix;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;
using PastryWorld.Editor.Interfaces;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XnaRectangle = Microsoft.Xna.Framework.Rectangle;

using PastryWorld;


namespace PastryWorld.Editor;

public class WorldEditorSystem : IEditorSystem
{
    private readonly ToolRailPanel _toolRailPanel;
    private readonly ImGuiRenderer _imGuiRenderer;
    private readonly EditorCamera _editorCamera;
    public bool IsActive { get; set; } = false;
    public float CurrentZoom => _editorCamera.Zoom;

    public WorldEditorSystem(ImGuiRenderer imGuiRenderer)
    {
        _imGuiRenderer = imGuiRenderer;
        _toolRailPanel = new ToolRailPanel();
        _editorCamera = new EditorCamera();
    }

    public void ToggleMode() => IsActive = !IsActive;
    public void Update(GameTime gameTime, XnaMatrix cameraMatrix)
    {
        if (!IsActive) return;

        _editorCamera.UpdateInput();

        XnaMatrix finalCameraMatrix = cameraMatrix * _editorCamera.GetViewMatrix();

        MouseState mouseState = Mouse.GetState();
        XnaVector2 screenPos = new XnaVector2(mouseState.X, mouseState.Y);

        XnaMatrix invertedCamera = XnaMatrix.Invert(finalCameraMatrix);
        XnaVector2 worldPos = XnaVector2.Transform(screenPos, invertedCamera);

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