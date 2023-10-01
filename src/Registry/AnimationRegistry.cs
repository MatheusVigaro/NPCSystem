using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace NPCSystem;

public static class AnimationRegistry
{
    private static string AnimationsDirectory => Path.Combine(Plugin.BaseDirectory, "animations");

    private static readonly List<Animation> LoadedAnimations = new();

    public static Animation GetAnimation(string id) => LoadedAnimations.FirstOrDefault(x => x.ID.value.Equals(id));

    public static Animation GetAnimation(AnimationID id) => LoadedAnimations.FirstOrDefault(x => x.ID.value.Equals(id.value));

    public static void Init()
    {
        LoadAnimationsFromDirectory(AnimationsDirectory);
    }

    public static void Reload()
    {
        foreach (var animation in LoadedAnimations)
        {
            animation.ID.Unregister();
        }

        LoadedAnimations.Clear();

        Init();
    }

    private static void LoadAnimationsFromDirectory(string directory)
    {
        try
        {
            var files = Utils.ListModDirectories(directory, includeAll: true).Where(x => ".json".Equals(Path.GetExtension(x.Path), StringComparison.InvariantCultureIgnoreCase));

            foreach (var fileData in files)
            {
                var file = fileData.Path;
                var text = File.ReadAllText(file);

                try
                {
                    var animation = JsonConvert.DeserializeObject<Animation>(text);
                    animation.ModID = fileData.ModID;
                    animation.Init();
                    LoadedAnimations.Add(animation);
                }
                catch
                {
                    Debug.LogError($"Error when parsing animation file ({file})");
                }
            }
        }
        finally
        {
            var subDirectories = Utils.ListModDirectories(directory, true, true).Distinct().ToList();
            foreach (var subDir in subDirectories)
            {
                LoadAnimationsFromDirectory(subDir.Path);
            }
        }
    }
}