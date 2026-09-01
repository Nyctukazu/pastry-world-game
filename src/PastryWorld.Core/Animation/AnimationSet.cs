

using System;
using System.Collections.Generic;
using PastryWorld.Core.Enums;

namespace PastryWorld.Core.Animation;

public class AnimationSet
{
    public string Name = "New Animation Set";
    public AnimationMode Mode { get; set; } = AnimationMode.Directional;

    public List<string> PartNames { get; private set; } = new()
    {
        "Head", "Torso", "LeftArm", "RightArm", "Ears", "Tail", "Weapon", "Accessory", "Expression"
    };

    public Dictionary<FacingDirection, AnimationClip> ClipsByDirection { get; private set; }= new();
    public AnimationClip SingleClip;

    public AnimationSet()
    {
        foreach (FacingDirection dir in Enum.GetValues(typeof(FacingDirection)))
        {
            ClipsByDirection[dir] = NewClipWithRig();
        }

        SingleClip = NewClipWithRig();
    }

    private AnimationClip NewClipWithRig()
    {
        var clip = new AnimationClip { Name = Name };
        foreach (var part in PartNames)
        {
            clip.Layers.Add(new PartLayer { PartName = part, SortOrder = clip.Layers.Count });
        }

        return clip;
    }

    public AnimationClip GetActiveClip(FacingDirection currentDirection) => 
        Mode == AnimationMode.SingleFacing ? SingleClip : ClipsByDirection[currentDirection];

    public void AddPart(string partName)
    {
        if (PartNames.Contains(partName)) return;
        PartNames.Add(partName);

        foreach (var clip in AllClips())
        {
            clip.Layers.Add(new PartLayer { PartName = partName, SortOrder = PartNames.Count - 1});
        }
    }

    public void RemovePart(string partName)
    {
        PartNames.Remove(partName);
        foreach (var clip in AllClips())
        {
            clip.Layers.RemoveAll(l => l.PartName == partName);
        }
    }

    private IEnumerable<AnimationClip> AllClips()
    {
        foreach (var clip in ClipsByDirection.Values)
        {
            yield return clip;
        }
        yield return SingleClip;
    }

    public void CopyFrom(AnimationSet other)
    {
        if (other == null) return;

        Name = other.Name;
        Mode = other.Mode;
        PartNames = new List<string>(other.PartNames);
        ClipsByDirection = new Dictionary<FacingDirection, AnimationClip>();
        foreach (var kvp in other.ClipsByDirection)
        {
            ClipsByDirection[kvp.Key] = kvp.Value.Clone();
        }
        SingleClip = other.SingleClip.Clone();
    }
}