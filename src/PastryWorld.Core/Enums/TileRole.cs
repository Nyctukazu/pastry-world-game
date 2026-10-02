namespace PastryWorld.Core.Enums;
/// <summary>
/// 1. None: No specific role,
/// 2. TopLeft: Top-left corner tile,
/// 3. Top: Top edge tile,
/// 4. TopRight: Top-right corner tile,
/// 5. Left: Left edge tile,
/// 6. Center: Center tile,
/// 7. Right: Right edge tile,
/// 8. BottomLeft: Bottom-left corner tile,
/// 9. Bottom: Bottom edge tile,
/// 10. BottomRight: Bottom-right corner tile,
/// 11. InnerTopLeft: Inner top-left corner tile,
/// 12. InnerTopRight: Inner top-right corner tile,
/// 13. InnerBottomLeft: Inner bottom-left corner tile,
/// 14. InnerBottomRight: Inner bottom-right corner tile
/// </summary>
public enum TileRole
{
    None,
    
    TopLeft, Top, TopRight,
    Left, Center, Right,
    BottomLeft, Bottom, BottomRight,

    InnerTopLeft, InnerTopRight,
    InnerBottomLeft, InnerBottomRight
}