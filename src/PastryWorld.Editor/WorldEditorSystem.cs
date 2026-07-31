using MonoGame.ImGuiNet;
using Microsoft.Xna.Framework;
using XnaMatrix = Microsoft.Xna.Framework.Matrix;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;
using PastryWorld.Editor.Interfaces;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XnaRectangle = Microsoft.Xna.Framework.Rectangle;


namespace PastryWorld.Editor;

public class WorldEditorSystem : IEditorSystem
{
    private readonly ToolRailPanel _toolRailPanel;
    private readonly ImGuiRenderer _imGuiRenderer;
    private readonly LevelEditor _levelEditor;
    public bool IsActive { get; set; } = false;

    public WorldEditorSystem(ImGuiRenderer imGuiRenderer)
    {
        _imGuiRenderer = imGuiRenderer;
        _toolRailPanel = new ToolRailPanel();
        _levelEditor = new LevelEditor();
    }

    public void ToggleMode() => IsActive = !IsActive;
    public void Update(GameTime gameTime, XnaMatrix cameraMatrix)
    {
        if (!IsActive) return;

        MouseState mouseState = Mouse.GetState();
        XnaVector2 screenPos = new XnaVector2(mouseState.X, mouseState.Y);

        XnaMatrix invertedCamera = XnaMatrix.Invert(cameraMatrix);
        XnaVector2 worldPos = XnaVector2.Transform(screenPos, invertedCamera);

        _toolRailPanel.Update(worldPos);
    }

    public void DrawWorld(SpriteBatch spriteBatch, XnaRectangle viewportBounds, XnaMatrix viewMatrix, Texture2D pixel)
    {
        if (!IsActive) return;

        if (viewMatrix.M11 == 0 && viewMatrix.M22 == 0) return;

        XnaMatrix invertedView = XnaMatrix.Invert(viewMatrix);

        if (float.IsNaN(invertedView.M11)) return; 

        XnaVector2 topLeft = XnaVector2.Transform(new XnaVector2(viewportBounds.Left, viewportBounds.Top), invertedView);
        XnaVector2 bottomRight = XnaVector2.Transform(new XnaVector2(viewportBounds.Right, viewportBounds.Bottom), invertedView);

        XnaRectangle visibleWorldBounds = new XnaRectangle(
            (int)topLeft.X,
            (int)topLeft.Y,
            (int)(bottomRight.X - topLeft.X),
            (int)(bottomRight.Y - topLeft.Y)
        );

        _toolRailPanel.Draw(spriteBatch, visibleWorldBounds, pixel);
    }

    public void DrawUI(GameTime gameTime)
    {
        if (!IsActive) return;
        _imGuiRenderer.BeginLayout(gameTime);
        _toolRailPanel.DrawGui();
        _imGuiRenderer.EndLayout();
    }
}