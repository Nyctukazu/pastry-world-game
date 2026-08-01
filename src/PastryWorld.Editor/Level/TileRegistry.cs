using System.Collections.Generic;
using PastryWorld.Core.Level;
using PastryWorld.Core.Enums;

namespace PastryWorld.Editor.Level;

public class TileRegistry
{
    private readonly Dictionary<int, TileDefinition> _definitions = new();

    public void RegisterTile(TileDefinition def)
    {
        _definitions[def.Id] = def;
    }

    public TileDefinition Get(int tileId)
    {
        if (_definitions.TryGetValue(tileId, out var def))
        {
            return def;
        }
        return new TileDefinition
        {
            Id = tileId,
            Name = "Unknown",
            Collision = TileCollision.Passable
        };
    }
}