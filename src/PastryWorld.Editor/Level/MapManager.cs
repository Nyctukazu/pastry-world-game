
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PastryWorld.Core.Level;
using PastryWorld.Editor.Commands;
using PastryWorld.Engine;
using PastryWorld.Maps;

namespace PastryWorld.Editor.Level;

public class MapManager
{
    private readonly MapData _mapData;
    private readonly CommandManager _commandManager;
    private readonly JsonMapSerializer _mapSerializer;
    private string _mapName = "UntitledMap";

    public string MapName 
    {
         get => _mapName; 
         set => _mapName = value ?? ""; 
    }
    public List<string> AvailableMapFiles { get; } = new();
    public int SelectedMapIndex { get; set; } = 0;
    public string StatusMessage { get; private set; } = "";
    public bool IsStatusError { get; private set; } = false;


    public MapManager(MapData mapData, CommandManager commandManager, JsonMapSerializer mapSerializer, string mapName)
    {
        _mapData = mapData;
        _commandManager = commandManager;
        _mapSerializer = mapSerializer;
        MapName = mapName;
    }

    public void RefreshMapList()
    {
        AvailableMapFiles.Clear();
        string mapsDir = MapPathUtility.GetUserMapsDirectory();

        Directory.CreateDirectory(mapsDir);

        var files = Directory.GetFiles(mapsDir, "*.json")
                            .Select(Path.GetFileNameWithoutExtension)
                            .Where(name => !string.IsNullOrWhiteSpace(name))
                            .Select(name => name!)
                            .ToList();

        AvailableMapFiles.AddRange(files);

        if (!string.IsNullOrWhiteSpace(MapName))
        {
            int index = AvailableMapFiles.IndexOf(MapName);
            if (index >= 0)
            {
                SelectedMapIndex = index;
            }
        }
    }

    public void SaveCurrentMap()
    {
        string cleanName = MapName.Trim();
        if (string.IsNullOrWhiteSpace(cleanName))
        {
            SetStatus("Cannot save: Map name is empty!", isError: true);
            return;
        }

        try
        {
            _mapData.Name = cleanName;
            string savePath = MapPathUtility.GetFullPathForMap(cleanName);
            _mapSerializer.SaveMap(_mapData, savePath);

            RefreshMapList();
            SetStatus($"Saved: {cleanName}.json", isError: false);
        }
        catch (Exception ex)
        {
            SetStatus($"Save Fauled: {ex.Message}", isError: true);
        }
    }

    public bool LoadMapByName(string mapName)
    {
        if (string.IsNullOrWhiteSpace(mapName)) return false;

        try
        {
            string loadPath = MapPathUtility.GetFullPathForMap(mapName);
            MapData loadedMap = _mapSerializer.LoadMap(loadPath);

            _mapData.CopyFrom(loadedMap);
            MapName = mapName;
            _commandManager.Clear();

            SetStatus($"Loaded: {mapName}.json", isError: false);
            return true;
        }
        catch (Exception ex)
        {
            SetStatus($"Load Failed: {ex.Message}", isError: true);
            return false;
        }
    }

    public void CreateNewMap(string newName = "NewMap", int width = 50, int height = 50)
    {
        MapName = newName;
        _mapData.CopyFrom(new Core.Level.MapData(width, height) { Name = newName });
        _commandManager.Clear();
        SetStatus($"Created new map: {newName}", isError: false);
    }

    private void SetStatus(string message, bool isError)
    {
        StatusMessage = message;
        IsStatusError = isError;
    }
}