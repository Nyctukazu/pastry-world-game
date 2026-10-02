namespace PastryWorld.Core.Enums;
/// <summary>
/// 1. None: No tile, empty space,
/// 2. Grass: Grass tile, walkable,
/// 3. Dirt: Dirt tile, walkable,
/// 4. Stone: Stone tile, walkable,
/// 5. Water: Water tile, swimmable, slows movement,
/// 6. Sand: Sand tile, walkable
/// </summary>
public enum TileType
{
    None = 0,
    Grass,
    Dirt,
    Stone,
    Water,
    Sand,
}