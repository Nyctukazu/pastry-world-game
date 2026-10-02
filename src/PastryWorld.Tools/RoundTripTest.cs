using System;
using System.IO;
using PastryWorld.Core.Animation;
using PastryWorld.Core.Enums;

namespace PastryWorld.Tools;

public static class RoundTripTest
{
    public static void Run()
    {
        var serializer = new JsonAnimationSerializer();

        var original = new AnimationSet { Name = "RoundTripTest"};
        var layer = original.ClipsByDirection[FacingDirection.North].Layers[0];
        layer.Keyframes[0] = new PartKeyframe { SpriteId = "head_01", X = 5, Y = 3, Rotation = 45f };
        layer.Keyframes[4] = new PartKeyframe { SpriteId = "head_02", X = 8, Y = 1, FlipX = true };

        serializer.Save(original, "a.json");
        var loaded = serializer.Load("a.json");
        serializer.Save(loaded, "b.json");

        bool identical = File.ReadAllText("a.json") == File.ReadAllText("b.json");
        Console.WriteLine(identical ? "Round trip OK" : "DATA MISMATCH");

        var loadedLayer = loaded.ClipsByDirection[FacingDirection.North].Layers[0];
        Console.WriteLine($"North Layers: {loaded.ClipsByDirection[FacingDirection.North].Layers.Count}");
        Console.WriteLine($"Keyframes on layer 0: {loadedLayer.Keyframes.Count} (expected 2)");
    }
}