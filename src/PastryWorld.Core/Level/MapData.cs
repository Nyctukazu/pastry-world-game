using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using static System.Array;

namespace PastryWorld.Core.Level;

public class MapData
{
    public string Name { get; set; } = "Untitled Map";
    public int Width { get; set; } = 50;
    public int Height { get; set;} = 50;
    public int TileSize { get; set; } = 16;

    public bool AutoExpand { get; set; } = false;
    
    public List<int> TileGrid { get; set; } = new();

    public MapData() : this(50, 50) { }


    public MapData(int width, int height)
    {
        Width = width;
        Height = height;
        InitializeGrid();
    }

    /// <summary>
    /// Fills the map grid with -1 (representing empty/air tiles).
    /// </summary>
    public void InitializeGrid()
    {
        TileGrid = new List<int>(Width * Height);
        for (int i = 0; i < Width * Height; i++)
        {
            TileGrid.Add(-1);
        }
    }

    public int GetTile(int x, int y)
    {
        if (TileGrid == null) return -1;
        if (x < 0 || x >= Width || y < 0 || y >= Height) return -1;

        int index = y * Width + x;

        if (index < 0 || index >= TileGrid.Count) return -1;

        return TileGrid[index];
    }

    public void SetTile(int x, int y, int tileId)
    {
        if (AutoExpand)
        {
            EnsureCapacity(x, y);
        }

        if (TileGrid == null || x < 0 || x >= Width || y < 0 || y >= Height)
            return;

        int index = y * Width + x;

        if (index >= 0 && index < TileGrid.Count)
        {
            TileGrid[index] = tileId;
        }
    }

    /// <summary>
    /// Grows the map grid if (x, y) falls outside the current grid dimensions.
    /// </summary>
    public void EnsureCapacity(int x, int y, int padding = 10)
    {
        if (x < 0 || y < 0) return;

        if (x >= Width || y >= Height)
        {
            int newWidth = Math.Max(Width, x + 1 + padding);
            int newHeight = Math.Max(Height, y + 1 + padding);

            List<int> newGrid = new List<int>(newWidth * newHeight);

            // Populate new expanded list with empty tiles (-1)
            for (int i = 0; i < newWidth * newHeight; i++)
            {
                newGrid.Add(-1);
            }

            // Copy old tile data to the new expanded grid
            for (int oldY = 0; oldY < Height; oldY++)
            {
                for (int oldX = 0; oldX < Width; oldX++)
                {
                    int oldIndex = oldY * Width + oldX;
                    int newIndex = oldY * newWidth + oldX;

                    if (oldIndex < TileGrid.Count)
                    {
                        newGrid[newIndex] = TileGrid[oldIndex];
                    }
                }
            }

            Width = newWidth;
            Height = newHeight;
            TileGrid = newGrid;
        }
    }
}