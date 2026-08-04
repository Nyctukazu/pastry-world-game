using System;
using System.Collections.Generic;
using PastryWorld.Core.Level;
using PastryWorld.Editor.Interfaces;

namespace PastryWorld.Editor.Commands;

public class ResizeMapCommand : ICommand
{
    private readonly MapData _mapData;
    private readonly int _oldWidth, _oldHeight;
    private readonly List<int> _oldGrid;
    public string Name => "Auto-Expand Map";

    public ResizeMapCommand(MapData mapData, int oldWidth, int oldHeight, List<int> oldGrid)
    {
        _mapData = mapData;
        _oldWidth = oldWidth;
        _oldHeight = oldHeight;
        _oldGrid = [..oldGrid]; 

    }

    public void Execute()
    {
    }

    public void Undo()
    {
        _mapData.RestoreState(_oldWidth, _oldHeight, _oldGrid);
    }
}