namespace PastryWorld.Core;

public class MapData
{
    public int Width { get; private set; }
    public int Height { get; private set;}
    public int TileSize { get; private set; } = 16;
    
    private int[,] _tiles;

    public MapData(int width = 50, int height = 50)
    {
        Width = width;
        Height = height;
        _tiles = new int[width, height];
    }

    public int GetTile(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height) return 0;
        return _tiles[x, y];
    }

    public void SetTile(int x, int y, int tileId)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height)
        {
            _tiles[x, y] = tileId;
        }
    }
}