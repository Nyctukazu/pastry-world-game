using ImGuiNET;
using PastryWorld.Editor.Enums;
using ImVector4 = System.Numerics.Vector4;
using ImVector2 = System.Numerics.Vector2;
using XnaRectangle = Microsoft.Xna.Framework.Rectangle;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;
using Microsoft.Xna.Framework.Graphics;
using PastryWorld.Editor.Level;
using PastryWorld.Editor.Entity;
using PastryWorld.Editor.Animation;
using Microsoft.Xna.Framework;
using PastryWorld.Engine;
using System;


namespace PastryWorld.Editor;

/// <summary>
/// Left-hand icon rail + a flyout options panel that swaps content
/// based on the currently selected tool.  Call Draw() once per frame
/// from you ImGui render pass.
/// </summary>
public class ToolRailPanel
{
    private EditorTool _activeTool = EditorTool.None;
    public EditorTool ActiveTool => _activeTool;
    public bool IsAnimationToolActive => _activeTool == EditorTool.AnimationEditor;
    private LevelEditor _levelTool;
    private EntityEditor _entityTool;
    private SmartObjectEditor _smartObjectTool;
    private AnimationEditor _animationTool;
    private const float RailWidth = 56f;
    private const float ButtonSize = 35f;
    private const float PanelWidth = 275f;
    private const float StatusBarHeight = 26f;
    private EditorStatusBar _statusBar = new EditorStatusBar(StatusBarHeight);
    private float PanelHeight;
    private EditorCamera _editorCamera;
    
    public ToolRailPanel(LevelEditor level, 
                        EntityEditor entity, 
                        SmartObjectEditor smartObject, 
                        AnimationEditor animation,
                        EditorCamera editorCamera)
    {
        _levelTool = level;
        _entityTool = entity;
        _smartObjectTool = smartObject;
        _animationTool = animation;
        _editorCamera = editorCamera;
        _editorCamera.SaveCameraPosition(_activeTool.ToString());
    }

    public void Update(GameTime gameTime, XnaVector2 worldPos)
    {
        switch (_activeTool)
        {
            case EditorTool.TileEditor:
                _levelTool.Update(worldPos);
                break;
            case EditorTool.EntityEditor:

                break;
            case EditorTool.SmartObjectEditor:
  
                break;
            case EditorTool.AnimationEditor:
                _animationTool.UpdateWorld(gameTime, worldPos);

                break;
        }
    }

    public void Draw(SpriteBatch spriteBatch, XnaRectangle visibleWorldBounds, Texture2D pixel)
    {
        switch (_activeTool)
        {
            case EditorTool.TileEditor:
                _levelTool.Draw(spriteBatch, visibleWorldBounds, pixel);
                break;
            case EditorTool.EntityEditor:

                break;
            case EditorTool.SmartObjectEditor:

                break;
            case EditorTool.AnimationEditor:
                _animationTool.Draw(spriteBatch, visibleWorldBounds, pixel);

                break;
        }
    }
    public void DrawGui()
    {
        DrawRail();
        if (_activeTool != EditorTool.None)
            DrawFlyoutPanel();
    }

    private void DrawRail()
    {   
        var viewport = ImGui.GetMainViewport();

        PanelHeight = viewport.Size.Y - StatusBarHeight;

        ImGui.SetNextWindowPos(new ImVector2(0, 0), ImGuiCond.Always);
        ImGui.SetNextWindowSize(new ImVector2(RailWidth, PanelHeight), ImGuiCond.Always);

        ImGuiWindowFlags flags = ImGuiWindowFlags.NoTitleBar
            | ImGuiWindowFlags.NoResize
            | ImGuiWindowFlags.NoMove
            | ImGuiWindowFlags.NoCollapse
            | ImGuiWindowFlags.NoScrollbar;

        ImGui.Begin("##ToolRail", flags);

        RailButton(EditorTool.TileEditor, "Tile");
        RailButton(EditorTool.EntityEditor, "Spwn");
        RailButton(EditorTool.SmartObjectEditor, "Objc");
        RailButton(EditorTool.AnimationEditor, "Anim");

        ImGui.End();
    }

    private void RailButton(EditorTool tool, string label)
    {

        bool isActive = _activeTool == tool;

        if (isActive)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, new ImVector4(0.25f, 0.55f, 0.9f, 1f));
        }

        if (ImGui.Button(label, new ImVector2(ButtonSize, ButtonSize)))
        {
            EditorTool newTool = isActive ? EditorTool.None : tool;
            SwitchTool(newTool);
        }

        if (isActive)
        {
            ImGui.PopStyleColor();
        }

        ImGui.Spacing();
    }

    private void SwitchTool(EditorTool newTool)
    {
        if (newTool == _activeTool) return;
        _editorCamera.SaveCameraPosition(_activeTool.ToString());
        _activeTool = newTool;

        if (_editorCamera.TryLoadCameraPosition(_activeTool.ToString(), out PositionModel saved))
        {
            _editorCamera.SnapTo(saved.Position, saved.Zoom);
        }
    }

    private void DrawFlyoutPanel()
    {
        ImGui.SetNextWindowPos(new ImVector2(RailWidth, 0), ImGuiCond.Always);
        ImGui.SetNextWindowSize(new ImVector2(PanelWidth, PanelHeight), ImGuiCond.Always);

        ImGuiWindowFlags flags = ImGuiWindowFlags.NoTitleBar
            | ImGuiWindowFlags.NoResize
            | ImGuiWindowFlags.NoMove
            | ImGuiWindowFlags.NoCollapse;

        ImGui.Begin("##ToolOptions", flags);
        ImGui.Text(HeaderFor(_activeTool));
        ImGui.Separator();

        switch (_activeTool)
        {
            case EditorTool.TileEditor:
                _levelTool.DrawTileToolOptions();
                break;
            case EditorTool.EntityEditor:
                _entityTool.DrawEntityOptions();
                break;
            case EditorTool.SmartObjectEditor:
                _smartObjectTool.DrawSmartObjectOptions();
                break;
            case EditorTool.AnimationEditor:
                _animationTool.DrawAnimationOptions();
                break;
        }

        ImGui.End();
    }

    private string HeaderFor(EditorTool tool) => tool switch
    {
        EditorTool.TileEditor => "Tile Editor",
        EditorTool.EntityEditor => "Entity Editor",
        EditorTool.SmartObjectEditor => "Smart Object Editor",
        EditorTool.AnimationEditor => "Animation Editor",
        _ => ""
    };
}
