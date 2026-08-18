using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using PastryWorld.Core.Animation;

namespace PastryWorld.Tools;

public class JsonAnimationSerializer
{
    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        IncludeFields = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    public void Save(AnimationSet set, string filePath)
    {
        ArgumentNullException.ThrowIfNull(set);
        Console.WriteLine(filePath);
        if (string.IsNullOrEmpty(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty");
        }

        string? directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
        using FileStream createStream = File.Create(filePath);
        JsonSerializer.Serialize(createStream, set, _options);
    }

    public AnimationSet Load(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be null or emtpy.", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Map file not found.", filePath);
        }

        using FileStream openStream = File.OpenRead(filePath);
        AnimationSet? set = JsonSerializer.Deserialize<AnimationSet>(openStream, _options);
        return set ?? throw new InvalidCastException($"Failed to deserialize animation set file at '{filePath}'.  File may be corrupt or empty.");
    }
}