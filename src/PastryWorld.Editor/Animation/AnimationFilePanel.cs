using ImGuiNET;
using PastryWorld.Core.Enums;
using PastryWorld.Core.Animation;
using PastryWorld.Core.Sprite;
using ImVector4 = System.Numerics.Vector4;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;
using PastryWorld.Tools;

namespace PastryWorld.Editor.Animation;

public class AnimationFilePanel
{
    private readonly AnimationManager _animationManager;
    private readonly AnimationEditor _editor;

    public AnimationFilePanel(AnimationManager animationManager, AnimationEditor editor)
    {
        _animationManager = animationManager;
        _editor = editor;
    }
    
    public void Update()
    {
        
    }
    public void DrawSetHeader()
    {
        DrawFileDropDown();

        if (ImGui.Button("Save (Ctrl+S)"))
        {
            _animationManager.SaveCurrentAnimation();
        }

        ImGui.SameLine();
        if (ImGui.Button("+ New"))
        {
            _editor.ResetToNewAnimation();
        }

        ImGui.SameLine();
        if (ImGui.Button("Refresh List"))
        {
            _animationManager.RefreshAnimationList();
        }

        if (!string.IsNullOrEmpty(_animationManager.StatusMessage))
        {
            ImVector4 color = _animationManager.IsStatusError
                ? new ImVector4(1f, 0.4f, 0.4f, 1f)
                : new ImVector4(0.4f, 1f, 0.4f, 1f);

            ImGui.TextColored(color, _animationManager.StatusMessage);
        }
    }

    public void DrawFileDropDown()
    {
        string currentAnimationName = _editor.AnimationSet.Name ?? "";

        if (ImGui.InputText("Animation Name", ref currentAnimationName, 64))
        {
            _editor.AnimationSet.Name = currentAnimationName;
            _editor.AnimationSet.Name = currentAnimationName;
        }

        if (_animationManager.AvailableAnimationFiles.Count > 0)
        {
            string currentPreview = _animationManager.SelectedAnimationIndex < _animationManager.AvailableAnimationFiles.Count
                ? _animationManager.AvailableAnimationFiles[_animationManager.SelectedAnimationIndex]
                : "Select Animation...";

            if (ImGui.BeginCombo("Load Existing", currentPreview))
            {
                for (int i = 0; i < _animationManager.AvailableAnimationFiles.Count; i++)
                {
                    bool isSelected = (_animationManager.SelectedAnimationIndex == i);

                    if (ImGui.Selectable(_animationManager.AvailableAnimationFiles[i], isSelected))
                    {
                        _animationManager.SelectedAnimationIndex = i;
                        _animationManager.LoadAnimation(_animationManager.AvailableAnimationFiles[i]);
                    }

                    if (isSelected) ImGui.SetItemDefaultFocus();
                }
                ImGui.EndCombo();
            }
        }
    }

}