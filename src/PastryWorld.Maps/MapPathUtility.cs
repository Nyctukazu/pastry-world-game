using System.IO;
using System;

namespace PastryWorld.Maps;

public static class MapPathUtility
{
    public const string MapExtension = ".json";

    /// <summary>
    /// Gets the absolute path to the user's maps directory, ensuring it exists.
    /// </summary>
    public static string GetUserMapsDirectory()
    {
    #if DEBUG
        string projectSourceDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Data", "Levels"));
        Directory.CreateDirectory(projectSourceDir);
        return projectSourceDir;
    #else

        string mapsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Levels");
        Directory.CreateDirectory(mapsDir);
        return mapsDir;
    #endif
    }
    /// <summary>
    /// Combines the maps directory with a filename and ensures the correct extension.
    /// </summary>
    public static string GetFullPathForMap(string mapName)
    {
        if (string.IsNullOrWhiteSpace(mapName))
        {
            throw new ArgumentException("Map name cannot be null or empty.", nameof(mapName));
        }

        string fileNameOnly = Path.GetFileNameWithoutExtension(mapName);

        string sanitizedName = string.Join("-", fileNameOnly.Split(Path.GetInvalidFileNameChars()));

            sanitizedName += MapExtension;

        return Path.Combine(GetUserMapsDirectory(), sanitizedName);
    }
    /// <summary>
    /// Checks if a map file exists in the user's maps directory.
    /// </summary>
    public static bool MapExists(string mapName)
    {
        string fullPath = GetFullPathForMap(mapName);
        return File.Exists(fullPath);
    }
}