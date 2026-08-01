
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using PastryWorld.Core.Level;
using PastryWorld.Core.Enums;

namespace PastryWorld.Editor.Level;

public class TilePalette
{
    public Dictionary<int, TileDefinition> Definitions { get; } = new();
    public Texture2D SheetTexture { get; private set; }

    public void Load(Texture2D sheet)
    {
        SheetTexture = sheet;
        Definitions.Clear();

        int spacing = 1;
        int tileSize = 16;
        int cols = (sheet.Width + spacing) / (tileSize + spacing);
        int rows = (sheet.Height + spacing) / (tileSize + spacing);

        int currentId = 0;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                int srcX = c * (tileSize + spacing);
                int srcY = r * (tileSize + spacing);

                Definitions[currentId] = new TileDefinition
                {
                    Id = currentId,
                    Name = $"Tile #{currentId}",
                    SourceX = srcX,
                    SourceY = srcY,
                    Width = tileSize,
                    Height = tileSize,
                    Collision = TileCollision.Passable
                };

                currentId++;
            }
        }
    }
}