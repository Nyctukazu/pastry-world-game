using ImGuiNET;
using PastryWorld.Core;
using PastryWorld.Core.Animation;
using PastryWorld.Editor.Animation;

namespace PastryWorld.Editor.Animation;
/// <summary>
/// Provides a context menu for PartLayer objects within the animation timeline, allowing users to perform actions such as viewing properties, copying, pasting, duplicating, and deleting layers.
/// </summary>
public static class LayerContextMenu
{
    public static void Draw(PartLayer layer, AnimationTimelinePanel panel, out bool propertiesRequested)
    {
        propertiesRequested = false;
        if (ImGui.BeginPopupContextItem("layer_ctx_" + layer.PartName))
        {
            panel.SelectLayer(layer);
            if (ImGui.MenuItem("Properties", "F3"))
            {
                propertiesRequested = true;
            }

            ImGui.Separator();

            if (ImGui.MenuItem("Copy", "Ctrl+C"))
            {
                AnimationClipboard.Copy(layer);
            }

            if (ImGui.MenuItem("Paste", "Ctrl+V", false, AnimationClipboard.HasContent))
            {
                panel.PasteLayerBelow(layer);
            }

            if (ImGui.MenuItem("Duplicate", "Ctrl+D"))
            {
                panel.DuplicateLayer(layer);
            }

            ImGui.Separator();

            if (ImGui.MenuItem("Delete", "Del"))
            {
                panel.DeleteLayer(layer);
            }

            ImGui.EndPopup();
        }
    }
}

