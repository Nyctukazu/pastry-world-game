using System.Diagnostics;
using Microsoft.Xna.Framework;
using PastryWorld.Core.Enums;

namespace PastryWorld.Core.Level;

public class TileDefinition
{
    public int Id { get; set; }
    public string Name { get; set; } = "New Tile";
    public TileType Type { get; set; } = TileType.Grass;
    public TileCollision Collision {get; set; } = TileCollision.Passable;

    public float SpeedMultiplier { get; set; } = 1.0f;
    public Rectangle SourceRectangle { get; set; }
    public int SourceX { get; set; }
    public int SourceY { get; set; }
    public int Width { get; set; } = 16;
    public int Height { get; set; } = 16;
}