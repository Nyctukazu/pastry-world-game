
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using XnaRectangle = Microsoft.Xna.Framework.Rectangle;
using PastryWorld.Core.Sprite;
using System.Text.Json;

namespace PastryWorld.Tools;

public class SpriteCutterTool
{
    public Texture2D LoadedTexture { get; private set; }
    public string LoadedFilePath { get; private set; }
    public List<SpriteSlice> Slices { get; } = new();

    public int GridCellWidth = 16;
    public int GridCellHeight = 16;

    private Point? _dragStartTexPx;
    public XnaRectangle? CurrentSelection { get; private set; }

    public void Load(GraphicsDevice device, string pngPath)
    {
        using var stream = File.OpenRead(pngPath);
        LoadedTexture = Texture2D.FromStream(device, stream);
        LoadedFilePath = pngPath;
        Slices.Clear();
        CurrentSelection = null;
    }

    public void AutoSlideGrid()
    {
        if (LoadedTexture == null) return;
        Slices.Clear();

        int cols = LoadedTexture.Width / GridCellWidth;
        int rows = LoadedTexture.Height / GridCellHeight;
        int index = 0;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                Slices.Add(new SpriteSlice
                {
                    Id = $"sprite_{index++}",
                    Rect = new XnaRectangle(x * GridCellWidth, y * GridCellHeight, GridCellWidth, GridCellHeight),
                    PivotX = GridCellWidth / 2,
                    PivotY = GridCellHeight / 2
                });
            }
        }
    }
    
    public void UpdateManualDrag(Point mouseTexPx, bool leftDown, bool leftJustReleased)
    {
        if (leftDown && _dragStartTexPx == null)
        {
            _dragStartTexPx = mouseTexPx;
        }

        if (_dragStartTexPx.HasValue)
        {
            int minX = Math.Min(_dragStartTexPx.Value.X, mouseTexPx.X);
            int minY = Math.Min(_dragStartTexPx.Value.Y, mouseTexPx.Y);
            int w = Math.Abs(mouseTexPx.X - _dragStartTexPx.Value.X);
            int h = Math.Abs(mouseTexPx.Y - _dragStartTexPx.Value.Y);
            CurrentSelection = new XnaRectangle(minX, minY, Math.Max(w, 1), Math.Max(h, 1));
        }

        if (leftJustReleased)
        {
            _dragStartTexPx = null;
        }
    }

    public void CommitSelection(string spriteId)
    {
        if (!CurrentSelection.HasValue) return;

        Slices.Add(new SpriteSlice
        {
            Id = spriteId,
            Rect = CurrentSelection.Value,
            PivotX = CurrentSelection.Value.Width / 2,
            PivotY = CurrentSelection.Value.Height / 2
        });

        CurrentSelection = null;
    }

    public void SaveManifest(string outputJsonPath)
    {
        var manifest = new SpriteManifest
        {
            AtlasFile = Path.GetFileName(LoadedFilePath),
            Sprites = Slices
        };

        string json = JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true });
        Directory.CreateDirectory(Path.GetDirectoryName(outputJsonPath)!);
        File.WriteAllText(outputJsonPath, json);
    }

    public static SpriteManifest LoadManifest(string jsonPath)
    {
        string json = File.ReadAllText(jsonPath);
        return JsonSerializer.Deserialize<SpriteManifest>(json);
    }

}