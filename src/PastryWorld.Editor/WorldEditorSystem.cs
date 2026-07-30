using MonoGame.ImGuiNet;
using Microsoft.Xna.Framework;
using XnaMatrix = Microsoft.Xna.Framework.Matrix;
using PastryWorld.Editor.Interfaces;

namespace PastryWorld.Editor;

public class WorldEditorSystem : IEditorSystem
{
    private readonly ToolRailPanel _toolRailPanel;
    private readonly ImGuiRenderer _imGuiRenderer;
    public bool IsActive { get; set; } = false;

    public WorldEditorSystem(ImGuiRenderer imGuiRenderer)
    {
        _imGuiRenderer = imGuiRenderer;
        _toolRailPanel = new ToolRailPanel();
    }

    public void ToggleMode() => IsActive = !IsActive;
    public void Update(GameTime gameTime, XnaMatrix cameraMatrix)
    {
        if (!IsActive) return;
    }

    public void DrawUI(GameTime gameTime)
    {
        if (!IsActive) return;
        _imGuiRenderer.BeginLayout(gameTime);
        _toolRailPanel.Draw();
        _imGuiRenderer.EndLayout();
    }
}