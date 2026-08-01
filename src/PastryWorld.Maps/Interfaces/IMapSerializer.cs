
using PastryWorld.Core.Level;

namespace PastryWorld.Maps.Interfaces;

public interface IMapSerializer
{
    void SaveMap(MapData map, string filePath);
    MapData LoadMap(string filePath);
}