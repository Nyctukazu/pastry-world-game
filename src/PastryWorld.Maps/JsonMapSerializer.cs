using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using PastryWorld.Core;
using PastryWorld.Maps.Interfaces;

namespace PastryWorld.Maps;

public class JsonMapSerializer : IMapSerializer
{
    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    public void SaveMap(MapData map, string filePath)
    {
        ArgumentNullException.ThrowIfNull(map);
        Console.WriteLine(filePath);
        if (string.IsNullOrEmpty(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
        }

        string? directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
        using FileStream createStream = File.Create(filePath);
        JsonSerializer.Serialize(createStream, map, _options);
    }

    public MapData LoadMap(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Map file not found.", filePath);
        }

        using FileStream openStream = File.OpenRead(filePath);
        MapData? map = JsonSerializer.Deserialize<MapData>(openStream, _options);
        return map ?? throw new InvalidDataException($"Failed to deserialize map file at '{filePath}'.  File may be corrupt or empty.");
    }
}