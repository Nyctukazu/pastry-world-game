using ImVector4 = System.Numerics.Vector4;
using ImVector2 = System.Numerics.Vector2;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using PastryWorld.Core.Level;
using PastryWorld.Core.Enums;
using System.Linq;
using ImGuiNET;
using System;
using System.Drawing;


namespace PastryWorld.Editor.Level;

public class TilePalette
{
    private readonly TileRegistry _registry;
    public List<TileGroup> Groups { get; } = new();
    public Texture2D SheetTexture { get; private set; }
    public IntPtr ImGuiTextureId { get; private set; }
    public int selectedTileIndex {get; set; } = 0;
    private int _selectedGroupIndex = 0;

    public TilePalette(TileRegistry registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }
    /// <summary>
    /// Slices a spritesheet, organizes tiles into TileGroups, and registers them into the master TileRegistry.
    /// </summary>
    public void Load(Texture2D sheet, IntPtr imGuiTextureId, int tileSize = 16, int spacing = 1, int margin = 1)
    {
        SheetTexture = sheet;
        ImGuiTextureId = imGuiTextureId;

        Groups.Clear();
        _registry.Clear();

        var defaultGroup = new TileGroup
        {
            Id = 0,
            Name = "General Terrain"
        };

        Groups.Add(defaultGroup);

        int cols = (sheet.Width) / (tileSize + spacing);
        int rows = (sheet.Height) / (tileSize + spacing);

        int currentId = 0;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                int srcX = margin + c * (tileSize + spacing);
                int srcY = margin + r * (tileSize + spacing);

                var tileDef = new TileDefinition
                {
                    Id = currentId,
                    GroupId = defaultGroup.Id,
                    Name = $"Tile #{currentId}",
                    SourceX = srcX,
                    SourceY = srcY,
                    Width = tileSize,
                    Height = tileSize,
                    Collision = TileCollision.Passable,
                    Role = TileRole.None
                };

                defaultGroup.Tiles.Add(tileDef);

                _registry.RegisterTile(tileDef);

                currentId++;
            }
        }
    }
    /// <summary>
    /// Inspects a region of pixel data to check if it contains no visible graphics (100% transparent).
    /// </summary>
    private static bool IsTileEmpty(Color[] pixels, int sheetWidth, int srcX, int srcY, int width, int height)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int pixelIndex = (srcY + y) * sheetWidth + (srcX + x);

                if (pixels[pixelIndex].A > 0)
                {
                    return false;
                }
            }
        }
        return true;
    }

    public TileGroup GetGroup(int index)
    {
        if (index >= 0 && index < Groups.Count) return Groups[index];
        return null;
    }

    public void DrawTilePaletteGui()
    {
        ImGui.Text("Tileset Palette");
        ImGui.Separator();

        if (Groups.Count == 0 || SheetTexture == null)
        {
            ImGui.TextDisabled("Palette content not loaded.");
            return;
        }



        string[] groupNames = Groups.Select(g => g.Name).ToArray();
        if (ImGui.Combo("Group", ref _selectedGroupIndex, groupNames, groupNames.Length))
        {
            // Reset tile selection or keep current selection when switching groups
        }

        ImGui.Separator();

        TileGroup activeGroup = Groups[_selectedGroupIndex];
        if (activeGroup.Tiles.Count == 0)
        {
            ImGui.TextDisabled("Group contain no tiles.");
            return;
        }

        if (ImGui.BeginChild("TilePaletteScrollArea", new ImVector2(0, 0), ImGuiChildFlags.Borders))
    {
        float itemSize = 32f;
        float padding = 6f;

        // Get max right boundary inside the child container
        float maxRightX = ImGui.GetWindowPos().X + ImGui.GetContentRegionAvail().X;

        float sheetW = SheetTexture?.Width ?? 1f;
        float sheetH = SheetTexture?.Height ?? 1f;

        for (int i = 0; i < activeGroup.Tiles.Count; i++)
        {
            var tile = activeGroup.Tiles[i];
            bool isSelected = (selectedTileIndex == tile.Id);

            if (isSelected)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, new ImVector4(0.2f, 0.6f, 1.0f, 0.8f));
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new ImVector4(0.3f, 0.7f, 1.0f, 1.0f));
            }

            ImGui.PushID(tile.Id);

            ImVector2 uv0 = new ImVector2(tile.SourceX / sheetW, tile.SourceY / sheetH);
            ImVector2 uv1 = new ImVector2((tile.SourceX + tile.Width) / sheetW, (tile.SourceY + tile.Height) / sheetH);

            bool clicked = ImGui.ImageButton($"tile_{tile.Id}", ImGuiTextureId, new ImVector2(itemSize, itemSize), uv0, uv1);

            if (clicked)
            {
                selectedTileIndex = tile.Id;
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip($"{tile.Name} (ID: #{tile.Id})\nCollision: {tile.Collision}\nRole: {tile.Role}");
            }

            if (isSelected)
            {
                ImGui.PopStyleColor(2);
            }

            float lastButtonRight = ImGui.GetItemRectMax().X;
            float nextButtonRight = lastButtonRight + padding + itemSize;

            // Wrap to next line if the next button exceeds the scroll region width
            if (i + 1 < activeGroup.Tiles.Count && nextButtonRight < maxRightX)
            {
                ImGui.SameLine(0, padding);
            }

            ImGui.PopID();
        }

        ImGui.EndChild(); // End Scroll Region
        }
    }
}