using System.Collections.Generic;

namespace PastryWorld.Core.Animation;

public class AnimationClip
{
    public const int Fps = 12;
    public string Name { get; set; } = "NewAnimation";
    public int FrameCount { get; set; } = 8;
    public bool Loop = true;
    public List<PartLayer> Layers { get; set; } = new();

    public AnimationClip Clone()
    {
        var copy = new AnimationClip
        {
            Name = this.Name,
            FrameCount = this.FrameCount,
            Loop = this.Loop
        };

        foreach (var layer in this.Layers)
        {
            copy.Layers.Add(layer.Clone());
        }

        return copy;
    }

    public void CopyFrom(AnimationClip other)
    {
        if (other == null) return;

        Name = other.Name;
        FrameCount = other.FrameCount;
        Loop = other.Loop;

        Layers.Clear();
        foreach (var layer in other.Layers)
        {
            Layers.Add(layer.Clone());
        }
    }
}
