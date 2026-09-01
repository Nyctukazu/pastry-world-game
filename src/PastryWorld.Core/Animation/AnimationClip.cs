using System.Collections.Generic;

namespace PastryWorld.Core.Animation;

public class AnimationClip
{
    public const int Fps = 12;
    public string Name { get; set; } = "NewAnimation";
    public int FrameCount { get; set; } = 8;
    public bool Loop = true;
    public List<PartLayer> Layers { get; private set; } = new();

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
}
