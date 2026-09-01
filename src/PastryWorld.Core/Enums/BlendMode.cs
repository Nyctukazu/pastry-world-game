
namespace PastryWorld.Core.Enums;

public enum BlendMode
{
    Normal,
    Additive,   // fire, magic, muzzle flashes, energy
    Multiply,   // shadows, darkening, poison/dark tint
    Screen,     // glows, auras, softer light effects
    Subtract    // smoke, ink, drain/curse effects
}