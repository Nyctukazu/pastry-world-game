using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using PastryWorld.Core.Animation;
using PastryWorld.Core.Level;
using PastryWorld.Editor.Commands;
using PastryWorld.Tools;


namespace PastryWorld.Editor.Animation;

public class AnimationManager
{

    private readonly AnimationSet _animationSet;
    private readonly CommandManager _commandManager;
    private readonly JsonAnimationSerializer _animSerializer;
    private string _animationName = "Untitled";
    public string AnimationName
    {
        get => _animationName;
        set => _animationName = value ?? "";
    }


    public List<string> AvailableAnimationFiles { get; } = new();
    public int SelectedAnimationIndex { get; set; } = 0;
    public string StatusMessage { get; private set; } = "";
    public bool IsStatusError { get; private set; } = false;

    public AnimationManager(AnimationSet animationSet, CommandManager commandManager, JsonAnimationSerializer animSerializer, string animationName)
    {
        _animationSet = animationSet;
        _commandManager = commandManager;
        _animSerializer = animSerializer;
        AnimationName = animationName;
    }
    public void RefreshAnimationList()
    {
        AvailableAnimationFiles.Clear();
        string AnimationDir = AnimationPathUtility.GetDirectory();

        Directory.CreateDirectory(AnimationDir);

        var files = Directory.GetFiles(AnimationDir, "*.json")
                            .Select(Path.GetFileNameWithoutExtension)
                            .Where(name => !string.IsNullOrWhiteSpace(name))
                            .Select(name => name!)
                            .ToList();

        AvailableAnimationFiles.AddRange(files);

        if (!string.IsNullOrWhiteSpace(AnimationName))
        {
            int index = AvailableAnimationFiles.IndexOf(AnimationName);
            if (index >= 0)
            {
                SelectedAnimationIndex = index;
            }
        }
    }
    public void SaveCurrentAnimation()
    {
        string cleanName = AnimationName.Trim();
        if (string.IsNullOrWhiteSpace(cleanName))
        {
            SetStatus("Cannot save: Animation Name is empty!", isError: true);
            return;
        }

        try
        {
            _animationSet.Name = cleanName;
            string savePath = AnimationPathUtility.GetFullPath(cleanName);
            _animSerializer.Save(_animationSet, savePath);
            

            RefreshAnimationList();
            SetStatus($"Saved: {cleanName}.json", isError: false);
        }
        catch (Exception ex)
        {
            SetStatus($"Save Failed: {ex.Message}", isError: true);
        }
    }

    public bool LoadAnimation(string animationName)
    {
        if (string.IsNullOrWhiteSpace(animationName)) return false;

        try
        {
            string loadPath = AnimationPathUtility.GetFullPath(animationName);
            AnimationSet loadedAnimation = _animSerializer.Load(loadPath);

            _animationSet.CopyFrom(loadedAnimation);
            AnimationName = animationName;
            _commandManager.Clear();

            SetStatus($"Loaded: {animationName}.json", isError: false);
            return true;
        }
        catch (Exception ex)
        {
            SetStatus($"Load Failed: {ex.Message}", isError: true);
            return false;
        }
    }

    private void SetStatus(string message, bool isError)
    {
        StatusMessage = message;
        IsStatusError = isError;
    }
}