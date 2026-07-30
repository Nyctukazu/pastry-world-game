using ImGuiNET;
using ImVector4 = System.Numerics.Vector4;
using ImVector2 = System.Numerics.Vector2;
using XnaMatrix = Microsoft.Xna.Framework.Matrix;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;
using Microsoft.Xna.Framework.Input;

using PastryWorld.Maps;
using PastryWorld.Core;
using System.IO;

namespace PastryWorld.Editor;
public class LevelEditor
{

    private MapData _mapData;
    private JsonMapSerializer _mapSerializer;
    private int _selectedTileIndex = 0;
    private string _mapName = "MyCustomWorld";
    private string _loadedMapStatus = "";
    private string _statusMessage = "";
    private bool _isStatusError = false;
    private string _currentTool = "Tile";
    private int _selectedTileId = 0;

    public LevelEditor()
    {
        _mapData = new MapData();
        _mapSerializer = new JsonMapSerializer();
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
        ImGui.Text("Tileset");
        for (int i = 0; i < 8; i++)
        {
            bool selected = _selectedTileIndex == i;
            if (selected) ImGui.PushStyleColor(ImGuiCol.Button, new ImVector4(0.25f, 0.55f, 0.9f, 1f));
            if (ImGui.Button($"Tile #{i}", new ImVector2(110, 24)))
            {
                _selectedTileIndex = i;
            }

            if (selected) ImGui.PopStyleColor();
            if (i % 2 == 0) ImGui.SameLine();
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

    private void SetStatus(string message, bool isError)
    {
        _statusMessage = message;
        _isStatusError = isError;
    }

}