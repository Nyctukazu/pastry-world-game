using System.IO;
using System.Text.Json;
using PastryWorld.Core;
using PastryWorld.Maps.Interfaces;

namespace PastryWorld.Maps;

public class JsonMapSerializer : IMapSerializer
{
    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true
    };

    public void SaveMap(MapData map, string filePath)
    {
        string? directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string json = JsonSerializer.Serialize(map, _options);
        File.WriteAllText(filePath, json);
    }

    public MapData LoadMap(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Map file not found at {filePath}");
        }

        string json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<MapData>(json, _options) ?? new MapData();
    }
}