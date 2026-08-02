using Microsoft.Xna.Framework.Graphics;
using System;
using System.Text.Json.Serialization;
using static System.Array;

namespace PastryWorld.Core.Level;

public class MapData
{
    public string Name { get; set; } = "Untitled Map";
    public int Width { get; set; } = 50;
    public int Height { get; set;} = 50;
    public int TileSize { get; set; } = 16;

    public bool AutoExpand { get; set; } = false;
    
    public int[] TileGrid { get; set; } = Empty<int>();

    public MapData()
    {
        TileGrid = new int[Width * Height];
    }

    public MapData(int width, int height)
    {
        Width = width;
        Height = height;
        TileGrid = new int[width * height];
    }

    public int GetTile(int x, int y)
    {
        if (TileGrid == null) return 0;
        if (x < 0 || x >= Width || y < 0 || y >= Height) return 0;

        int index = y * Width + x;

        if (index < 0 || index >= TileGrid.Length) return 0;

        return TileGrid[index];
    }

    /// <summary>
    /// Grows the map array if (x, y) falls outside the current grid dimensions.
    /// </summary>
    public void EnsureCapacity(int x, int y, int padding = 10)
    {
        if (x < 0 || y < 0) return;

        if (x >= Width || y >= Height)
        {
            int newWidth = Math.Max(Width, x + 1 + padding);
            int newHeight = Math.Max(Height, y + 1 + padding);

            int [] newGrid = new int[newWidth * newHeight];

            for (int oldY = 0; oldY < Height; oldY++)
            {
                for (int oldX = 0; oldX < Width; oldX++)
                {
                    newGrid[oldY * newWidth + oldX] = TileGrid[oldY * Width + oldX];
                }
            }

            Width = newWidth;
            Height = newHeight;
            TileGrid = newGrid;
        }
    }

    public void SetTile(int x, int y, int tileId)
    {
        if (TileGrid == null) return;
        
        if (x < 0 || x >= Width || y < 0 || y >= Height) return;

        int index = y * Width +  x;

        if (index >= 0 && index < TileGrid.Length)
        {
            TileGrid[index] = tileId;
        }
    }
}