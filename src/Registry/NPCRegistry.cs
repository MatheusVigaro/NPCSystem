using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace NPCSystem;

public static class NPCRegistry
{
    private static string NPCsDirectory => Path.Combine(Plugin.BaseDirectory, "npcs");

    private static readonly List<NPC> LoadedNPCs = new();

    public static NPC GetNPC(string id) => LoadedNPCs.FirstOrDefault(x => x.ID.value.Equals(id));

    public static NPC GetNPC(NPCID id) => LoadedNPCs.FirstOrDefault(x => x.ID == id);

    public static void Init()
    {
        LoadNPCsFromDirectory(NPCsDirectory);
    }

    public static void Reload()
    {
        foreach (var npc in LoadedNPCs)
        {
            npc.ID.Unregister();
        }

        LoadedNPCs.Clear();

        Init();
    }

    private static void LoadNPCsFromDirectory(string directory)
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
                    var npc = JsonConvert.DeserializeObject<NPC>(text);
                    npc.ModID = fileData.ModID;
                    npc.Init();
                    LoadedNPCs.Add(npc);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error when parsing NPC file ({file}){Environment.NewLine}{StackTraceUtility.ExtractStringFromException(ex)}");
                }
            }
        }
        finally
        {
            var subDirectories = Utils.ListModDirectories(directory, true, true).Distinct().ToList();
            foreach (var subDir in subDirectories)
            {
                LoadNPCsFromDirectory(subDir.Path);
            }
        }
    }
}