using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PastryWorld.Core.Level;
namespace PastryWorld.Maps;

public class TileMapRenderer
{
    /// <summary>
    /// Renders the MapData grid to the screen using a shared TileRegistry and Texture2D sheet.
    /// Works in both the gameplay engine and the level editor.
    /// </summary>
    public void Draw(
        SpriteBatch spriteBatch,
        MapData mapData,
        TileRegistry registry,
        Texture2D sheetTexture)
    {
        if (mapData == null || registry == null || sheetTexture == null || mapData.TileGrid == null)
            return;

        int tileSize = mapData.TileSize;

        for (int y = 0; y < mapData.Height; y++)
        {
            for (int x = 0; x < mapData.Width; x++)
            {
                int index = y * mapData.Width + x;

                if (index < 0 || index >= mapData.TileGrid.Count)
                    continue;

                int tileId = mapData.TileGrid[index];

                if (tileId <= -1) continue;

                TileDefinition def = registry.Get(tileId);
                if (def == null) continue;

                Rectangle destRect = new Rectangle(
                    x * tileSize,
                    y * tileSize,
                    tileSize,
                    tileSize
                );

                spriteBatch.Draw(
                    sheetTexture,
                    destRect,
                    def.SourceRectangle,
                    Color.White
                );
            }
        }
    }
}