using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XnaMatrix = Microsoft.Xna.Framework.Matrix;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;
using XnaVector3 = Microsoft.Xna.Framework.Vector3;
using Microsoft.Xna.Framework.Input;
using System;

namespace PastryWorld.Engine;
/// <summary>
/// Represents a 2D camera system for managing view transformations, zooming, and rotation
/// </summary>
public class Camera2D
{
    private Viewport _viewport;


    public Vector2 Position { get; set; } = Vector2.Zero;
    public float Zoom { get; set; } = 1.0f;
    public float Rotation { get; set; } = 0.0f;

    public Camera2D(Viewport viewport)
    {
        _viewport = viewport;
        Position = Vector2.Zero;
    }

    public void UpdateViewport(Viewport viewport)
    {
        _viewport = viewport;
    }

    /// <summary>
    /// Computes the Transformation Matrix passed to SpriteBatch.Begin()
    /// </summary>
    /// <returns></returns>
    public Matrix GetViewMatrix()
    {
        XnaVector2 snappedPos = new XnaVector2(
            MathF.Floor(Position.X),
            MathF.Floor(Position.Y)
        );

        return Matrix.CreateTranslation(new XnaVector3(-snappedPos.X, -snappedPos.Y, 0.0f))
                * Matrix.CreateRotationZ(Rotation)
                * Matrix.CreateScale(Zoom, Zoom, 1.0f)
                * Matrix.CreateTranslation(new XnaVector3(_viewport.Width * 0.5f, _viewport.Height * 0.5f, 0.0f));
    }


    public XnaVector2 GetWorldMousePosition(Rectangle destinationRect, float scale)
    {
        MouseState mouseState = Mouse.GetState();

        if (scale <= 0f) scale = 1f;

        float canvasX = (mouseState.X - destinationRect.X) / scale;
        float canvasY = (mouseState.Y - destinationRect.Y) / scale;
        Vector2 canvasMouse = new Vector2(canvasX, canvasY);

        XnaMatrix invertedView = Matrix.Invert(GetViewMatrix());
        return Vector2.Transform(canvasMouse, invertedView);
    }

}
