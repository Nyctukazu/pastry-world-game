
using System;
using System.Collections.Generic;
using System.IO;

namespace PastryWorld.Tools;

public class TilesetConfigDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string TextureFileName { get; set; } = "";
    public int TileSize { get; set; } = 16;
    public int Padding { get; set; } = 1;
    public List<TileOverrideDto> Tiles { get; set; } = new();

}