using System;
using Microsoft.Xna.Framework;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;

namespace PastryWorld.Engine;

public class CameraPanTween
{
    private XnaVector2 _start;
    private XnaVector2 _target;
    private float _duration;
    private float _elapsed;

    public bool IsActive { get; private set; }

    public void Start(XnaVector2 from, XnaVector2 to, float durationSeconds)
    {
        _start = from;
        _target = to;
        _duration = MathF.Max(durationSeconds, 0.0001f);
        _elapsed = 0f;
        IsActive = true;
    }
    /// <summary>
    /// Cancels the tween in place
    /// </summary>
    public void Cancel() => IsActive = false;

    public XnaVector2 Update(GameTime gameTime)
    {
        if (!IsActive) return _target;

        _elapsed += (float)gameTime.ElapsedGameTime.TotalSeconds;
        float t = Math.Clamp(_elapsed / _duration, 0f, 1f);
        float eased = EaseOutCubic(t);

        if (t >= 1f) IsActive = false;

        return XnaVector2.Lerp(_start, _target, eased);
    }

    private static float EaseOutCubic(float t) => 1f - MathF.Pow(1f - t, 3f);
}