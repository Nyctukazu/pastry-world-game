using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XnaRectangle = Microsoft.Xna.Framework.Rectangle;
using XnaMatrix = Microsoft.Xna.Framework.Matrix;

namespace PastryWorld.Editor.Interfaces;
public interface IEditorSystem
{
    bool IsActive { get; set; }
    void ToggleMode();
    void Update(GameTime gameTime, Matrix cameraMatrix);
    void DrawWorld(SpriteBatch spriteBatch, XnaRectangle bounds, XnaMatrix viewMatrix, Texture2D pixel);
    void DrawUI(GameTime gameTime);
}