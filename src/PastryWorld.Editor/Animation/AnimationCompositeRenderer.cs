
using PastryWorld.Core.Enums;
using PastryWorld.Core.Animation;
using PastryWorld.Core.Sprite;
using ImVector4 = System.Numerics.Vector4;
using ImVector2 = System.Numerics.Vector2;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;
using XnaRectangle = Microsoft.Xna.Framework.Rectangle;

namespace PastryWorld.Editor.Animation;

public class AnimationCompositeRenderer
{
   
    private AnimationEditor _editor;
    private readonly AnimationTimelinePanel _timeline;
    private XnaVector2 _characterOrigin;
    private readonly Color _axisColor = new Color(255, 255, 255, 90);

    public AnimationCompositeRenderer(AnimationEditor editor)
    {
        _editor = editor;
    }

    public void DrawWorld(SpriteBatch spriteBatch, XnaRectangle visibleWorldBounds, Texture2D pixel)
    {
        if (_editor.Set == null) return;

        spriteBatch.Draw(pixel, visibleWorldBounds, _editor.CanvasBackgroundColor);
        DrawAxes(spriteBatch, visibleWorldBounds, pixel);

        if (_editor.SpriteSheetTexture == null) return;

        var clip = _editor.Set.GetActiveClip(_editor.CurrentDirection);
        int currentFrame = _timeline.CurrentFrame;

        if (_timeline.OnionSkinEnabled)
        {
            for (int i = 1; i <= _timeline.OnionFramesBefore; i++)
            {
                DrawComposite(spriteBatch, clip, currentFrame - i, new Color(2, 55, 100, 100) * 0.25f);

            }

            for (int i = 1; i <= _timeline.OnionFramesAfter; i++)
            {
                DrawComposite(spriteBatch, clip, currentFrame + i, new Color(100, 180, 255) * 0.25f);
            }
        }

        DrawComposite(spriteBatch, clip, currentFrame, Color.White);
    }
    private void DrawAxes(SpriteBatch spriteBatch, XnaRectangle visibleWorldBounds, Texture2D pixel)
    {
        spriteBatch.Draw(pixel, new XnaRectangle(0, visibleWorldBounds.Top, 1, visibleWorldBounds.Height), _axisColor);
        spriteBatch.Draw(pixel, new XnaRectangle(visibleWorldBounds.Left, 0, visibleWorldBounds.Width, 1), _axisColor);
    }

    private void DrawComposite(SpriteBatch spriteBatch, AnimationClip clip, int frame, Color tint)
    {
        if (frame < 0 || frame >= clip.FrameCount) return;

        var layers = new List <PartLayer>(clip.Layers);
        layers.Sort((a, b) => a.SortOrder.CompareTo(b.SortOrder));

        foreach (var layer in layers)
        {
            if (!layer.Visible) continue;

            var kf = GetHeldKeyframe(layer, frame);
            if (kf?.SpriteId == null) continue;

            var slice = _editor.ActiveManifest?.Sprites.Find(s => s.Id == kf.SpriteId);

            if (slice == null) continue;

            var effects = SpriteEffects.None; 
            if (kf.FlipX) effects |= SpriteEffects.FlipHorizontally;
            if (kf.FlipY) effects |= SpriteEffects.FlipVertically;

            var origin = new XnaVector2(slice.PivotX, slice.PivotY);
            var position = _characterOrigin + new XnaVector2(kf.X, kf.Y);

            spriteBatch.Draw(_editor.SpriteSheetTexture, position, slice.Rect, tint, kf.Rotation, origin, 1f, effects, 0f);
        }
    }
    private static PartKeyframe GetHeldKeyframe(PartLayer layer, int frame)
    {
        for (int f = frame; f >= 0; f--)
        {
            if (layer.Keyframes.TryGetValue(f, out var kf))
            {
                return kf;
            }
        }
        return null;
    }

}