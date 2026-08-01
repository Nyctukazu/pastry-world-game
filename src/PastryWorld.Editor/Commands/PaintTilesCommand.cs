using System.Collections.Generic;
using PastryWorld.Core.Level;
using PastryWorld.Editor.Interfaces;

namespace PastryWorld.Editor.Commands;



public class PaintTilesCommand : ICommand
{
    public string Name => "Paint Tiles";

    private readonly MapData _mapData;
    private readonly List<TileChange> _changes;

    public PaintTilesCommand(MapData mapData, List<TileChange> changes)
    {
        _mapData = mapData;
        _changes = changes;
    }

    public void Execute()
    {
        foreach (var change in _changes)
        {
            _mapData.SetTile(change.X, change.Y, change.NewTileId);
        }
    }

    public void Undo()
    {
        foreach (var change in _changes)
        {
            _mapData.SetTile(change.X, change.Y, change.OldTileId);
        }
    }
}