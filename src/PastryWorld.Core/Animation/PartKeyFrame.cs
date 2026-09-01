using System;
using PastryWorld.Core.Enums;

namespace PastryWorld.Core.Animation;

public class PartKeyframe
{
    public string SpriteId { get; set; } = string.Empty;
    public int X { get; set; }
    public int Y { get; set; }
    public float Rotation { get; set; }
    public bool FlipX { get; set; }
    public bool FlipY { get; set; }
    public int Z_Index { get; set; }
    public int HoldFrames { get; set; } = 1;
    public bool Visible { get; set; } = true;
    public float ScaleX { get; set; } = 1f;
    public float ScaleY { get; set; } = 1f;
    public byte Opacity { get; set; } = 255;
    public string? TintColor { get; set; }
    public float TintStrength { get; set; }
    public BlendMode Blend { get; set; } = BlendMode.Normal;
    public int PaletteIndex { get; set; } = 0;

    /// <summary>
    /// Creates a complete copy of this keyframe configuration.
    /// </summary>
    public PartKeyframe Clone()
    {
        return (PartKeyframe)this.MemberwiseClone();
    }
}
