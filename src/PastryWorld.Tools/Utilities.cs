
using Microsoft.Xna.Framework.Graphics;
using XnaRectangle = Microsoft.Xna.Framework.Rectangle;


namespace PastryWorld.Tools;
public static class Utilities
{
    public static XnaRectangle GetCenteredLetterboxRect(XnaRectangle nativeBounds, int scale, Viewport viewport)
    {
        int scaledWidth = nativeBounds.Width * scale;
        int scaledHeight = nativeBounds.Height * scale;

        int x = (viewport.Width - scaledWidth) / 2;
        int y = (viewport.Height - scaledHeight) / 2;

        return new XnaRectangle(x, y, scaledWidth, scaledHeight);
    }
}