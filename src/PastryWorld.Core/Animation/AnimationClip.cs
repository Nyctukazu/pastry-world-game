using System.Collections.Generic;

namespace PastryWorld.Core.Animation;

public class AnimationClip
{
    public const int Fps = 12;
    public string Name = "NewAnimation";
    public int FrameCount = 8;
    public bool Loop = true;
    public List<PartLayer> Layers = new();
}