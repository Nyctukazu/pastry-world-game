using System.IO;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using PastryWorld.Core.Level;
using System.Text.RegularExpressions;

namespace PastryWorld.Tools;

public class TilesetLoader
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly string _jsonDirectory;
    private readonly string _contentDirectory;

    public TilesetLoader(GraphicsDevice graphicsDevice, string dataSubPath = "Data/TileGroup", string contentSubPath = "Content/Tilesets")
    {
        _graphicsDevice = graphicsDevice;
        _jsonDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dataSubPath);
        _contentDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, contentSubPath);
    }

    public List<(TileGroup Group, Texture2D Texture)> LoadAllTilesets(TileRegistry registry)
    {
        var loadedGroups = new List<(TileGroup, Texture2D)>();

        if (!Directory.Exists(_jsonDirectory))
        {
            Console.WriteLine($"[TilesetLoader ERROR] Directory does not exist: {_jsonDirectory}");
            Directory.CreateDirectory(_jsonDirectory);
            return loadedGroups;
        }

        string[] jsonFiles = Directory.GetFiles(_jsonDirectory, "*.json");
        Console.WriteLine($"[TilesetLoader] Found {jsonFiles.Length} JSON file(s) in {_jsonDirectory}");
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        foreach (string jsonPath in jsonFiles)
        {
            Console.WriteLine($"[TilesetLoader] Reading file: {Path.GetFileName(jsonPath)}");
            string jsonContent = File.ReadAllText(jsonPath);
            var config = JsonSerializer.Deserialize<TilesetConfigDto>(jsonContent, jsonOptions);

            if (config == null || string.IsNullOrWhiteSpace(config.TextureFileName))
            {
                Console.WriteLine($"[TilesetLoader ERROR] Failed to deserialize {jsonPath}");
                continue;
            }
                

            string texturePath = Path.Combine(_contentDirectory, config.TextureFileName);
            if (!File.Exists(texturePath))
            {
                throw new FileNotFoundException($"Tileset texture file not found: {texturePath}");
            }

            using var stream = File.OpenRead(texturePath);
            Texture2D texture = Texture2D.FromStream(_graphicsDevice, stream);

            var group = new TileGroup
            {
                Id = config.Id,
                Name = config.Name,
                TextureFileName = config.TextureFileName
            };
            
            int stride = config.TileSize + (config.Padding * 2);
            int cols = texture.Width / stride;
            int rows = texture.Height / stride;

            var overrides = new Dictionary<int, TileOverrideDto>();
            foreach (var tileOverride in config.Tiles)
            {
                overrides[tileOverride.Id] = tileOverride;
            }

            int currentId = config.Id * 1000;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    int tileId = currentId++;

                    int srcX = (c * stride) + config.Padding;
                    int srcY = (r * stride) + config.Padding;

                    var tileDef = new TileDefinition
                    {
                        Id = tileId,
                        GroupId = group.Id,
                        Name = $"Tile #{tileId}",
                        SourceX = srcX,
                        SourceY = srcY,
                        Width = config.TileSize,
                        Height = config.TileSize
                    };
                    if (overrides.TryGetValue(tileId, out var customData))
                    {
                        if (!string.IsNullOrEmpty(customData.Name)) tileDef.Name = customData.Name;
                    }

                    group.Tiles.Add(tileDef);
                    registry.RegisterTile(tileDef);
                }
            }

            loadedGroups.Add((group, texture));
            Console.WriteLine($"[TilesetLoader SUCCESS] Loaded group '{group.Name}' (ID: {group.Id}) with {group.Tiles.Count} tiles.");
        }

        return loadedGroups;
    }
}