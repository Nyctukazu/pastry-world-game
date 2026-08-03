using System.Collections.Generic;
using PastryWorld.Core.Enums;

namespace PastryWorld.Core.Level;

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

    public bool Contains(int tileId) => _definitions.ContainsKey(tileId);
    public IEnumerable<TileDefinition> AllTiles => _definitions.Values;
    public void Clear() => _definitions.Clear();
}