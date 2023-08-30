using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fisobs.Core;
using Newtonsoft.Json;
using NPCSystem.Fisobs;
using UnityEngine;

namespace NPCSystem;

public static class ItemRegistry
{
    private static string ItemsDirectory => Path.Combine(Plugin.BaseDirectory, "items");

    public static readonly List<Item> LoadedItems = new();

    public static Item GetItem(string id) => LoadedItems.FirstOrDefault(x => x.ID.value.Equals(id));

    public static Item GetItem(ItemID id) => LoadedItems.FirstOrDefault(x => x.ID == id);

    public static void Init()
    {
        LoadItemsFromDirectory(ItemsDirectory);

        foreach (var item in LoadedItems)
        {
            Content.Register(new ItemFisob(item));
        }
    }

    //-- TODO: doesn't actually reload
    public static void Reload()
    {
        if (LoadedItems.Count > 0) return;

        foreach (var item in LoadedItems)
        {
            item.ID.Unregister();
        }

        LoadedItems.Clear();

        Init();
    }

    private static void LoadItemsFromDirectory(string directory)
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
                    var item = JsonConvert.DeserializeObject<Item>(text);
                    item.ModID = fileData.ModID;
                    item.Init();
                    LoadedItems.Add(item);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error when parsing item file ({file}){Environment.NewLine}{StackTraceUtility.ExtractStringFromException(ex)}");
                }
            }
        }
        finally
        {
            var subDirectories = Utils.ListModDirectories(directory, true, true).Distinct().ToList();
            foreach (var subDir in subDirectories)
            {
                LoadItemsFromDirectory(subDir.Path);
            }
        }
    }
}