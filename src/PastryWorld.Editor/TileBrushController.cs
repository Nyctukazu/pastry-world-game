using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XnaRectangle = Microsoft.Xna.Framework.Rectangle;
using PastryWorld.Editor;

public class TileBrushController
{
    public const int TileSize = 16;
    public BrushMode Mode = BrushMode.Single;
    public bool ShowGrid = true;
    public bool ShowSubGrid = false;
    private MouseState _prevMouse;
    private Point? _rectStart;
    private Point _hoverTile;
    private bool _hasHover;
    private Texture2D _pixel;
    public void Update(Vector2 mouseWorldPos, int selectedTileIndex, Action<int, int, int> paintTile)
    {
        MouseState mouse = Mouse.GetState();
        int tx = (int)Math.Floor(mouseWorldPos.X / TileSize);
        int ty = (int)Math.Floor(mouseWorldPos.Y / TileSize);
        _hoverTile = new Point(tx, ty);
        _hasHover = true;
        
        bool justPressed = mouse.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released;
        bool held = mouse.LeftButton == ButtonState.Pressed;
        bool justReleased = mouse.LeftButton == ButtonState.Released && _prevMouse.LeftButton == ButtonState.Pressed;

        switch (Mode)
        {
            case BrushMode.Single:
                if (held)
                {
                    paintTile(tx, ty, selectedTileIndex);
                }
                break;
            
            case BrushMode.Rectangle:
                if (justPressed)
                {
                    _rectStart = _hoverTile;
                }
                if (justReleased && _rectStart.HasValue)
                {
                    FillRectangle(_rectStart.Value, _hoverTile, selectedTileIndex, paintTile);
                    _rectStart = null;
                }
                break;
        }

        _prevMouse = mouse;
    }

    private void FillRectangle(Point a, Point b, int tileIndex, Action<int, int, int> paintTile)
    {
        int minX = Math.Min(a.X, b.X), maxX = Math.Max(a.X, b.X);
        int minY = Math.Min(a.Y, b.Y), maxY = Math.Max(a.Y, b.Y);

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                paintTile(x, y, tileIndex);
            }
        }
    }

    public void DrawOverlay(SpriteBatch spriteBatch, XnaRectangle visibleWorldBounds, Texture2D pixel)
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
                    
    }

    private void DrawGridLines(SpriteBatch spriteBatch, XnaRectangle bounds, int cell, Color color)
    {
        int startX = (bounds.Left / cell) * cell;
        int startY = (bounds.Top / cell) * cell;

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