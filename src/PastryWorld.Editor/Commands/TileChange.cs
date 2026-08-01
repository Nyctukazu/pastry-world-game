
namespace PastryWorld.Editor.Commands;
public readonly struct TileChange
{
    public readonly int X;
    public readonly int Y;
    public readonly int OldTileId;
    public readonly int NewTileId;

    public TileChange(int x, int y, int oldTileId, int newTileId)
    {
        X = x;
        Y = y;
        OldTileId = oldTileId;
        NewTileId = newTileId;
    }
}