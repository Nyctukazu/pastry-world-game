using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using XnaVector3 = Microsoft.Xna.Framework.Vector3;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;
using XnaMatrix = Microsoft.Xna.Framework.Matrix;
using XnaRectangle = Microsoft.Xna.Framework.Rectangle;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input.Touch;


namespace PastryWorld.Engine;

public class EditorCamera
{
    private readonly CameraPanTween _panTween = new();
    public float Zoom { get; private set; } = 3.0f;
    public float MinZoom { get; set; } = 0.5f;
    public float MaxZoom { get; set; } = 10.0f;
    public float ZoomStep { get; set; } = 0.25f;

    public XnaVector2 Position { get; set; } = XnaVector2.Zero;

    private int _previousScrollValue;
    private int _previousHorizontalScrollValue;
    private KeyboardState _previousKeyboardState;
    private XnaVector2 _previousCanvasMousePos;
    private Dictionary<String, PositionModel> cameraPositions = new Dictionary<String, PositionModel>();

    public EditorCamera()
    {
        TouchPanel.EnabledGestures |= GestureType.Pinch;
        cameraPositions["MapEditor"] = new PositionModel(new XnaVector2(500, 500), Zoom);
        cameraPositions["SmartObjectEditor"] = new PositionModel(new XnaVector2(200, 120), Zoom);
        cameraPositions["AnimationEditor"] = new PositionModel(new XnaVector2(200, 120), Zoom);
    }
    
    public XnaMatrix GetViewMatrix()
    {
        return XnaMatrix.CreateTranslation(new XnaVector3(-Position.X, -Position.Y, 0)) *
                XnaMatrix.CreateScale(Zoom, Zoom, 1.0f);
    }

    public XnaRectangle GetVisibleWorldBounds(XnaRectangle viewportBounds, XnaMatrix invertedView)
    {
        XnaVector2 topLeft = XnaVector2.Transform(new XnaVector2(viewportBounds.Left, viewportBounds.Top), invertedView);
        XnaVector2 bottomRight = XnaVector2.Transform(new XnaVector2(viewportBounds.Right, viewportBounds.Bottom), invertedView);

        int left = (int)MathF.Floor(topLeft.X);
        int top = (int)Math.Floor(topLeft.Y);
        int right = (int)MathF.Ceiling(bottomRight.X);
        int bottom = (int)MathF.Ceiling(bottomRight.Y);

        return new XnaRectangle(left, top, right - left, bottom - top);
    }

    public void PanTo(XnaVector2 target, float duration = 0.4f)
    {
        _panTween.Start(Position, target, duration);
    }

    public void SnapTo(XnaVector2 position, float? zoom = null)
    {
        _panTween.Cancel();
        Position = position;

        if (zoom.HasValue)
        {
            Zoom = MathHelper.Clamp(zoom.Value, MinZoom, MaxZoom);
        }
    }

    public void Update(GameTime gameTime)
    {
        if (_panTween.IsActive)
        {
            Position = _panTween.Update(gameTime);
        }
    }

    public void UpdateInput(XnaRectangle destinationRect, float scale)
    {
        KeyboardState keyState = Keyboard.GetState();
        MouseState mouseState = Mouse.GetState();

        int scrollDeltaY = mouseState.ScrollWheelValue - _previousScrollValue;
        int scrollDeltaX = mouseState.HorizontalScrollWheelValue - _previousHorizontalScrollValue;

        _previousScrollValue = mouseState.ScrollWheelValue;
        _previousHorizontalScrollValue = mouseState.HorizontalScrollWheelValue;
        
        XnaVector2 canvasMousePos = GetCanvasMousePosition(mouseState, destinationRect, scale);

        if (ImGui.GetIO().WantCaptureMouse)
        {
            _previousKeyboardState = keyState;
            _previousCanvasMousePos = canvasMousePos;
        }

        while (TouchPanel.IsGestureAvailable)
        {
            GestureSample gesture = TouchPanel.ReadGesture();

            if (gesture.GestureType == GestureType.Pinch)
            {
                XnaVector2 p1 = gesture.Position;
                XnaVector2 p2 = gesture.Position2;

                XnaVector2 prevP1 = p1 - gesture.Delta;
                XnaVector2 prevP2 = p2 - gesture.Delta2;

                float oldDistance = XnaVector2.Distance(prevP1, prevP2);
                float newDistance = XnaVector2.Distance(p1, p2);

                if (oldDistance > 0.001f)
                {
                    float scaleFactor = newDistance / oldDistance;
                    XnaVector2 pinchCenterScreen = (p1 + p2) * 0.5f;
                    XnaVector2 canvasPinchCenter = GetCanvasMousePosition(pinchCenterScreen, destinationRect, scale);

                    SetZoomAt(Zoom * scaleFactor, canvasPinchCenter);
                }
            }
        }

        bool isCtrlDown = keyState.IsKeyDown(Keys.LeftControl) || keyState.IsKeyDown(Keys.RightControl);
        bool isShiftDown = keyState.IsKeyDown(Keys.LeftShift) || keyState.IsKeyDown(Keys.RightShift);

        bool isMiddlePan = mouseState.MiddleButton == ButtonState.Pressed;
        bool isRightPan = mouseState.RightButton == ButtonState.Pressed;
        bool isSpacePan = keyState.IsKeyDown(Keys.Space);

        if (isMiddlePan || isRightPan || isSpacePan)
        {
            XnaVector2 mouseDelta = canvasMousePos - _previousCanvasMousePos;

            if (mouseDelta != XnaVector2.Zero)
            {
                _panTween.Cancel();
            }

            Position -= mouseDelta / Zoom;
        }

        if (isShiftDown)
        {
            if (scrollDeltaY != 0 || scrollDeltaX != 0)
            {
                _panTween.Cancel();
                float panSpeed = 0.5f;
                XnaVector2 panDelta = new XnaVector2(-scrollDeltaX, -scrollDeltaY) * panSpeed;
                Position -= panDelta / Zoom;
            }
        }

        if (isCtrlDown)
        {
            if (scrollDeltaY != 0)
            {
                float zoomFactor = 1.0f + (scrollDeltaY * 0.0015f);
                SetZoomAt(Zoom * zoomFactor, canvasMousePos);
            }


            if (JustPressed(keyState, Keys.OemPlus) || JustPressed(keyState, Keys.Add))
            {
                ZoomIn(canvasMousePos);
            }
            if (JustPressed(keyState, Keys.OemMinus) || JustPressed(keyState, Keys.Subtract))
            {
                ZoomOut(canvasMousePos);
            }
            if (JustPressed(keyState, Keys.D0) || JustPressed(keyState, Keys.NumPad0))
            {
                SetZoomAt(3.0f, canvasMousePos);
            }
        
        }

        _previousScrollValue = mouseState.ScrollWheelValue;
        _previousHorizontalScrollValue = mouseState.HorizontalScrollWheelValue;
        _previousKeyboardState = keyState;
        _previousCanvasMousePos = canvasMousePos;
    }

    public XnaVector2 GetCanvasMousePosition(XnaVector2 screenPos, XnaRectangle destinationRect, float scale)
    {
        if (scale <= 0) scale = 1;

        float canvasX = (screenPos.X - destinationRect.X) / (float)scale;
        float canvasY = (screenPos.Y - destinationRect.Y) / (float)scale;

        return new XnaVector2(canvasX, canvasY);
    }

    public XnaVector2 GetCanvasMousePosition(MouseState mouseState, XnaRectangle destinationRect, float scale)
    {
        return GetCanvasMousePosition(new XnaVector2(mouseState.X, mouseState.Y), destinationRect, scale);
    }


    public void SetZoomAt(float targetZoom, XnaVector2 screenFocusPos)
    {
        float newZoom = MathHelper.Clamp(targetZoom, MinZoom, MaxZoom);
        if (Math.Abs(newZoom - Zoom) < 0.0001f) return;

        _panTween.Cancel();

        XnaVector2 worldPosBeforeZoom = (screenFocusPos / Zoom) + Position;

        Zoom = newZoom;

        Position = worldPosBeforeZoom - (screenFocusPos / Zoom);
    }
    public void SaveCameraPosition(String key)
    {
      
        cameraPositions[linkData(key)] = new PositionModel(Position, Zoom);
    
    }

    public bool TryLoadCameraPosition(String key, out PositionModel model)
    {
        return cameraPositions.TryGetValue(linkData(key), out model);
    }

    private String linkData(String key)
    {
        if (key == "TileEditor" || key == "EntityEditor")
        {
            return "MapEditor";
        }
        return key;
    }

    public void LoadCameraPosition(String key)
    {
        if (TryLoadCameraPosition(key, out PositionModel model))
        {
            SnapTo(model.Position, model.Zoom);
        }
      
    }

    public void ZoomIn(XnaVector2 screenFocusPos) => SetZoomAt(Zoom + ZoomStep, screenFocusPos);
    public void ZoomOut(XnaVector2 screenFocusPos) => SetZoomAt(Zoom - ZoomStep, screenFocusPos);

    private bool JustPressed(KeyboardState currentState, Keys key)
    {
        return currentState.IsKeyDown(key) && _previousKeyboardState.IsKeyUp(key);
    }
}

public class PositionModel
{
    public XnaVector2 Position { get; set; }
    public float Zoom { get; set; }
    public PositionModel(XnaVector2 Position, float Zoom)
    {
        this.Position = Position;
        this.Zoom = Zoom;
    }
}