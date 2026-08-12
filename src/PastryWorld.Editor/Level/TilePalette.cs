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
    public int selectedTileId {get; set; } = -1;
    private int _selectedGroupIndex = 0;
    private readonly List<(TileGroup group, Texture2D Texture, IntPtr TextureId)> _availableGroups = new();
    public TileDefinition? SelectedTile { get; private set; }
    public int SelectedTileId { get; private set; }

    public TilePalette(TileRegistry registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public void DrawTilePaletteGui()
    {
        ImGui.Text("Tileset Palette");
        ImGui.Separator();


        if (_availableGroups.Count == 0)
        {
            ImGui.TextDisabled("Palette content not loaded.");
            return;
        }



        string[] groupNames = _availableGroups.Select(g => g.group.Name).ToArray();
        Console.WriteLine($"[TilePalette] Drawing combo with {groupNames.Length} items: {string.Join(", ", groupNames)}");
        if (ImGui.Combo("Tileset", ref _selectedGroupIndex, groupNames, groupNames.Length))
        {
            var newGroup = _availableGroups[_selectedGroupIndex].group;
            if (newGroup.Tiles.Count > 0)
            {
                SelectTile(newGroup.Tiles[0]);
            }
        }

        ImGui.Separator();

        var (activeGroup, activeTexture, activeTextureId) = _availableGroups[_selectedGroupIndex];


        if (activeGroup.Tiles.Count == 0)
        {
            ImGui.TextDisabled("Group contain no tiles.");
            return;
        }

        if (ImGui.BeginChild("TilePaletteScrollArea", new ImVector2(0, 0), ImGuiChildFlags.Borders))
    {
        float itemSize = 32f;
        float padding = 6f;

        float maxRightX = ImGui.GetWindowPos().X + ImGui.GetContentRegionAvail().X;

        float sheetW = activeTexture.Width;
        float sheetH = activeTexture.Height;

        for (int i = 0; i < activeGroup.Tiles.Count; i++)
        {
            var tile = activeGroup.Tiles[i];
            bool isSelected = SelectedTileId == tile.Id;

            if (isSelected)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, new ImVector4(0.2f, 0.6f, 1.0f, 0.8f));
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new ImVector4(0.3f, 0.7f, 1.0f, 1.0f));
            }

            ImGui.PushID(tile.Id);

            ImVector2 uv0 = new ImVector2(tile.SourceX / sheetW, tile.SourceY / sheetH);
            ImVector2 uv1 = new ImVector2((tile.SourceX + tile.Width) / sheetW, (tile.SourceY + tile.Height) / sheetH);

            bool clicked = ImGui.ImageButton($"tile_{tile.Id}", activeTextureId, new ImVector2(itemSize, itemSize), uv0, uv1);

            if (clicked)
            {
                SelectTile(tile);
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

        ImGui.EndChild();
        }
    }

    public void AddGroup(TileGroup group, Texture2D texture, IntPtr imGuiTextureId)
    {
        _availableGroups.Add((group, texture, imGuiTextureId));
        Console.WriteLine($"[TilePalette] Registered group '{group.Name}'. Total in palette: {_availableGroups.Count}");
        if (_availableGroups.Count == 1 && group.Tiles.Count > 0)
        {
            SelectTile(group.Tiles[0]);
        }
    }

    public void SelectTile(TileDefinition tile)
    {
        SelectedTile = tile;
        selectedTileId = tile.Id;
    }

    public void Clear()
    {
        _availableGroups.Clear();
        SelectedTile = null;
        selectedTileId = -1;
        _selectedGroupIndex = 0;
    }
}