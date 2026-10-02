
namespace PastryWorld.Core.Enums;
/// <summary>
/// 1. Normal: Default blending, no special effects,
/// 2. Additive: fire, magic, muzzle flashes, energy,
/// 3. Multiply: shadows, darkening, poison/dark tint,
/// 4. Screen: glows, auras, softer light effects,
/// 5. Subtract: smoke, ink, drain/curse effects
/// </summary>
public enum BlendMode
{
    Normal,
    Additive,   // fire, magic, muzzle flashes, energy
    Multiply,   // shadows, darkening, poison/dark tint
    Screen,     // glows, auras, softer light effects
    Subtract    // smoke, ink, drain/curse effects
}