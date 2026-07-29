using System.IO;
using System;

namespace PastryWorld.Maps;

public static class MapPathUtility
{
    public static string GetUserMapsDirectory()
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string mapsDir = Path.Combine(appData, "PastryWorld", "Maps");

        if (!Directory.Exists(mapsDir))
        {
            Directory.CreateDirectory(mapsDir);
        }

        return mapsDir;
    }
}