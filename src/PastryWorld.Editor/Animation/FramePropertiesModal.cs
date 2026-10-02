using System;
using System.Collections.Generic;
using ImGuiNET;
using PastryWorld.Core;
using PastryWorld.Core.Animation;
using PastryWorld.Core.Enums;
using ImVector2 = System.Numerics.Vector2;
using ImVector4 = System.Numerics.Vector4;

namespace PastryWorld.Editor.Animation;
/// <summary>
/// Represents a modal dialog for editing the properties of a specific frame within an animation clip, including opacity, blend mode, channel visibility, stretch, rotation, and z-index.
/// </summary>
public class FramePropertiesModal
{
    private const string PopupId = "Frame Properties###FramePropsModal";

    private bool _shouldOpen;
    private AnimationClip _clip;
    private PartLayer _target;
    private int _frame;
    private Func<PartLayer, int, PartKeyframe> _getHeld;
    private float _opacity;
    private BlendMode _blend;
    private bool _chR, _chG, _chB, _chA;
    private float _stretchX, _stretchY;
    private bool _linkStretch = true;
    private float _rotation;
    private int _zIndex;

    public bool IsOpen { get; private set; }

    public void Open(AnimationClip clip, PartLayer layer, int frame, Func<PartLayer, int, PartKeyframe> getHeld)
    {
        if (layer == null || clip == null) return;
        _clip = clip;
        _target = layer;
        _frame = frame;
        _getHeld = getHeld;

        var kf = getHeld(layer, frame) ?? new PartKeyframe();

        _opacity = kf.Opacity;
        _blend = kf.Blend;
        _chR = kf.ChannelR; _chG = kf.ChannelG; _chB = kf.ChannelB; _chA = kf.ChannelA;
        _stretchX = kf.ScaleX;
        _stretchY = kf.ScaleY;
        _rotation = kf.Rotation;
        _zIndex = kf.Z_Index;
        _shouldOpen = true;
    }

    public void Draw()
    {
        if (_shouldOpen)
        {
            ImGui.OpenPopup(PopupId);
            _shouldOpen = false;
        }

        ImGui.SetNextWindowSize(new ImVector2(520, 480), ImGuiCond.Appearing);
        bool open = true;
        if (ImGui.BeginPopupModal(PopupId, ref open, ImGuiWindowFlags.NoResize))
        {
            IsOpen = true;
            ImGui.Text($"{_target?.PartName}   \u2014  Frame {_frame + 1}");
            ImGui.Separator();
            ImGui.SliderFloat("Opacity", ref _opacity, 0f, 1f);
            string[] blendNames = Enum.GetNames(typeof(BlendMode));
            int blendIdx = (int)_blend;
            if (ImGui.Combo("Blend Mode", ref blendIdx, blendNames, blendNames.Length))
            {
                _blend = (BlendMode)blendIdx;
            }

            ImGui.Text("Active Channels:");
            ImGui.SameLine();
            ImGui.Checkbox("Blue", ref _chB);
            ImGui.SameLine();
            ImGui.Checkbox("Green", ref _chG);
            ImGui.SameLine();
            ImGui.Checkbox("Red", ref _chR);
            ImGui.SameLine();
            ImGui.Checkbox("Alpha", ref _chA);

            ImGui.Separator();

            ImGui.SetNextItemWidth(140);
            bool xChanged = ImGui.SliderFloat("X Stretch", ref _stretchX, 0.1f, 3f);
            ImGui.SameLine();
            ImGui.Checkbox("Link##stretchLink", ref _linkStretch);

            ImGui.SetNextItemWidth(140);
            bool yChanged = ImGui.SliderFloat("Y Stretch", ref _stretchY, 0.1f, 3f);

            if (_linkStretch)
            {
                if (xChanged)
                {
                    _stretchY = _stretchX;
                   
                } else if (yChanged)
                {
                    _stretchX = _stretchY;
                } 

                ImGui.SliderAngle("Rotation", ref _rotation, -180f, 180f);
                ImGui.DragInt("Z-Index", ref _zIndex, 1, -1000, 1000);
                ImGui.Separator();
                DrawZOrderPanel();
                ImGui.Spacing();
                DrawFrameContentsPanel();
                ImGui.Separator();
                DrawFooterButtons();
                ImGui.EndPopup();
            } 
            else
            {
                IsOpen = false;
            }

        }
    }

    private void DrawZOrderPanel()
    {
        ImGui.TextDisabled("Z-Order at this frame (front to back):");
 
        if (ImGui.BeginChild("##zorderPanel", new ImVector2(0, 110), ImGuiChildFlags.Borders))
        {
            var entries = new List<(PartLayer layer, int z)>();
            foreach (var layer in _clip.Layers)
            {
                var held = _getHeld(layer, _frame);
                int z = layer == _target ? _zIndex : (held?.Z_Index ?? 0);
                entries.Add((layer, z));
            }
            entries.Sort((a, b) => b.z.CompareTo(a.z));
 
            foreach (var (layer, z) in entries)
            {
                bool isCurrent = layer == _target;
                if (isCurrent)
                    ImGui.PushStyleColor(ImGuiCol.Text, new ImVector4(0.2f, 0.8f, 0.9f, 1f));
 
                ImGui.Text($"{z,5}   {layer.PartName}");
 
                if (isCurrent)
                    ImGui.PopStyleColor();
            }
        }
        ImGui.EndChild();
    }
 
    /// <summary>
    /// Shows every layer's keyframe status (keyed / held / empty) and visibility at the current frame.
    /// </summary>
    private void DrawFrameContentsPanel()
    {
        ImGui.TextDisabled("In this frame:");
 
        if (ImGui.BeginChild("##frameContentsPanel", new ImVector2(0, 90), ImGuiChildFlags.Borders))
        {
            foreach (var layer in _clip.Layers)
            {
                bool hasExplicitKey = layer.Keyframes.ContainsKey(_frame);
                bool hasHeld = _getHeld(layer, _frame) != null;
                string tag = hasExplicitKey ? "[keyed]" : hasHeld ? "[held]" : "[empty]";
 
                ImVector4 color = layer.Visible
                    ? new ImVector4(1f, 1f, 1f, 1f)
                    : new ImVector4(0.5f, 0.5f, 0.5f, 1f);
 
                ImGui.TextColored(color, $"{layer.PartName}  {tag}");
            }
        }
        ImGui.EndChild();
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
 
        if (!_target.Keyframes.TryGetValue(_frame, out var kf))
        {
            kf = new PartKeyframe();
            var held = _getHeld(_target, _frame);
            if (held != null)
            {
                kf.SpriteId = held.SpriteId;
                kf.X = held.X;
                kf.Y = held.Y;
                kf.FlipX = held.FlipX;
                kf.FlipY = held.FlipY;
            }
            _target.Keyframes[_frame] = kf;
        }
 
        kf.Opacity = _opacity;
        kf.Blend = _blend;
        kf.ChannelR = _chR; kf.ChannelG = _chG; kf.ChannelB = _chB; kf.ChannelA = _chA;
        kf.ScaleX = _stretchX; kf.ScaleY = _stretchY;
        kf.Rotation = _rotation;
        kf.Z_Index = _zIndex;
    }

    
}