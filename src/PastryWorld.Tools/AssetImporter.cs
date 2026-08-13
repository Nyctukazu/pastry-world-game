using System;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using System.Net.Mime;

namespace PastryWorld.Tools;

public static class AssetImporter
{
    public const string SpritesFolder = "Content/Sprites";
    public const string SfxFolder = "Content/sfx";

    public static string ImportPng(GraphicsDevice device, string sourceFilePath, out Texture2D texture)
    {
        Directory.CreateDirectory(SpritesFolder);
        string destPath = Path.Combine(SpritesFolder, Path.GetFileName(sourceFilePath));
        File.Copy(sourceFilePath, destPath, overwrite: true);

        using var stream = File.OpenRead(destPath);
        texture = Texture2D.FromStream(device, stream);
        return destPath;
    }

    public static string ImportSfx(string sourceFilePath, out SoundEffect sfx)
    {
        string ext = Path.GetExtension(sourceFilePath).ToLowerInvariant();
        if (ext != ".wav")
        {
            throw new NotSupportedException($"'{ext}' isn't directly loadable at runtime.  Convert to .wav first, or add an OGG/MP3 decoder step.");
        }

        Directory.CreateDirectory(SfxFolder);
        string destPath = Path.Combine(SfxFolder, Path.GetFileName(sourceFilePath));
        File.Copy(sourceFilePath, destPath, overwrite: true);

        using var stream = File.OpenRead(destPath);
        sfx = SoundEffect.FromStream(stream);
        return destPath;
    }
}