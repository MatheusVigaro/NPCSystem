using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace NPCSystem;

public static class ActionRegistry
{
    private static string ActionsDirectory => Path.Combine(Plugin.BaseDirectory, "actions");

    private static readonly List<Action> LoadedActions = new();

    public static Action GetAction(string id) => LoadedActions.FirstOrDefault(x => x.ID.value.Equals(id));

    public static Action GetAction(ActionID id) => LoadedActions.FirstOrDefault(x => x.ID.value.Equals(id.value));

    public static void Init()
    {
        LoadActionsFromDirectory(ActionsDirectory);
    }

    public static void Reload()
    {
        foreach (var action in LoadedActions)
        {
            action.ID.Unregister();
        }

        LoadedActions.Clear();

        Init();
    }

    private static void LoadActionsFromDirectory(string directory)
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
                    var action = JsonConvert.DeserializeObject<Action>(text);
                    action.ModID = fileData.ModID;
                    action.Init();
                    LoadedActions.Add(action);
                }
                catch
                {
                    Debug.LogError($"Error when parsing action file ({file})");
                }
            }
        }
        finally
        {
            var subDirectories = Utils.ListModDirectories(directory, true, true).Distinct().ToList();
            foreach (var subDir in subDirectories)
            {
                LoadActionsFromDirectory(subDir.Path);
            }
        }
    }
}