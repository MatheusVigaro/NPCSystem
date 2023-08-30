using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace NPCSystem;

public static class SpriteRegistry
{
    private static string AtlasesDirectory => Path.Combine(Plugin.BaseDirectory, "sprites");

    private static readonly List<FAtlas> LoadedAtlases = new();

    public static void Init()
    {
        LoadAtlasesFromDirectory(AtlasesDirectory);
    }

    public static void Reload()
    {
        foreach (var atlas in LoadedAtlases)
        {
            Futile.atlasManager.UnloadAtlas(atlas.name);
        }

        LoadedAtlases.Clear();

        Init();
    }

    private static void LoadAtlasesFromDirectory(string directory)
    {
        try
        {
            var files = Utils.ListModDirectories(directory, includeAll: true).Where(x => ".png".Equals(Path.GetExtension(x.Path), StringComparison.InvariantCultureIgnoreCase)).ToList();

            foreach (var fileData in files)
            {
                var file = fileData.Path;
                var fileName = Path.GetFileNameWithoutExtension(file);
                var fileNoExt = Path.ChangeExtension(file, null);
                var txtFile = Path.ChangeExtension(file, ".txt");
                var prefix = Utils.GetAtlasPrefix(fileData.ModID);

                var atlas = Utils.LoadAtlasOrImage(Utils.GetAtlasPrefix(fileData.ModID) + fileName, fileNoExt, prefix, File.Exists(txtFile) ? fileNoExt : "");
                LoadedAtlases.Add(atlas);
            }
        }
        finally
        {
            var subDirectories = Utils.ListModDirectories(directory, true, true).Distinct().ToList();
            foreach (var subDir in subDirectories)
            {
                LoadAtlasesFromDirectory(subDir.Path);
            }
        }
    }
}