namespace PastryWorld.Core.Enums;
/// <summary>
/// 1. Passable: No collision, can walk through,
/// 2. Solid: Cannot walk through, blocks movement,
/// 3. Water: Can walk through, but slows movement,
/// 4. Hazard: Can walk through, but causes damage or negative effects
/// </summary>
public enum TileCollision
{
    Passable,
    Solid,
    Water,
    Hazard
}