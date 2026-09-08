using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;

namespace PastryWorld.Tools;

public static class ImGuiUtilities
{   
    /// <summary>
    /// Centers the cursor horizontally relative to a panel width or the remaining available window space.
    /// </summary>
    /// <param name="itemWidth">The total width of the element to center.</param>
    /// <param name="panelWidth">Optional total container width.  If omitted or <= 0, uses available content region.</param>
    public static void CenterCursorX(float itemWidth, float panelWidth = -1f)
    {
        if (panelWidth <= 0f)
        {
            panelWidth = ImGui.GetContentRegionAvail().X;
        }

        float targetX = (panelWidth - itemWidth) * 0.5f;
        if (targetX > 0f)
        {
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + targetX);
        }
    }
    /// <summary>
    /// Renders unformatted text horizontally centered in the current container.
    /// </summary>
    public static void TextCentered(string text)
    {
        float textWidth = ImGui.CalcTextSize(text).X;
        CenterCursorX(textWidth);
        ImGui.TextUnformatted(text);

    }
    /// <summary>
    /// Offsets the cursor to right-align an element of the specified width.
    /// </summary>
    public static void AlignRight(float itemWidth)
    {
        float availableWidth = ImGui.GetContentRegionAvail().X;
        float targetX = availableWidth - itemWidth;
        if (targetX > 0f)
        {
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + targetX);
        }
    }

    public static void HelpMarker(string desc)
    {
        ImGui.TextDisabled("(?)");
        if (ImGui.IsItemHovered(ImGuiHoveredFlags.DelayShort) && ImGui.BeginTooltip())
        {
            ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
            ImGui.TextUnformatted(desc);
            ImGui.PopTextWrapPos();
            ImGui.EndTooltip();
        }
    }
}