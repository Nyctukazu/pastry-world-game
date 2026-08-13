using System.Collections.Generic;

namespace PastryWorld.Core.Animation;

public class PartLayer
{
    public string PartName;
    public int SortOrder;
    public bool Visible = true;
    public Dictionary<int, PartKeyframe> Keyframes = new();
}