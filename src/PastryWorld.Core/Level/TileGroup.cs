using System.Collections.Generic;

namespace PastryWorld.Core.Level;

public class TileGroup
{
    public int Id { get; set; }
    public string Name { get; set; } = "New Group";

    public List<TileDefinition> Tiles { get; set; } = new();
}