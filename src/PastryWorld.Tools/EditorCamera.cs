using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using XnaVector3 = Microsoft.Xna.Framework.Vector3;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;
using XnaMatrix = Microsoft.Xna.Framework.Matrix;
using XnaRectangle = Microsoft.Xna.Framework.Rectangle;


namespace PastryWorld;

public class EditorCamera
{
    public float Zoom { get; private set; } = 3.0f;
    public float MinZoom { get; set; } = 0.5f;
    public float MaxZoom { get; set; } = 6.0f;
    public float ZoomStep { get; set; } = 0.25f;

    public XnaVector2 Position { get; set; } = XnaVector2.Zero;

    private int _previousScrollValue;
    private KeyboardState _previousKeyboardState;
    private Point _previousMousePosition;
    
    public XnaMatrix GetViewMatrix()
    {
        return XnaMatrix.CreateTranslation(new XnaVector3(-Position.X, -Position.Y, 0)) *
                XnaMatrix.CreateScale(Zoom, Zoom, 1.0f);
    }

    public XnaRectangle GetVisibleWorldBounds(XnaRectangle viewportBounds, XnaMatrix invertedView)
    {
        XnaVector2 topLeft = XnaVector2.Transform(new XnaVector2(viewportBounds.Left, viewportBounds.Top), invertedView);
        XnaVector2 bottomRight = XnaVector2.Transform(new XnaVector2(viewportBounds.Right, viewportBounds.Bottom), invertedView);

        XnaRectangle visibleWorldBounds = new XnaRectangle(
            (int)topLeft.X,
            (int)topLeft.Y,
            (int)(bottomRight.X - topLeft.X),
            (int)(bottomRight.Y - topLeft.Y)
        );

        return visibleWorldBounds;
    }

    public void UpdateInput()
    {
        KeyboardState keyState = Keyboard.GetState();
        MouseState mouseState = Mouse.GetState();
        Point currentMousePos = new Point(mouseState.X, mouseState.Y);

        bool isMiddlePan = mouseState.MiddleButton == ButtonState.Pressed;
        bool isSpacePan = keyState.IsKeyDown(Keys.Space);

        if (isMiddlePan || isSpacePan)
        {
            XnaVector2 mouseDelta = new XnaVector2(
                currentMousePos.X - _previousMousePosition.X,
                currentMousePos.Y - _previousMousePosition.Y
            );

            Position -= mouseDelta / Zoom;
        }

        bool isCtrlDown = keyState.IsKeyDown(Keys.LeftControl) || keyState.IsKeyDown(Keys.RightControl);

        if (isCtrlDown)
        {
            int scrollDelta = mouseState.ScrollWheelValue - _previousScrollValue;
            if (scrollDelta > 0)
            {
                ZoomIn();
            }
            else if (scrollDelta < 0)
            {
                ZoomOut();
            }

            if (JustPressed(keyState, Keys.OemPlus) || JustPressed(keyState, Keys.Add))
            {
                ZoomIn();
            }
            if (JustPressed(keyState, Keys.OemMinus) || JustPressed(keyState, Keys.Subtract))
            {
                ZoomOut();
            }
            if (JustPressed(keyState, Keys.D0) || JustPressed(keyState, Keys.NumPad0))
            {
                Zoom = 3.0f;
            }
        }

        _previousScrollValue = mouseState.ScrollWheelValue;
        _previousKeyboardState = keyState;
        _previousMousePosition = currentMousePos;
    }

    public void ZoomIn() => Zoom = MathHelper.Clamp(Zoom + ZoomStep, MinZoom, MaxZoom);
    public void ZoomOut() => Zoom = MathHelper.Clamp(Zoom - ZoomStep, MinZoom, MaxZoom);

    private bool JustPressed(KeyboardState currentState, Keys key)
    {
        return currentState.IsKeyDown(key) && _previousKeyboardState.IsKeyUp(key);
    }
}