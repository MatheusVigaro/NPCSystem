using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace NPCSystem;

public static class SoundRegistry
{
    private static string SoundsDirectory => Path.Combine(Plugin.BaseDirectory, "sounds");

    public static readonly Dictionary<SoundID, string> LoadedSounds = new();
    
    private static bool HooksApplied;

    public static SoundID GetSound(string soundID, string modID)
    {
        var keys = LoadedSounds.Keys;
        
        var result = keys.FirstOrDefault(x => x.value == Utils.GetSoundPrefix(modID) + soundID);
        result ??= keys.FirstOrDefault(x => x.value.EndsWith("|" + soundID));

        return result;
    }

    public static void Init()
    {
        if (!HooksApplied)
        {
            HooksApplied = true;
            
            //-- Injects our own sound files into the sound loading pipeline
            On.SoundLoader.CheckIfFileExistsAsExternal += SoundLoader_CheckIfFileExistsAsExternal;
            IL.SoundLoader.SoundImporter.reloadSounds += SoundImporter_reloadSounds;
            IL.SoundLoader.LoadSounds += SoundLoader_LoadSounds;
        }
        LoadSoundsFromDirectory(SoundsDirectory);
    }

    #region Hooks
    private static bool SoundLoader_CheckIfFileExistsAsExternal(On.SoundLoader.orig_CheckIfFileExistsAsExternal orig, SoundLoader self, string name)
    {
        return orig(self, name) || Utils.IsSoundOurs(name);
    }

    private static void SoundLoader_LoadSounds(ILContext il)
    {
        var cursor = new ILCursor(il);

        var loc = -1;
        cursor.GotoNext(MoveType.After,
            i => i.MatchLdstr("Sounds.txt"),              
            i => i.MatchCallOrCallvirt<string>(nameof(string.Concat)),
            i => i.MatchCallOrCallvirt<AssetManager>(nameof(AssetManager.ResolveFilePath)),
            i => i.MatchCallOrCallvirt("System.IO.File", nameof(File.ReadAllLines)),
            i => i.MatchStloc(out loc));

        cursor.MoveAfterLabels();
        cursor.Emit(OpCodes.Ldloc, loc);

        cursor.EmitDelegate((string[] strings) =>
        {
            var index = strings.Length;
            Array.Resize(ref strings, strings.Length + LoadedSounds.Count);
            
            foreach (var sound in LoadedSounds.Keys)
            {
                strings[index] = $"{sound.value} : {Utils.RemoveSoundPrefix(sound.value)}";
                index++;
            }

            return strings;
        });
        
        cursor.Emit(OpCodes.Stloc, loc);
    }

    private static void SoundImporter_reloadSounds(ILContext il)
    {
        var cursor = new ILCursor(il);

        cursor.GotoNext(MoveType.After, i => i.MatchCallOrCallvirt<AssetManager>(nameof(AssetManager.ListDirectory)));

        cursor.MoveAfterLabels();

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate((string[] strings, SoundLoader.SoundImporter self) =>
        {
            var index = strings.Length;
            Array.Resize(ref strings, strings.Length + LoadedSounds.Count);
            
            foreach (var file in LoadedSounds.Values)
            {
                strings[index] = file;
                index++;
            }
            
            return strings;
        });
    }
    #endregion

    public static void Reload()
    {
        foreach (var sound in LoadedSounds.Keys)
        {
            sound.Unregister();
        }

        LoadedSounds.Clear();

        Init();
    }

    private static void LoadSoundsFromDirectory(string directory)
    {
        try
        {
            var files = Utils.ListModDirectories(directory, includeAll: true).Where(x => ".wav".Equals(Path.GetExtension(x.Path), StringComparison.InvariantCultureIgnoreCase)).ToList();

            foreach (var fileData in files)
            {
                var file = fileData.Path;
                var name = Path.GetFileNameWithoutExtension(file);
                var prefix = Utils.GetSoundPrefix(fileData.ModID);

                LoadedSounds[new SoundID(prefix + name, true)] = file;
            }
        }
        finally
        {
            var subDirectories = Utils.ListModDirectories(directory, true, true).Distinct().ToList();
            foreach (var subDir in subDirectories)
            {
                LoadSoundsFromDirectory(subDir.Path);
            }
        }
    }
}