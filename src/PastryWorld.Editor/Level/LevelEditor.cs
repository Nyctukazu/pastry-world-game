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
using System.Collections.Generic;
using System.Windows.Input;

namespace PastryWorld.Editor.Level;
public class LevelEditor
{
    private readonly TileRegistry _registry;
    private MapData _mapData;
    private JsonMapSerializer _mapSerializer;
    private string _mapName;
    private BrushController _brush;
    private CommandManager _commandManager;
    private EditorToolbar _toolbar;
    private TilePalette _palette;
    private XnaVector2 _mouseWorldPos;
    private readonly MapManager _mapManager;
    

    public LevelEditor(MapData mapData, CommandManager command, TileRegistry registry)
    {
        _mapData = mapData;
        _brush = new BrushController();
        _mapSerializer = new JsonMapSerializer();
        _commandManager = command;
        _toolbar = new EditorToolbar(_commandManager);
        _registry = registry;
        _palette = new TilePalette(_registry);
        _mapManager = new MapManager(_mapData, _commandManager, _mapSerializer, _mapName);
        
        _mapManager.RefreshMapList();


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

        var io = ImGui.GetIO();

        if (!io.WantTextInput && io.KeyCtrl && ImGui.IsKeyPressed(ImGuiKey.S))
        {
            _mapManager.SaveCurrentMap();
        }

        _commandManager.ProcessShortcuts();

        if (io.WantCaptureMouse) return;
    }

    public void Draw(SpriteBatch spriteBatch, Rectangle visibleWorldBounds, Texture2D pixel)
    {
        _brush.DrawOverlay(spriteBatch, visibleWorldBounds, pixel, _mapData);
    }

    public void DrawTileToolOptions()
    {


        string currentMapName = _mapManager.MapName ?? "";

        if (ImGui.InputText("Map Name", ref currentMapName, 64))
        {
            _mapManager.MapName = currentMapName;
            _mapData.Name = currentMapName;
        }
        if (_mapManager.AvailableMapFiles.Count > 0)
        {
            string currentPreview = _mapManager.SelectedMapIndex < _mapManager.AvailableMapFiles.Count
                ? _mapManager.AvailableMapFiles[_mapManager.SelectedMapIndex]
                : "Select Map...";

            if (ImGui.BeginCombo("Load Existing", currentPreview))
            {
                for (int i = 0; i < _mapManager.AvailableMapFiles.Count; i++)
                {
                    bool isSelected = (_mapManager.SelectedMapIndex == i);
                    if (ImGui.Selectable(_mapManager.AvailableMapFiles[i], isSelected))
                    {
                        _mapManager.SelectedMapIndex = i;
                        _mapManager.LoadMapByName(_mapManager.AvailableMapFiles[i]);
                    }

                    if (isSelected) ImGui.SetItemDefaultFocus();
                }
                ImGui.EndCombo();
            }
        }

    
        if (ImGui.Button("Save (Ctrl+S)")) 
        { 
            _mapManager.SaveCurrentMap();
        }

        ImGui.SameLine();
        if (ImGui.Button("+ New Map"))
        {
            _mapManager.CreateNewMap("UntitledMap", 50, 50);
        }

        if (ImGui.Button("Refresh List"))
        {
            _mapManager.RefreshMapList();
        }

        
        if (!string.IsNullOrEmpty(_mapManager.StatusMessage))
        {
            ImVector4 color = _mapManager.IsStatusError
                ? new ImVector4(1f, 0.4f, 0.4f, 1f)
                : new ImVector4(0.4f, 1f, 0.4f, 1f);

            ImGui.TextColored(color, _mapManager.StatusMessage);
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


}