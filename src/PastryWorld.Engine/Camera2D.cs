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
    /// Smoothly moves the camera toward a target position
    /// </summary>
    /// <param name="targetPosition">The destination coordinates in the game world.</param>
    /// <param name="lerpAmount">The interpolation factor between 0.0f and 1.0f</param>
    public void Follow(XnaVector2 targetPosition, float lerpAmount = 0.1f)
    {
        Position = XnaVector2.Lerp(Position, targetPosition, lerpAmount);
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


    public XnaVector2 GetWorldMousePosition(Rectangle destinationRect, int scale)
    {
        MouseState mouseState = Mouse.GetState();

        if (scale <= 0) scale = 1;

        float canvasX = (mouseState.X - destinationRect.X) / (float)scale;
        float canvasY = (mouseState.Y - destinationRect.Y) / (float)scale;
        Vector2 canvasMouse = new Vector2(canvasX, canvasY);

        XnaMatrix invertedView = Matrix.Invert(GetViewMatrix());
        return Vector2.Transform(canvasMouse, invertedView);
    }

}
