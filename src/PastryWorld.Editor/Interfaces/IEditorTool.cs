using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;
using XnaRectangle = Microsoft.Xna.Framework.Rectangle;

namespace PatryWorld.Editor.Interfaces;

public interface IEditorTool
{
    /// <summary>
    /// World-space drawing - tilemap, character composite, grid, cursor, whatever this tool renders on the canvas.  Called inside the camera-transforms SpriteBatch.Begin/End block.
    /// </summary>
    void Update(XnaVector2 worldMouse);

    /// <summary>
    /// World-space drawing - tilemap, character composite, grid, cursor, whatever this tool renders on the canvas.  Called inside the camera-transformed SpriteBatch.Begin/end block.
    /// </summary>
    void DrawWorld(SpriteBatch spritebatch, XnaRectangle visibleWorldBounds, Texture2D pixel);

    /// <summary>
    /// ImGui side-panel content for this tool.  Called inside ImGuiRenderer.BeginLayout/EndLayout.
    /// </summary>
    void DrawGui();

    /// <summary>
    /// Optional: called when switching INTO this tool (e.g. bind a texture, reset a selection).  Default no-op.
    /// </summary>
    void OnActivated() { }

    /// <summary>
    /// Optional: callled when switching AWAY from this tool (e.g. release a drag state).  Default no-op.
    /// </summary>
    void onDeactivated() { }

}