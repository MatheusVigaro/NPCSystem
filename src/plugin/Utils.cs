﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using RWCustom;

namespace NPCSystem;

public static class Utils
{
    private const string AtlasPrefix = "NPCSprite";

    public static string AtlasElementPrefix;
    
    public static FAtlas LoadAtlasOrImage(string name, string imagePath, string elementPrefix = null, string dataPath = "")
    {
        if (Futile.atlasManager.DoesContainAtlas(name))
        {
            return Futile.atlasManager.GetAtlasWithName(name);
        }

        var shouldLoadAsSingleImage = dataPath == "";

        if (shouldLoadAsSingleImage)
        {
            name = elementPrefix + name;
        }
        else
        {
            AtlasElementPrefix = elementPrefix;
        }
        
        FAtlas atlas;
        try
        {
            atlas = new FAtlas(name, imagePath, dataPath, FAtlasManager._nextAtlasIndex++, shouldLoadAsSingleImage);
        }
        finally
        {
            AtlasElementPrefix = null;
        }

        Futile.atlasManager.AddAtlas(atlas);
        return atlas;
    }
    
    public static List<ModPath> ListModDirectories(string path, bool directories = false, bool includeAll = false)
    {
        var result = new List<ModPath>();
        var uniquePaths = new List<string>();
        var modPaths = ModManager.ActiveMods.Select(mod => new ModPath(mod.id, mod.path)).ToList();
        foreach (var modPath in modPaths)
        {
            var currentPath = Path.Combine(modPath.Path, path.ToLowerInvariant());
            if (!Directory.Exists(currentPath))
            {
                continue;
            }
        
            var subPaths = directories ? Directory.GetDirectories(currentPath) : Directory.GetFiles(currentPath);
            
            foreach (var subPath in subPaths)
            {
                var fileName = Path.GetFileName(subPath);
                if (!uniquePaths.Contains(fileName) || includeAll)
                {
                    result.Add(new ModPath(modPath.ModID, subPath));
                    if (!includeAll)
                    {
                        uniquePaths.Add(fileName);
                    }
                }
            }
        }
        return result;
    }

    public static string GetAtlasPrefix(string modId) => $"{AtlasPrefix}_{modId}_";

    public class ModPath
    {
        public string ModID;
        public string Path;

        public ModPath(string modID, string path)
        {
            ModID = modID;
            Path = path;
        }
    }
}