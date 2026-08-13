using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PastryWorld.Core.Sprite;

public class SpriteSlice
{
    public string Id { get; set;}
    public Rectangle Rect { get; set; }
    public int PivotX { get; set; }
    public int PivotY { get; set; }
}
