using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using ImGuiNET;
using Microsoft.Xna.Framework;
using PastryWorld.Core.Animation;
using ImVector4 = System.Numerics.Vector4;
using ImVector2 = System.Numerics.Vector2;

namespace PastryWorld.Editor.Animation;

public class AnimationTimelinePanel
{
    private AnimationClip _clip;
    private int _currentFrame;
    private bool _isPlaying;
    private float _playbackTimer;
    private int _draggedLayerIndex = -1;

    public bool OnionSkinEnabled = true;
    public int OnionFramesBefore = 1;
    public int OnionFramesAfter = 1;

    public string SelectedLayer { get; private set; }
    public int SelectedFrame { get; private set; }
    public int CurrentFrame => _currentFrame;

    public void SetClip(AnimationClip clip)
    {
        _clip = clip;
        _currentFrame = 0;
        _isPlaying = false;
    }

    public void Tick(GameTime gameTime)
    {
        if (!_isPlaying || _clip == null || _clip.FrameCount <= 0) return;

        _playbackTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        const float frameDuration = 1f / AnimationClip.Fps;
        if (_playbackTimer >= frameDuration)
        {
            _playbackTimer -= frameDuration;
            int next = _currentFrame + 1;

            if (next >= _clip.FrameCount)
            {
                next = _clip.Loop ? 0 : _clip.FrameCount - 1;
            }

            _currentFrame = next;
        }
    }

    public void Draw()
    {
        if (_clip == null)
        {
            ImGui.Text("No clip loaded.");
            return;
        }

        DrawTransport();
        ImGui.Separator();
        DrawOnionSkinControl();
        ImGui.Separator();
        DrawLayerGrid();
    }

    private void DrawTransport()
    {
        if (ImGui.Button(_isPlaying? "Pause" : "Play")) _isPlaying = !_isPlaying;

        ImGui.SameLine();
        if (ImGui.Button("|<")) 
        {
            _currentFrame = 0; _isPlaying = false; 
        }

        ImGui.SameLine();
        if (ImGui.Button("<")) 
        { 
            _currentFrame = Math.Max(0, _currentFrame - 1); _isPlaying = false; 
        }

        ImGui.SameLine();
        ImGui.Text($"Frame {_currentFrame + 1}/{_clip.FrameCount}  @ {AnimationClip.Fps}fps (locked)");

        ImGui.SameLine();
        if (ImGui.Button(">"))
        {
            _currentFrame = Math.Min(_clip.FrameCount - 1, _currentFrame + 1); 
            _isPlaying = false;
        }

        ImGui.SameLine();
        if (ImGui.Button(">|"))
        {
            _currentFrame = _clip.FrameCount - 1;
            _isPlaying = false;
        }

        ImGui.SameLine();
        ImGui.Checkbox("Loop", ref _clip.Loop);

        if (ImGui.Button("+ Add Frame")) {
            _clip.FrameCount++;
        }
        ImGui.SameLine();
        if (ImGui.Button("- Remove Last") && _clip.FrameCount > 1)
        {
            _clip.FrameCount--;
        }
    }

    private void DrawOnionSkinControl()
    {
        ImGui.Checkbox("Onion Skin", ref OnionSkinEnabled);
        if (!OnionSkinEnabled) return;

        ImGui.SameLine();
        ImGui.SetNextItemWidth(70);
        ImGui.SliderInt("Before", ref OnionFramesBefore, 0, 3);
        ImGui.SameLine();
        ImGui.SetNextItemWidth(70);
        ImGui.SliderInt("After", ref OnionFramesAfter, 0, 3);
    }

    private void DrawLayerGrid()
    {
        ImGui.Text("Layers");
        
        var sorted = new List<PartLayer>(_clip.Layers);
        sorted.Sort((a, b) => a.SortOrder.CompareTo(b.SortOrder));

        for (int i = 0; i < sorted.Count; i++)
        {
            var layer = sorted[i];
            ImGui.PushID(layer.PartName);

            ImGui.Checkbox("##vis", ref layer.Visible);
            ImGui.SameLine();

            ImGui.Selectable(Truncate(layer.PartName, 10), i == _draggedLayerIndex,
                ImGuiSelectableFlags.None, new ImVector2(90, 0));

            if (ImGui.IsItemActive())
            {
                _draggedLayerIndex = i;
                float dragDy = ImGui.GetMouseDragDelta(ImGuiMouseButton.Left).Y;

                if (dragDy < -10f && i > 0)
                {
                    (sorted[i], sorted[i - 1]) = (sorted[i - 1], sorted[i]);
                    ImGui.ResetMouseDragDelta(ImGuiMouseButton.Left);
                }
                else if (dragDy > 10f && i < sorted.Count - 1)
                {
                    (sorted[i], sorted[i + 1]) = (sorted[i + 1], sorted[i]);
                    ImGui.ResetMouseDragDelta(ImGuiMouseButton.Left);
                }
            }

            for (int f = 0; f < _clip.FrameCount; f++)
            {
                ImGui.SameLine();
                DrawFrameCell(layer, f);
            }
            ImGui.PopID();
        }

        if (!ImGui.IsMouseDown(ImGuiMouseButton.Left))
        {
            _draggedLayerIndex = -1;
        }

        for (int i = 0; i < sorted.Count; i++)
        {
            sorted[i].SortOrder = i;
        }

        if (ImGui.Button("+ Add Layer"))
        {
            _clip.Layers.Add(new PartLayer { PartName = "NewPart", SortOrder = _clip.Layers.Count });
        }
    }

    private void DrawFrameCell(PartLayer layer, int frame)
    {
        bool hasKey = layer.Keyframes.ContainsKey(frame);
        bool isSelected = SelectedLayer == layer.PartName && SelectedFrame == frame;
        bool isPlayhead = frame == _currentFrame;

        ImVector4 color = hasKey
            ? new ImVector4(0.9f, 0.7f, 0.2f, 1f)
            : new ImVector4(0.28f, 0.28f, 0.28f, 1f);
        if (isSelected) color = new ImVector4(0.2f, 0.8f, 0.9f, 1f);

        ImGui.PushStyleColor(ImGuiCol.Button, color);
        if (ImGui.Button($"##{frame}", new System.Numerics.Vector2(16, 16)))
        {
            _currentFrame = frame;
            SelectedLayer = layer.PartName;
            SelectedFrame = frame;

            if (!hasKey)
            {
                var held = GetHeldKeyframe(layer, frame);
                layer.Keyframes[frame] = held != null
                    ? new PartKeyframe { SpriteId = held.SpriteId, 
                                        X = held.X, 
                                        Y = held.Y, 
                                        Rotation = held.Rotation,
                                        FlipX = held.FlipX,
                                        FlipY = held.FlipY
                                        }
                    : new PartKeyframe();
            }
        }

        ImGui.PopStyleColor();

        if (isPlayhead)
        {
            var min = ImGui.GetItemRectMin();
            var max = ImGui.GetItemRectMax();
            ImGui.GetWindowDrawList().AddRect(min, max, 0xFFFFFFFF, 0f, ImDrawFlags.None, 2f);
        }
    }

    public PartKeyframe GetHeldKeyframe(PartLayer layer, int frame)
    {
        for (int f = frame; f >= 0; f--)
        {
            if (layer.Keyframes.TryGetValue(f, out var kf))
            {
                return kf;
            }
            
        }
        return null;
            
    }

    private static string Truncate(string s, int max) => s.Length <= max ? s : s.Substring(0, max);
}