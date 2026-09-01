using System.Collections.Generic;

namespace PastryWorld.Core.Animation;

public class PartLayer
{
    public string PartName { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool Visible = true;
    public Dictionary<int, PartKeyframe> Keyframes { get; private set; } = new();

    public PartLayer Clone()
    {
        var copy = new PartLayer
        {
            PartName = this.PartName,
            SortOrder = this.SortOrder,
            Visible = this.Visible,
        };

        foreach (var kvp in this.Keyframes)
        {
            copy.Keyframes.Add(kvp.Key, kvp.Value.Clone());
        }

        return copy;
    }
}