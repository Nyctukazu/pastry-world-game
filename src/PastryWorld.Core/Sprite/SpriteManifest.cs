
using System.Collections.Generic;

namespace PastryWorld.Core.Sprite;

public class SpriteManifest
{
    public string AtlasFile { get; set; }
    public List<SpriteSlice> Sprites { get; set; } = new();
}