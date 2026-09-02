using ImGuiNET;
using PastryWorld.Core.Sprite;
using ImVector4 = System.Numerics.Vector4;
using ImVector2 = System.Numerics.Vector2;
using System;


namespace PastryWorld.Editor.Animation;

public class SpritePalettePanel
{
    private AnimationEditor _editor;

    public SpritePalettePanel(AnimationEditor editor)
    {
        _editor = editor;
    }
    public void DrawSpritePalette()
    {
        ImGui.Text("Sprite Palette");
        if (_editor.ActiveManifest == null)
        {
            ImGui.TextDisabled("No sprite sheet loaded - use the Sprite Cutter tool.");
            return;
        }

        foreach (var slice in _editor.ActiveManifest.Sprites)
        {
            ImGui.Button(slice.Id, new ImVector2(64, 20));

        }
    }
}