using Microsoft.Xna.Framework.Graphics;
using PastryWorld.Core.Enums;

namespace PastryWorld.Tools;
public static class BlendModeResolver
{
    public static readonly BlendState Multiply = new BlendState
    {
        ColorSourceBlend = Blend.DestinationColor,
        ColorDestinationBlend = Blend.Zero,
        AlphaSourceBlend = Blend.DestinationAlpha,
        AlphaDestinationBlend = Blend.Zero
    };

    public static BlendState Resolve(BlendMode mode) => mode switch
    {
        BlendMode.Normal => BlendState.AlphaBlend,
        BlendMode.Additive => BlendState.Additive,
        BlendMode.Multiply => Multiply,
        BlendMode.Screen => BlendState.AlphaBlend,
        BlendMode.Subtract => BlendState.AlphaBlend,
        _ => BlendState.AlphaBlend
    };
}