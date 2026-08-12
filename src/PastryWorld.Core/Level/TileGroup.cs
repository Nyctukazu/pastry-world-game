using System.Collections.Generic;

namespace PastryWorld.Core.Level;

public class TileGroup
{
    public int Id { get; set; }
    public string Name { get; set; } = "New Group";
    public string TextureFileName { get; set; } = "";

    public List<TileDefinition> Tiles { get; set; } = new();
}