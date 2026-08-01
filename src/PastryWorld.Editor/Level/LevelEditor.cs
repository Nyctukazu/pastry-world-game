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

namespace PastryWorld.Editor.Level;
public class LevelEditor
{

    private MapData _mapData;
    private JsonMapSerializer _mapSerializer;
    private int _selectedTileIndex = 0;
    private string _mapName = "MyCustomWorld";
    private string _statusMessage = "";
    private bool _isStatusError = false;
    private TileBrushController _brush = new TileBrushController();
    private CommandManager _commandManager;
    private EditorToolbar _toolbar;
    private TilePalette _palette;

    public LevelEditor()
    {
        _mapData = new MapData();
        _mapSerializer = new JsonMapSerializer();
        _commandManager = new CommandManager();
        _toolbar = new EditorToolbar(_commandManager);
        _palette = new TilePalette();
    }

    public void Update(XnaVector2 mouseWorldPos)
    {
        if (ImGui.GetIO().WantCaptureMouse) return;

        _brush.Update(
            mouseWorldPos, 
            _selectedTileIndex, 
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
        _brush.DrawOverlay(spriteBatch, visibleWorldBounds, pixel);
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
                WriteLine("Hello");
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
                _mapData = _mapSerializer.LoadMap(loadPath);
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
        _toolbar.DrawToolbarGui();

        ImGui.Spacing();
        DrawTilePaletteGui(_palette);
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

    public void DrawTilePaletteGui(TilePalette palette)
    {
        ImGui.Text("Tileset Palette");
        ImGui.Separator();

        foreach (var (id, def) in palette.Definitions)
        {
            bool isSelected = (_selectedTileIndex == id);
            if (isSelected) ImGui.PushStyleColor(ImGuiCol.Button, new ImVector4(0.2f, 0.6f, 1f, 1f));

            if (ImGui.Button($"{def.Name}##{id}", new ImVector2(70, 30)))
            {
                _selectedTileIndex = id;
            }

            if (isSelected) ImGui.PopStyleColor();
            if ((id + 1) % 2 != 0) ImGui.SameLine();
        }
    }

    private void SetStatus(string message, bool isError)
    {
        _statusMessage = message;
        _isStatusError = isError;
    }
}