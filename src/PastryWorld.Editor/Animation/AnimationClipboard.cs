using PastryWorld.Core.Animation;
using ImVector4 = System.Numerics.Vector4;
 
namespace PastryWorld.Editor.Animation;
 
/// <summary>
/// Holds a deep copy of a layer (settings + every keyframe across all frames)
/// so it can be pasted elsewhere. Also used by Duplicate (Ctrl+D), which
/// clones directly without touching the clipboard contents.
/// </summary>
public static class AnimationClipboard
{
    private static PartLayer _copied;
 
    public static bool HasContent => _copied != null;
 
    public static void Copy(PartLayer layer)
    {
        _copied = Clone(layer);
    }
 
    /// <summary>Returns a fresh clone of whatever is currently on the clipboard, or null.</summary>
    public static PartLayer PasteAsNew()
    {
        return _copied != null ? Clone(_copied) : null;
    }
 
    /// <summary>Deep-clones a layer: name, color label, sort order, and every keyframe.</summary>
    public static PartLayer Clone(PartLayer src)
    {
        var clone = new PartLayer
        {
            PartName = src.PartName,
            Visible = src.Visible,
            SortOrder = src.SortOrder,
            ColorLabel = src.ColorLabel
        };
 
        foreach (var kv in src.Keyframes)
        {
            var k = kv.Value;
            clone.Keyframes[kv.Key] = new PartKeyframe
            {
                SpriteId = k.SpriteId,
                X = k.X,
                Y = k.Y,
                Rotation = k.Rotation,
                FlipX = k.FlipX,
                FlipY = k.FlipY,
                Opacity = k.Opacity,
                Blend = k.Blend,
                ChannelR = k.ChannelR,
                ChannelG = k.ChannelG,
                ChannelB = k.ChannelB,
                ChannelA = k.ChannelA,
                ScaleX = k.ScaleX,
                ScaleY = k.ScaleY,
                Z_Index = k.Z_Index
            };
        }
 
        return clone;
    }
}
 
