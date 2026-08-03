using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XnaMatrix = Microsoft.Xna.Framework.Matrix;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;
using Microsoft.Xna.Framework.Input;

namespace PastryWorld.Engine;
/// <summary>
/// Represents a 2D camera system for managing view transformations, zooming, and rotation
/// </summary>
public class Camera2D
{
    private readonly Viewport _viewport;


    public Vector2 Position { get; set; }
    public float Zoom { get; set; } = 1.0f;
    public float Rotation { get; set; } = 0.0f;

    public Camera2D(Viewport viewport)
    {
        _viewport = viewport;
        Position = Vector2.Zero;
    }


    /// <summary>
    /// Smoothly moves the camera toward a target position
    /// </summary>
    /// <param name="targetPosition">The destination coordinates in the game world.</param>
    /// <param name="lerpAmount">The interpolation factor between 0.0f and 1.0f</param>
    public void Follow(Vector2 targetPosition, float lerpAmount = 0.1f)
    {
        Position = Vector2.Lerp(Position, targetPosition, lerpAmount);
    }

    /// <summary>
    /// Computes the Transformation Matrix passed to SpriteBatch.Begin()
    /// </summary>
    /// <returns></returns>
    public Matrix GetViewMatrix()
    {
        return Matrix.CreateTranslation(new Vector3(-Position.X, -Position.Y, 0.0f))
                * Matrix.CreateRotationZ(Rotation)
                * Matrix.CreateScale(Zoom, Zoom, 1.0f)
                * Matrix.CreateTranslation(new Vector3(_viewport.Width * 0.5f, _viewport.Height * 0.5f, 0.0f));
    }

    public XnaVector2 CameraPosition(EditorCamera _camera, XnaMatrix cameraMatrix)
    {
        _camera.UpdateInput();

        XnaMatrix finalCameraMatrix = cameraMatrix * _camera.GetViewMatrix();

        MouseState mouseState = Mouse.GetState();
        XnaVector2 screenPos = new XnaVector2(mouseState.X, mouseState.Y);

        XnaMatrix invertedCamera = XnaMatrix.Invert(finalCameraMatrix);
        XnaVector2 worldPos = XnaVector2.Transform(screenPos, invertedCamera);

        return worldPos;
    }

}
