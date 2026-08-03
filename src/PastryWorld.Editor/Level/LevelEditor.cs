using ImGuiNET;
using ImVector4 = System.Numerics.Vector4;
using ImVector2 = System.Numerics.Vector2;
using XnaMatrix = Microsoft.Xna.Framework.Matrix;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;
using XnaRectangle = Microsoft.Xna.Framework.Rectangle;
using Microsoft.Xna.Framework.Input;
using static System.Console;

using PastryWorld.Maps;
using PastryWorld.Core.Level;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PastryWorld.Editor.Enums;
using PastryWorld.Editor.Commands;
using System;
using System.Linq;
using MonoGame.ImGuiNet;

namespace PastryWorld.Editor.Level;
public class LevelEditor
{
    private readonly TileRegistry _registry;
    private MapData _mapData;
    private JsonMapSerializer _mapSerializer;
    private string _mapName = "MyCustomWorld";
    private string _statusMessage = "";
    private bool _isStatusError = false;
    private BrushController _brush = new BrushController();
    private CommandManager _commandManager;
    private EditorToolbar _toolbar;
    private TilePalette _palette;
    private XnaVector2 _mouseWorldPos;

    public LevelEditor(MapData mapData, CommandManager command, TileRegistry registry)
    {
        _mapData = mapData;
        _mapSerializer = new JsonMapSerializer();
        _commandManager = command;
        _toolbar = new EditorToolbar(_commandManager);
        _registry = registry;
        _palette = new TilePalette(_registry);

    }

    public void LoadContent(Texture2D spritesheet, ImGuiRenderer imGuiRenderer)
    {
        IntPtr imGuiTexId = imGuiRenderer.BindTexture(spritesheet);

        _palette.Load(spritesheet, imGuiTexId);
    }

    public void Update(XnaVector2 mouseWorldPos)
    {
        int activeTileId = _palette.selectedTileIndex;
        if (ImGui.GetIO().WantCaptureMouse) return;

        _mouseWorldPos = mouseWorldPos;

        _brush.Update(
            mouseWorldPos, 
            activeTileId,
            (x, y, tileId) =>
            {
                _mapData.SetTile(x, y, tileId);
            },
            _mapData,
            _commandManager
        );
    }

    public void Draw(SpriteBatch spriteBatch, Rectangle visibleWorldBounds, Texture2D pixel)
    {
        _brush.DrawOverlay(spriteBatch, visibleWorldBounds, pixel, _mapData);
    }

    public void DrawTileToolOptions()
    {
        ImGui.InputText("Map Name", ref _mapName, 64);

        string cleanName = _mapName.Trim();
        bool isNameValid = !string.IsNullOrWhiteSpace(cleanName);

        if (!isNameValid) ImGui.BeginDisabled();
        if (ImGui.Button("Save Map")) 
        { 
            try
            {
                string savePath = MapPathUtility.GetFullPathForMap(_mapName);
                _mapSerializer.SaveMap(_mapData, savePath);
                SetStatus($"Saved: {cleanName}.json", isError: false);
            }
            catch (System.Exception ex)
            {
                SetStatus($"Save Failed: {ex.Message}", isError: true);
            }
        }

        ImGui.SameLine();
        if (ImGui.Button("Load Map"))
        {
            try
            {
                string loadPath = Path.Combine(MapPathUtility.GetUserMapsDirectory(), $"{cleanName}.json");
                MapData loadedMap = _mapSerializer.LoadMap(loadPath);
                _mapData.CopyFrom(loadedMap);
                _commandManager.Clear();
                SetStatus($"Loaded: {cleanName}.json", isError: false);
            }
            catch (System.Exception ex)
            {
                SetStatus($"Load Failed: {ex.Message}", isError: true);
            }
        }

        if (!isNameValid) ImGui.EndDisabled();
        if (!string.IsNullOrEmpty(_statusMessage))
        {
            ImVector4 color = _isStatusError
                ? new ImVector4(1f, 0.4f, 0.4f, 1f)
                : new ImVector4(0.4f, 1f, 0.4f, 1f);

            ImGui.TextColored(color, _statusMessage);
        }

        ImGui.Spacing();
        ImGui.Text("Brush");
        BrushModeButton("Brush", BrushMode.Single);
        ImGui.SameLine();
        BrushModeButton("Rect", BrushMode.Rectangle);

        ImGui.Spacing();
        ImGui.Checkbox("Show Grid (16x16)", ref _brush.ShowGrid);
        ImGui.Checkbox("Show Sub-grid (8x8)", ref _brush.ShowSubGrid);

        ImGui.Spacing();
        ImGui.Text($"Dimensions: {_mapData.Width} x {_mapData.Height} Tiles");

        bool autoExpand = _mapData.AutoExpand;
        if (ImGui.Checkbox("Auto-Expand Bounds on Paint", ref autoExpand))
        {
            _mapData.AutoExpand = autoExpand;
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("When enabled, painting past the map edges automatically expands the map size.");
        }

        ImGui.Spacing();
        _toolbar.DrawToolbarGui();

        ImGui.Spacing();
        _palette.DrawTilePaletteGui();

        EditorStatusBar.Draw(_mapData, _mouseWorldPos, _brush, _palette.selectedTileIndex);
    }

    private void BrushModeButton(string label, BrushMode mode)
    {
        bool active = _brush.Mode == mode;
        if (active)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, new ImVector4(0.25f, 0.55f, 0.9f, 1f));
        }

        if (ImGui.Button(label, new ImVector2(60, 28)))
        {
            _brush.Mode = mode;

        }
           
        if (active)
        {
            ImGui.PopStyleColor();
        }
    }

    private void SetStatus(string message, bool isError)
    {
        _statusMessage = message;
        _isStatusError = isError;
    }
}