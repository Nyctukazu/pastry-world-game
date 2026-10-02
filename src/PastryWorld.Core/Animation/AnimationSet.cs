

using System;
using System.Collections.Generic;
using PastryWorld.Core.Enums;

namespace PastryWorld.Core.Animation;

public class AnimationSet
{
    public string Name = "New Animation Set";
    public AnimationMode Mode { get; set; } = AnimationMode.Directional;

    private static readonly string[] DefaultParts = 
    {
        "Head", "Torso", "LeftArm", "RightArm", "Ears", "Tail", "Weapon", "Accessory"
    };

    public Dictionary<FacingDirection, AnimationClip> ClipsByDirection { get; set; }= new();
    public AnimationClip SingleClip;

    public AnimationSet()
    {
        foreach (FacingDirection dir in Enum.GetValues(typeof(FacingDirection)))
        {
            ClipsByDirection[dir] = NewClipWithRig();
        }

        SingleClip = NewClipWithRig();
    }
    /// <summary>
    /// Creates a new AnimationClip with the current PartNames as layers.
    /// </summary>
    /// <returns></returns>
    private AnimationClip NewClipWithRig()
    {
        var clip = new AnimationClip { Name = Name };
        foreach (var part in DefaultParts)
        {
            clip.Layers.Add(new PartLayer { PartName = part, SortOrder = clip.Layers.Count });
        }

        return clip;
    }

    /// <summary>
    /// Identifies the active AnimationClip
    /// </summary>
    /// <param name="currentDirection">The direction the character is facing</param>
    /// <returns>The active AnimationClip</returns>
    public AnimationClip GetActiveClip(FacingDirection currentDirection) => 
        Mode == AnimationMode.SingleFacing ? SingleClip : ClipsByDirection[currentDirection];


    /// <summary>
    /// Returns all AnimationClips in the set, including directional clips and the single facing clip.
    /// </summary>
    /// <returns>The collection of AnimationClips</returns>
    private IEnumerable<AnimationClip> AllClips()
    {
        foreach (var clip in ClipsByDirection.Values)
        {
            yield return clip;
        }
        yield return SingleClip;
    }

    /// <summary>
    /// Copies the properties and clips from another AnimationSet into this one.
    /// </summary>
    /// <param name="other">The other AnimationSet to copy from</param>
    public void CopyFrom(AnimationSet other)
    {
        if (other == null) return;

        Name = other.Name;
        Mode = other.Mode;
        
        foreach (FacingDirection dir in Enum.GetValues(typeof(FacingDirection)))
        {
            if (!ClipsByDirection.TryGetValue(dir, out var clip))
            {
                clip = new AnimationClip();
                ClipsByDirection[dir] = clip;
            }

            if (other.ClipsByDirection.TryGetValue(dir, out var source))
            {
                clip.CopyFrom(source);
            }
            else
            {
                clip.CopyFrom(NewClipWithRig());
            }
        }

        SingleClip.CopyFrom(other.SingleClip);
    }
}