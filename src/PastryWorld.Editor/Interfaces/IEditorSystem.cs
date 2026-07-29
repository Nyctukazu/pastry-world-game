using Microsoft.Xna.Framework;

namespace PastryWorld.Editor.Interfaces;
public interface IEditorSystem
{
    bool IsActive { get; set; }
    void ToggleMode();
    void Update(GameTime gameTime, Matrix cameraMatrix);
    void DrawUI(GameTime gameTime);
}