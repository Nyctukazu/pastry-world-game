using System;
using System.Text;
using ImGuiNET;
using PastryWorld.Core.Animation;
using ImVector2 = System.Numerics.Vector2;
using ImVector4 = System.Numerics.Vector4;

namespace PastryWorld.Editor.Animation;

public class LayerPropertiesModal
{
    private const string PopupId = "Layer Properties###LayerPropsModal";

    private bool _shouldOpen;
    private PartLayer _target;
    private readonly byte[] _nameBuffer = new byte[64];
    private ImVector4 _color;

    public bool IsOpen { get; private set; }
    
    public void Open(PartLayer layer)
    {
        if (layer == null) return;

        _target = layer;
        _shouldOpen = true;

        Array.Clear(_nameBuffer, 0, _nameBuffer.Length);
        var bytes = Encoding.UTF8.GetBytes(layer.PartName ?? string.Empty);
        Array.Copy(bytes, _nameBuffer, Math.Min(bytes.Length, _nameBuffer.Length - 1));
        _color = layer.ColorLabel;
    }

    public void Draw()
    {
        if (_shouldOpen)
        {
            ImGui.OpenPopup(PopupId);
            _shouldOpen = false;
        }
        ImGui.SetNextWindowSize(new ImVector2(320, 170), ImGuiCond.Appearing);

        bool open = true;

        if (ImGui.BeginPopupModal(PopupId, ref open, ImGuiWindowFlags.NoResize))
        {
            IsOpen = true;
            ImGui.InputText("Name", _nameBuffer, (uint)_nameBuffer.Length);
            ImGui.ColorEdit4("Color Label", ref _color);
            ImGui.TextDisabled("Used to visually group / distinguish helper layers.");

            ImGui.Separator();
            DrawFooterButtons();

            ImGui.EndPopup();
        } 
        else
        {
            IsOpen = false;
        }
        
    }

    private void DrawFooterButtons()
    {
        const float buttonWidth = 80f;
        float avail = ImGui.GetContentRegionAvail().X;
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + avail - buttonWidth * 2 - ImGui.GetStyle().ItemSpacing.X);

        if (ImGui.Button("Cancel", new ImVector2(buttonWidth, 0)))
        {
            ImGui.CloseCurrentPopup();
        }

        ImGui.SameLine();
        if (ImGui.Button("OK", new ImVector2(buttonWidth, 0)))
        {
            Apply();
            ImGui.CloseCurrentPopup();
        }
    }

    private void Apply()
    {
        if (_target == null) return;
        string name = Encoding.UTF8.GetString(_nameBuffer).TrimEnd('\0');
        if (!string.IsNullOrWhiteSpace(name))
        {
            _target.PartName = name;
        }
        _target.ColorLabel = _color;
    }

    
}