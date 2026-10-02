using System.Collections.Generic;
using ImVector4 = System.Numerics.Vector4;
namespace PastryWorld.Core.Animation;

public class PartLayer
{
    public string PartName { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool Visible = true;
    public Dictionary<int, PartKeyframe> Keyframes { get; set; } = new();
    public ImVector4 ColorLabel = new ImVector4(1f, 1f, 1f, 1f);

    public PartLayer Clone()
    {
        var copy = new PartLayer
        {
            PartName = this.PartName,
            SortOrder = this.SortOrder,
            Visible = this.Visible,
            ColorLabel = this.ColorLabel
        };

        foreach (var kvp in this.Keyframes)
        {
            copy.Keyframes.Add(kvp.Key, kvp.Value.Clone());
        }

        return copy;
    }
}