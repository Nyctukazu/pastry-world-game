using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XnaRectangle = Microsoft.Xna.Framework.Rectangle;
using PastryWorld.Editor.Enums;
using ImGuiNET;
using System.Collections.Generic;
using PastryWorld.Editor.Commands;
using PastryWorld.Editor.Interfaces;
using PastryWorld.Core;
using PastryWorld.Core.Level;
using System.Text.RegularExpressions;

namespace PastryWorld.Editor;

public class BrushController
{   
    private readonly List<TileChange> _activeStrokeChanges = new();
    private bool _isPaintingStroke = false;
    public const int TileSize = 16;
    public BrushMode Mode = BrushMode.Single;
    public bool ShowGrid = true;
    public bool ShowSubGrid = false;
    private MouseState _prevMouse;
    private Point? _rectStart;
    private Point _hoverTile;
    private bool _hasHover;
    private Texture2D _pixel;
    private int _currentSelectedTile = 0;
    private int _strokeStartWidth;
    private int _strokeStartHeight;
    private List<int> _strokeStartGrid = new();
    private Point? _lastPaintedTile = null;

    public void Update(
        Vector2 mouseWorldPos, 
        int selectedTileIndex, 
        Action<int, int, int> paintTile, 
        MapData mapData, 
        CommandManager commandManager)
    {
        _currentSelectedTile = selectedTileIndex;
        MouseState mouse = Mouse.GetState();

        if (ImGui.GetIO().WantCaptureMouse)
        {
            _hasHover = false;
            _rectStart = null;
            _lastPaintedTile = null;
            CommitStroke(mapData, commandManager);
            _prevMouse = mouse;
            return;
        }

        int tx = (int)Math.Floor(mouseWorldPos.X / TileSize);
        int ty = (int)Math.Floor(mouseWorldPos.Y / TileSize);
        _hoverTile = new Point(tx, ty);
        _hasHover = true;
        
        bool leftPressed = mouse.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released;
        bool leftHeld = mouse.LeftButton == ButtonState.Pressed;
        bool leftReleased = mouse.LeftButton == ButtonState.Released && _prevMouse.LeftButton == ButtonState.Pressed;
        bool rightHeld = mouse.RightButton == ButtonState.Pressed;
        bool rightJustPressed = mouse.RightButton == ButtonState.Pressed && _prevMouse.RightButton == ButtonState.Released;
        bool rightReleased = mouse.RightButton == ButtonState.Released && _prevMouse.RightButton == ButtonState.Pressed;

        if (rightJustPressed && _rectStart.HasValue)
        {
            _rectStart = null;
        }

        switch (Mode)
        {
            case BrushMode.Single:
                int activeTileId = leftHeld ? selectedTileIndex : (rightHeld ? -1 : -2);
                if (leftHeld || rightHeld)
                {   
                    if (_lastPaintedTile.HasValue)
                    {
                        PlotLine(_lastPaintedTile.Value, _hoverTile, activeTileId, mapData, paintTile);
                    }
                    else
                    {
                        RecordAndPaint(tx, ty, activeTileId, mapData, paintTile);
                    }
                    _lastPaintedTile = _hoverTile;
                }

                if (!leftHeld && !rightHeld && (leftReleased || rightReleased))
                {
                    CommitStroke(mapData, commandManager);
                    _lastPaintedTile = null;
                }
                break;
                
            case BrushMode.Rectangle:
                if (leftPressed)
                {
                    _rectStart = _hoverTile;
                    
                }
                if (leftReleased && _rectStart.HasValue)
                {
                    FillRectangle(_rectStart.Value, _hoverTile, selectedTileIndex, mapData, paintTile);
                    CommitStroke(mapData, commandManager);
                    _rectStart = null;
                }
                break;
        }

        _prevMouse = mouse;
    }

    private void RecordAndPaint(int x, int y, int targetTileId, MapData mapData, Action<int, int, int> paintTile)
    {
        bool isOutOfBounds = x < 0 || y < 0 || x >= mapData.Width || y >= mapData.Height;
        if (!mapData.AutoExpand && isOutOfBounds)
        {
            return;
        }

        if (!_isPaintingStroke)
        {
            _isPaintingStroke = true;
            _strokeStartWidth = mapData.Width;
            _strokeStartHeight = mapData.Height;
            _strokeStartGrid = [..mapData.TileGrid];
        }

        if (mapData.AutoExpand)
        {
            mapData.EnsureCapacity(x, y);
        }

        int currentTileId = mapData.GetTile(x, y);

        if (currentTileId != targetTileId && !_activeStrokeChanges.Exists(c => c.X == x && c.Y == y))
        {
            _activeStrokeChanges.Add(new TileChange(x, y, currentTileId, targetTileId));
            paintTile(x, y, targetTileId);
        }
    }

    

    private void FillRectangle(Point a, Point b, int tileIndex, MapData mapData, Action<int, int, int> paintTile)
    {
        int minX = Math.Min(a.X, b.X), maxX = Math.Max(a.X, b.X);
        int minY = Math.Min(a.Y, b.Y), maxY = Math.Max(a.Y, b.Y);

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                RecordAndPaint(x, y, tileIndex, mapData, paintTile);
            }
        }
    }

    private void CommitStroke(MapData mapData, CommandManager commandManager)
    {
        if (_isPaintingStroke)
        {
            bool tilesChanged = _activeStrokeChanges.Count > 0;
            bool mapExpanded = mapData.Width != _strokeStartWidth || mapData.Height != _strokeStartHeight;

            // ONLY push to CommandManager if an actual state mutation occurred!
            if (tilesChanged || mapExpanded)
            {
                var strokeCopy = new List<TileChange>(_activeStrokeChanges);

                if (mapExpanded)
                {
                    var resizeCmd = new ResizeMapCommand(
                        mapData, 
                        _strokeStartWidth, 
                        _strokeStartHeight, 
                        _strokeStartGrid
                    );

                    var commands = new List<ICommand> { resizeCmd };
                    if (tilesChanged)
                    {
                        commands.Add(new PaintTilesCommand(mapData, strokeCopy));
                    }

                    commandManager.Execute(new CompositeCommand("Auto-Expand Map", commands));
                }
                else if (tilesChanged)
                {
                    commandManager.Execute(new PaintTilesCommand(mapData, strokeCopy));
                }
            }

            _activeStrokeChanges.Clear();
        }

        _isPaintingStroke = false;
    }

    private void PlotLine(Point start, Point end, int targetTileId, MapData mapData, Action<int, int, int> paintTile)
    {
        int x0 = start.X;
        int y0 = start.Y;
        int x1 = end.X;
        int y1 = end.Y;

        int dx = Math.Abs(x1 - x0);
        int dy = Math.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        int maxSteps = dx + dy + 1;

        while (maxSteps-- > 0)
        {
            RecordAndPaint(x0, y0, targetTileId, mapData, paintTile);

            if (x0 == x1 && y0 == y1) break;

            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }

    }

    public void DrawOverlay(SpriteBatch spriteBatch, XnaRectangle visibleWorldBounds, Texture2D pixel, MapData mapData)
    {
        _pixel = pixel;

        if (ShowGrid)
        {
            DrawGridLines(spriteBatch, visibleWorldBounds, TileSize, Color.White * 0.15f);
        }

        if (ShowSubGrid)
        {
            DrawGridLines(spriteBatch, visibleWorldBounds, TileSize / 2, Color.White * 0.06f);
        }

        if (_hasHover)
        {
            DrawRectOutline(spriteBatch, new XnaRectangle(_hoverTile.X * TileSize, _hoverTile.Y * TileSize, TileSize, TileSize), Color.Yellow, 1);
        }

        if (Mode == BrushMode.Rectangle && _rectStart.HasValue)
        {
            DrawRectPreview(spriteBatch, _rectStart.Value);
        }

        DrawOutOfBoundsShade(spriteBatch, mapData, pixel, visibleWorldBounds);
                    
    }

    public void DrawOutOfBoundsShade(SpriteBatch spriteBatch, MapData mapData, Texture2D pixel, XnaRectangle visibleWorldBounds)
    {
        int mapPxW = mapData.Width * mapData.TileSize;
        int mapPxH = mapData.Height * mapData.TileSize;
        Color darkOverlay = Color.Black * 0.6f;

        if (visibleWorldBounds.Top < 0)
        {
            int height = Math.Min(visibleWorldBounds.Height, -visibleWorldBounds.Top);
            spriteBatch.Draw(pixel, new XnaRectangle(visibleWorldBounds.Left, visibleWorldBounds.Top, visibleWorldBounds.Width, height), darkOverlay);
        }

        if (visibleWorldBounds.Bottom > mapPxH)
        {
            int y = Math.Max(visibleWorldBounds.Top, mapPxH);
            int height = visibleWorldBounds.Bottom - y;
            spriteBatch.Draw(pixel, new XnaRectangle(visibleWorldBounds.Left, y, visibleWorldBounds.Width, height), darkOverlay);
        }

        if (visibleWorldBounds.Left < 0)
        {
            int width = Math.Min(visibleWorldBounds.Width, -visibleWorldBounds.Left);
            int top = Math.Max(visibleWorldBounds.Top, 0);
            int bottom = Math.Min(visibleWorldBounds.Bottom, mapPxH);
            if (bottom > top)
            {
                spriteBatch.Draw(pixel, new XnaRectangle(visibleWorldBounds.Left, top, width, bottom - top), darkOverlay);
            }
        }

        if (visibleWorldBounds.Right > mapPxW)
        {
            int x = Math.Max(visibleWorldBounds.Left, mapPxW);
            int width = visibleWorldBounds.Right - x;
            int top = Math.Max(visibleWorldBounds.Top, 0);
            int bottom = Math.Min(visibleWorldBounds.Bottom, mapPxH);
            if (bottom > top)
            {
                spriteBatch.Draw(pixel, new XnaRectangle(x, top, width, bottom - top), darkOverlay);
            }
        }

        DrawRectOutline(spriteBatch, new XnaRectangle(0, 0, mapPxW, mapPxH), Color.Orange * 0.8f, 2);
    }

    private void DrawGridLines(SpriteBatch spriteBatch, XnaRectangle bounds, int cell, Color color)
    {
        int startX = bounds.Left / cell * cell;
        int startY = bounds.Top / cell * cell;

        for (int x = startX; x <= bounds.Right; x += cell)
        {
            spriteBatch.Draw(_pixel, new XnaRectangle(x, bounds.Top, 1, bounds.Height), color);
        }

        for (int y = startY; y <= bounds.Bottom; y += cell)
        {
            spriteBatch.Draw(_pixel, new XnaRectangle(bounds.Left, y, bounds.Width, 1), color);
        }
    }

    private void DrawRectPreview(SpriteBatch spriteBatch, Point start)
    {
        Point a = start, b = _hoverTile;
        int minX = Math.Min(a.X, b.X) * TileSize;
        int minY = Math.Min(a.Y, b.Y) * TileSize;
        int w = (Math.Abs(b.X - a.X) + 1) * TileSize;
        int h = (Math.Abs(b.Y - a.Y) + 1) * TileSize;

        DrawRectOutline(spriteBatch, new XnaRectangle(minX, minY, w, h), Color.Cyan, 1);
    }

    private void DrawRectOutline(SpriteBatch spriteBatch, XnaRectangle rect, Color color, int thickness)
    {
        spriteBatch.Draw(_pixel, new XnaRectangle(rect.Left, rect.Top, rect.Width, thickness), color);
        spriteBatch.Draw(_pixel, new XnaRectangle(rect.Left, rect.Bottom - thickness, rect.Width, thickness), color);
        spriteBatch.Draw(_pixel, new XnaRectangle(rect.Left, rect.Top, thickness, rect.Height), color);
        spriteBatch.Draw(_pixel, new XnaRectangle(rect.Right - thickness, rect.Top, thickness, rect.Height), color);
    }

}