using Microsoft.Xna.Framework.Graphics;
using System.Text.Json.Serialization;
using static System.Array;

namespace PastryWorld.Core.Level;

public class MapData
{
    public string Name { get; set; } = "Untitled Map";
    public int Width { get; set; } = 50;
    public int Height { get; set;} = 50;
    public int TileSize { get; set; } = 16;
    
    public int[] TileGrid { get; set; } = Empty<int>();

    public MapData() { }

    public MapData(int width, int height)
    {
        Width = width;
        Height = height;
        TileGrid = new int[width * height];
    }

    public int GetTile(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height) return 0;
        return TileGrid[y * Width + x];
    }

    public void SetTile(int x, int y, int tileId)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height)
        {
            TileGrid[y * Width + x] = tileId;
        }
    }
}