using System;
using System.Collections.Generic;
using ImGuiNET;
using Microsoft.Xna.Framework;
using PastryWorld.Core.Animation;
using ImVector4 = System.Numerics.Vector4;
using ImVector2 = System.Numerics.Vector2;
using PastryWorld.Tools;
using System.Text;
using System.Runtime.CompilerServices;

namespace PastryWorld.Editor.Animation;
/// <summary>
/// Represents the animation timeline panel in the editor, allowing users to view and edit animation clips, layers, and keyframes.
/// </summary>
public class AnimationTimelinePanel
{
    private AnimationClip _clip;
    private int _currentFrame;
    private bool _isPlaying;
    private float _playbackTimer;
    private int _draggedLayerIndex = -1;

    private string _selectedLayerName;
    private string _renamingLayer;
    private bool _renameJustStarted;
    private readonly byte[] _renameBuffer = new byte[64];
    private readonly LayerPropertiesModal _layerPropsModal = new();
    private readonly FramePropertiesModal _framePropsModal = new();

    public bool OnionSkinEnabled = false;
    public int OnionFramesBefore = 1;
    public int OnionFramesAfter = 1;

    public string SelectedLayer { get; private set; }
    public int SelectedFrame { get; private set; }
    public int CurrentFrame => _currentFrame;
    private float panelHeight = 0;
    private float panelWidth = 0;
    /// <summary>
    /// Sets the current animation clip to be displayed and edited in the timeline panel.
    /// </summary>
    /// <param name="clip">The animation clip to display and edit</param>
    public void SetClip(AnimationClip clip)
    {
        _clip = clip;
        _currentFrame = 0;
        _isPlaying = false;
        _selectedLayerName = null;
        _renamingLayer = null;
    }
    /// <summary>
    /// Updates the timeline panel, advancing the playback timer and current frame if the animation is playing.
    /// </summary>
    /// <param name="gameTime">The game time</param>
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

    public void Draw(float width, float height)
    {
        panelWidth = width;
        panelHeight = height;
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
        _layerPropsModal.Draw();
        _framePropsModal.Draw();
    }

    private void DrawTransport()
    {
        float lineWidth = 380.0f;
        ImGuiUtilities.CenterCursorX(lineWidth, panelWidth);

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
        ImGui.Text($"Frame {_currentFrame + 1}/{_clip.FrameCount}  @ {AnimationClip.Fps}fps");

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
        
        if (_clip.Layers.Count == 0)
        {
            ImGui.TextDisabled("No layers yet.");
            if (ImGui.Button("+ Add Layer"))
            {
                _clip.Layers.Add(new PartLayer
                {
                    PartName = MakeUniqueName("NewPart"), SortOrder = _clip.Layers.Count
                });
            }
            return;
        }

        int columnCount = 1 + _clip.FrameCount;

        ImGuiTableFlags flags = ImGuiTableFlags.ScrollX
            | ImGuiTableFlags.ScrollY
            | ImGuiTableFlags.BordersInnerV
            | ImGuiTableFlags.RowBg
            | ImGuiTableFlags.SizingFixedFit
            | ImGuiTableFlags.NoSavedSettings;

        float tableHeight = ImGui.GetContentRegionAvail().Y - ImGui.GetFrameHeightWithSpacing();
        var sorted = new List<PartLayer>(_clip.Layers);
        sorted.Sort((a, b) => a.SortOrder.CompareTo(b.SortOrder));

        HandleLayerShortcuts(sorted);

        if (ImGui.BeginTable("###LayerFrameTable", columnCount, flags, new ImVector2(0, tableHeight)))
        {
            ImGui.TableSetupScrollFreeze(1, 1);

            ImGui.TableSetupColumn("Layers", ImGuiTableColumnFlags.WidthFixed, 110f);
            for (int f = 0; f < _clip.FrameCount; f++)
            {
                ImGui.TableSetupColumn($"##col_f{f}", ImGuiTableColumnFlags.WidthFixed, 16f);

            }

            ImGui.TableNextRow(ImGuiTableRowFlags.Headers);

            ImGui.TableSetColumnIndex(0);
            ImGui.TextDisabled("Layers");

            for (int f = 0; f < _clip.FrameCount; f++)
            {
                ImGui.TableSetColumnIndex(f + 1);
                DrawRulerCell(f);
            }

            for (int i = 0; i < sorted.Count; i++)
            {
                var layer = sorted[i];
                ImGui.TableNextRow();
                ImGui.PushID(layer.PartName);
                ImGui.TableSetColumnIndex(0);
                ImGui.Checkbox("##vis", ref layer.Visible);
                ImGui.SameLine();

                DrawLayerNameCell(layer, i);
                float rowAlpha = layer.Visible ? 1f : 0.4f;
                ImGui.PushStyleVar(ImGuiStyleVar.Alpha, rowAlpha);

                for (int f = 0; f < _clip.FrameCount; f++)
                {
                    ImGui.TableSetColumnIndex(f + 1);
                    DrawFrameCell(layer, f);
                }
                ImGui.PopStyleVar();
                ImGui.PopID();
            }

            if (!ImGui.IsMouseDown(ImGuiMouseButton.Left))
                _draggedLayerIndex = -1;

            for (int i = 0; i < sorted.Count; i++)
                sorted[i].SortOrder = i;

            ImGui.EndTable();
        }

        if (ImGui.Button("+ Add Layer"))
        {
            _clip.Layers.Add(new PartLayer { PartName = "NewPart", SortOrder = _clip.Layers.Count });
        }
    }

    private void DrawLayerNameCell(PartLayer layer, int rowIndex)
    {
        if (_renamingLayer == layer.PartName)
        {
            ImGui.SetNextItemWidth(90);
            if (_renameJustStarted)
            {
                ImGui.SetKeyboardFocusHere();
                _renameJustStarted = false;
            }
            bool enterPressed = ImGui.InputText("##rename", _renameBuffer, (uint)_renameBuffer.Length,
                ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.AutoSelectAll);
            bool lostFocus = ImGui.IsItemDeactivated();

            if (enterPressed || lostFocus)
            {
                CommitRename(layer);
            }
            return;
        }

        bool isSelected = _selectedLayerName == layer.PartName;
        ImGui.Selectable(Truncate(layer.PartName, 10), isSelected || rowIndex == _draggedLayerIndex,
            ImGuiSelectableFlags.None, new ImVector2(90, 0));

        if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
        {
            StartRename(layer);
        }

        LayerContextMenu.Draw(layer, this, out bool wantProperties);
        if (wantProperties)
        {
            _layerPropsModal.Open(layer);
        }

        if (ImGui.IsItemActivated())
        {
            SelectLayer(layer);
        }


        if (ImGui.IsItemActive())
        {
            _draggedLayerIndex = rowIndex;
            float dragDy = ImGui.GetMouseDragDelta(ImGuiMouseButton.Left).Y;
            var sorted = new List<PartLayer>(_clip.Layers);
            sorted.Sort((a, b) => a.SortOrder.CompareTo(b.SortOrder));

            if (dragDy < -10f && rowIndex > 0)
            {
                (sorted[rowIndex], sorted[rowIndex - 1]) = (sorted[rowIndex - 1], sorted[rowIndex]);
                for (int i = 0; i < sorted.Count; i++)
                {
                    sorted[i].SortOrder = i;
                }
                ImGui.ResetMouseDragDelta(ImGuiMouseButton.Left);
            }
            else if (dragDy > 10f && rowIndex < sorted.Count - 1)
            {
                (sorted[rowIndex], sorted[rowIndex + 1]) = (sorted[rowIndex + 1], sorted[rowIndex]);
                for (int i = 0; i < sorted.Count; i++) sorted[i].SortOrder = i;
                ImGui.ResetMouseDragDelta(ImGuiMouseButton.Left);
            }
        }
    }

    private void StartRename(PartLayer layer)
    {
        _renamingLayer = layer.PartName;
        _renameJustStarted = true;
        Array.Clear(_renameBuffer, 0, _renameBuffer.Length);
        var bytes = Encoding.UTF8.GetBytes(layer.PartName ?? string.Empty);
        Array.Copy(bytes, _renameBuffer, Math.Min(bytes.Length, _renameBuffer.Length - 1));
    }

    private void CommitRename(PartLayer layer)
    {
        string name = Encoding.UTF8.GetString(_renameBuffer).TrimEnd('\0');
        if (!string.IsNullOrWhiteSpace(name))
        {
            if (_selectedLayerName == layer.PartName) _selectedLayerName = name;
            layer.PartName = name;

        }
        _renamingLayer = null;
    }

    public void SelectLayer(PartLayer layer)
    {
        _selectedLayerName = layer?.PartName;
    }

    public void DuplicateLayer(PartLayer layer)
    {
        var clone = AnimationClipboard.Clone(layer);
        clone.PartName = MakeUniqueName(clone.PartName);
        InsertAfter(layer, clone);
        _selectedLayerName = clone.PartName;
    }

    public void PasteLayerBelow(PartLayer anchor)
    {
        var pasted = AnimationClipboard.PasteAsNew();
        if (pasted == null) return;

        pasted.PartName = MakeUniqueName(pasted.PartName);
        InsertAfter(anchor, pasted);
        _selectedLayerName = pasted.PartName;

    }

    public void DeleteLayer(PartLayer layer)
    {
        if (layer == null) return;

        _clip.Layers.Remove(layer);
        if (_selectedLayerName == layer.PartName) _selectedLayerName = null;

        var sorted = new List<PartLayer>(_clip.Layers);
        sorted.Sort((a, b) => a.SortOrder.CompareTo(b.SortOrder));

        for (int i = 0; i < sorted.Count; i++)
        {
            sorted[i].SortOrder = i;
        }
    }

    private void InsertAfter(PartLayer anchor, PartLayer newLayer)
    {
        var sorted = new List<PartLayer>(_clip.Layers);
        sorted.Sort((a, b) => a.SortOrder.CompareTo(b.SortOrder));
        int idx = sorted.FindIndex(l => l.PartName == anchor.PartName);
        sorted.Insert(idx + 1, newLayer);
        _clip.Layers.Add(newLayer);
        for (int i = 0; i < sorted.Count; i++)
        {
            sorted[i].SortOrder = i;
        }
    }

    private String MakeUniqueName(string baseName)
    {
        string name = baseName;
        int n = 1;
        while (_clip.Layers.Exists(l => l.PartName == name))
        {
            name = $"{baseName} ({n++})";
        }
        return name;
    }

    private void HandleLayerShortcuts(List<PartLayer> sorted)
    {
        if (_renamingLayer != null) return;
        if (ImGui.GetIO().WantTextInput) return;
        var selected = sorted.Find(l => l.PartName == _selectedLayerName);
        if (selected != null && ImGui.IsKeyPressed(ImGuiKey.F3, false))
        {
            _layerPropsModal.Open(selected);
        }

        if (selected == null) return;
        
        bool ctrl = ImGui.GetIO().KeyCtrl;

        if (ctrl && ImGui.IsKeyPressed(ImGuiKey.C, false))
        {
            AnimationClipboard.Copy(selected);
        }
        else if (ctrl && ImGui.IsKeyPressed(ImGuiKey.V, false))
        {
            PasteLayerBelow(selected);
        }
        else if (ctrl && ImGui.IsKeyPressed(ImGuiKey.D, false))
        {
            DuplicateLayer(selected);
        }
        else if (ImGui.IsKeyPressed(ImGuiKey.Delete, false))
        {
            DeleteLayer(selected);
        }
    }
    
    private void DrawRulerCell(int f)
    {
        int frameNumber = f + 1;
        ImGui.PushID($"ruler_f_{f}");

        ImVector2 cellSize = new ImVector2(16, 16);
        ImVector2 pos = ImGui.GetCursorScreenPos();
        var drawList = ImGui.GetWindowDrawList();

        bool clicked = ImGui.InvisibleButton("##ruler_cell", cellSize);
        bool isHovered = ImGui.IsItemHovered();
        bool isPlayhead = (f == _currentFrame);

        if (clicked)
        {
            _currentFrame = f;
            _isPlaying = false;
        }

        if (isHovered)
            drawList.AddRectFilled(pos, pos + cellSize, ImGui.ColorConvertFloat4ToU32(new ImVector4(1f, 1f, 1f, 0.2f)), 2f);
        else if (isPlayhead)
            drawList.AddRectFilled(pos, pos + cellSize, ImGui.ColorConvertFloat4ToU32(new ImVector4(0.2f, 0.8f, 0.9f, 0.25f)), 2f);

        ImVector4 textColor = isHovered
            ? new ImVector4(1f, 1f, 1f, 1f)
            : isPlayhead
                ? new ImVector4(0.2f, 0.8f, 0.9f, 1f)
                : new ImVector4(0.6f, 0.6f, 0.6f, 1f);

        string numStr = frameNumber.ToString();
        ImVector2 textSize = ImGui.CalcTextSize(numStr);
        ImVector2 textPos = pos + new ImVector2((cellSize.X - textSize.X) * 0.5f, (cellSize.Y - textSize.Y) * 0.5f);
        drawList.AddText(textPos, ImGui.ColorConvertFloat4ToU32(textColor), numStr);

        if (isHovered)
            ImGui.SetTooltip($"Jump to Frame {frameNumber}");

        ImGui.PopID();
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
        bool leftClicked = ImGui.Button($"##{frame}", new ImVector2(16, 16));
        ImGui.PopStyleColor();

        if (leftClicked)
        {
            _currentFrame = frame;
            SelectedLayer = layer.PartName;
            SelectedFrame = frame;

            if (!hasKey)
            {
                var held = GetHeldKeyframe(layer, frame);
                layer.Keyframes[frame] = held?.Clone() ?? new PartKeyframe();
            }
        }
        
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            _currentFrame = frame;
            SelectedLayer = layer.PartName;
            SelectedFrame = frame;
            _framePropsModal.Open(_clip, layer, frame, GetHeldKeyframe);
        }

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