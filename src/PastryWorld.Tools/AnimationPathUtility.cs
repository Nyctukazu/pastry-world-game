
using System;
using System.IO;

namespace PastryWorld.Tools;

public class AnimationPathUtility
{
    public const string AnimationExtension = ".json";

    public static string GetDirectory()
    {
    #if DEBUG
        string projectSourceDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Data", "Animations", "Characters"));
        Directory.CreateDirectory(projectSourceDir);
        return projectSourceDir;
    #else
        string AnimationDir = Path.Combine(AppDoman.CurrentDomain.BaseDirectory, "Data", "Animations", "Characters");
        Directory.CreateDirectory(AnimationDir);
        return AnimationDir;

    #endif
    }

    public static string GetFullPath(string animationName)
    {
        if (string.IsNullOrWhiteSpace(animationName))
        {
            throw new ArgumentException("Animation name cannot be null or empty.", nameof(animationName));
        }

        string fileNameOnly = Path.GetFileNameWithoutExtension(animationName);

        string sanitizedName = string.Join("-", fileNameOnly.Split(Path.GetInvalidFileNameChars()));

            sanitizedName += AnimationExtension;
        
        return Path.Combine(GetDirectory(), sanitizedName);
    }

}