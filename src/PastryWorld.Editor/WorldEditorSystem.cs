using ImGuiNET;
using MonoGame.ImGuiNet;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;
using XnaMatrix = Microsoft.Xna.Framework.Matrix;
using System.IO;

using PastryWorld.Editor.Interfaces;
using PastryWorld.Core;
using PastryWorld.Maps;

using ImVector4 = System.Numerics.Vector4;

namespace PastryWorld.Editor;

public class WorldEditorSystem : IEditorSystem
{
    private readonly ImGuiRenderer _imGuiRenderer;
    private readonly MapData _mapData;
    private readonly JsonMapSerializer _mapSerializer;

    public bool IsActive { get; set; } = false;
    private string _currentTool = "Tile";
    private int _selectedTileId = 0;

    private string _mapNameInput = "MyCustomWorld";
    private string _statusMessage = "";

    public WorldEditorSystem(ImGuiRenderer imGuiRenderer, 
                            MapData mapData,
                            JsonMapSerializer mapSerializer)
    {
        _imGuiRenderer = imGuiRenderer;
        _mapData = mapData;
        _mapSerializer = mapSerializer;
    }

    public void ToggleMode() => IsActive = !IsActive;
    public void Update(GameTime gameTime, XnaMatrix cameraMatrix)
    {
        if (!IsActive) return;

        if (!ImGui.GetIO().WantCaptureMouse)
        {
            HandleMousePlacement(cameraMatrix);
        }
    }

    private void HandleMousePlacement(XnaMatrix cameraMatrix)
    {
        MouseState mouse = Mouse.GetState();
        if (mouse.LeftButton == ButtonState.Pressed)
        {
            XnaVector2 worldPos = XnaVector2.Transform(new XnaVector2(mouse.X, mouse.Y), XnaMatrix.Invert(cameraMatrix));
            int tileX = (int)(worldPos.X / 32);
            int tileY = (int)(worldPos.Y / 32);

            if (_currentTool == "Tile")
            {
                _mapData.SetTile(tileX, tileY, _selectedTileId);
            }
        }
    }

    public void DrawUI(GameTime gameTime)
    {
        if (!IsActive) return;
        _imGuiRenderer.BeginLayout(gameTime);
        RenderToolWindow();
        RenderTilesetWindow();
        _imGuiRenderer.EndLayout();
    }

    private void RenderToolWindow()
    {
        ImGui.Begin("PastryWorld Admin Tools");
        ImGui.Text($"Mode: {_currentTool}");
        if (ImGui.Button("Tile Tool")) _currentTool = "Tile";
        ImGui.SameLine();
        if (ImGui.Button("ObjectSpawner")) _currentTool = "Spawner";

        ImGui.Separator();

        ImGui.InputText("Map Name", ref _mapNameInput, 64);

        if (ImGui.Button("Save Map"))
        {
            try
            {
                string cleanName = _mapNameInput.Trim();
                string savePath = Path.Combine(MapPathUtility.GetUserMapsDirectory(), $"{cleanName}.json");
                _mapSerializer.SaveMap(_mapData, savePath);
                _statusMessage = $"Saved: {_mapNameInput}.json";
            }
            catch (System.Exception ex)
            {
                _statusMessage = $"Save Failed: {ex.Message}";
            }
        }

        ImGui.SameLine();

        if (ImGui.Button("Load Map"))
        {
            try
            {
                string loadPath = $"Content/Maps/{_mapNameInput.Trim()}.json";
                var loaded = _mapSerializer.LoadMap(loadPath);
                _statusMessage = $"Loaded: {_mapNameInput}.json";
            }
            catch (System.Exception ex)
            {
                _statusMessage = $"Load Failed: {ex.Message}";
            }
        }
        
        if (!string.IsNullOrEmpty(_statusMessage))
        {
            ImGui.TextColored(new ImVector4(0f, 1f, 0f, 1f), _statusMessage);
        }

        ImGui.End();
    }

    private void RenderTilesetWindow()
    {
        ImGui.Begin("Tileset Selector");
        for (int i = 0; i < 8; i++)
        {
            if (ImGui.Button($"Tile #{i}"))
            {
                _selectedTileId = i;
            }
            if ((i + 1) % 4 != 0)
            {
                ImGui.SameLine();
            }
        }
        ImGui.End();
    }
}