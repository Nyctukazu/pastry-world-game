using ImGuiNET;
using Microsoft.Xna.Framework;
using PastryWorld.Core.Level;
using ImVector2 = System.Numerics.Vector2;
using ImVector4 = System.Numerics.Vector4;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;
using static System.Math;
using System.Reflection.Emit;


namespace PastryWorld.Editor;

public class EditorStatusBar
{

    private static float _statusBarHeight;

    public EditorStatusBar(float StatusBarHeight)
    {
        _statusBarHeight = StatusBarHeight;
    }

    public static void Draw(
        MapData mapData,
        Vector2 mouseWorldPos,
        BrushController brush,
        int selectedTileId
    )
    {
        var viewport = ImGui.GetMainViewport();

        ImGui.SetNextWindowPos(new ImVector2(
            viewport.Pos.X,
            viewport.Pos.Y + viewport.Size.Y - _statusBarHeight
        ));

        ImGui.SetNextWindowSize(new ImVector2(viewport.Size.X, _statusBarHeight));

        ImGuiWindowFlags flags =
            ImGuiWindowFlags.NoTitleBar |
            ImGuiWindowFlags.NoResize |
            ImGuiWindowFlags.NoMove |
            ImGuiWindowFlags.NoScrollbar |
            ImGuiWindowFlags.NoSavedSettings |
            ImGuiWindowFlags.NoBringToFrontOnFocus;

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new ImVector2(10f, 4f));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1f);

        ImGui.PushStyleColor(ImGuiCol.WindowBg, new ImVector4(0.08f, 0.08f, 0.10f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.Border, new ImVector4(0.20f, 0.20f, 0.22f, 1.0f));

        if (ImGui.Begin("###EditorStatusBar", flags))
        {
            int tileX = (int)Floor(mouseWorldPos.X / mapData.TileSize);
            int tileY = (int)Floor(mouseWorldPos.Y / mapData.TileSize);
            bool isInsideMap = tileX >= 0 && tileX < mapData.Width && tileY >= 0 && tileY < mapData.Height;

            ImGui.TextDisabled("Map");
            ImGui.SameLine();
            ImGui.TextUnformatted(string.IsNullOrEmpty(mapData.Name) ? "Untitled" : mapData.Name);

            DrawSeparator();

            int pixelW = mapData.Width * mapData.TileSize;
            int pixelH = mapData.Height * mapData.TileSize;
            ImGui.TextDisabled("Size:");
            ImGui.SameLine();
            ImGui.TextUnformatted($"{mapData.Width}x{mapData.Height} tiles ({pixelW}x{pixelH}");


            DrawSeparator();

            ImGui.TextDisabled("Cursor:");
            ImGui.SameLine();
            if (isInsideMap)
            {
                ImGui.TextUnformatted($"Tile ({tileX}, {tileY}) | World ({(int)mouseWorldPos.X}, {(int)mouseWorldPos.Y})");
            }
            else
            {
                ImGui.TextColored(new ImVector4(0.7f, 0.7f, 0.7f, 1.0f), $"({tileX}, {tileY}) [Out of Bounds]");
            }

            DrawSeparator();

            ImGui.TextDisabled("Tool:");
            ImGui.SameLine();
            ImGui.TextUnformatted($"{brush.Mode}");
            ImGui.SameLine();
            ImGui.TextDisabled("| Tile:");
            ImGui.SameLine();
            ImGui.TextUnformatted($"#{selectedTileId}");

            DrawRightAlignedSection(mapData);

            ImGui.End();
        }
        ImGui.PopStyleColor(2);
        ImGui.PopStyleVar(3);
    }

    private static void DrawRightAlignedSection(MapData mapData)
    {
        var io = ImGui.GetIO();
        string fpsText = $"{io.Framerate: 0} FPS";
        string expandText = mapData.AutoExpand ? "Auto-Expand: ON" : "Auto-Expand: OFF";

        float rightSectionWidth = 
            ImGui.CalcTextSize(fpsText).X +
            ImGui.CalcTextSize(expandText).X + 60f;

        ImGui.SameLine(ImGui.GetWindowWidth() - rightSectionWidth);

        if (mapData.AutoExpand)
        {
            ImGui.TextColored(new ImVector4(0.3f, 0.8f, 0.3f, 1.0f), expandText);
        }
        else
        {
            ImGui.TextDisabled(expandText);
        }

        DrawSeparator();

        ImGui.TextColored(new ImVector4(0.4f, 0.8f, 1.0f, 1.0f), fpsText);
    }

    private static void DrawSeparator()
    {
        ImGui.SameLine();
        ImGui.TextDisabled("|");
        ImGui.SameLine();
    }
}
