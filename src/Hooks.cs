using System.Linq;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using NPCSystem.DevTools;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NPCSystem;

public class Hooks
{
    public static void Apply()
    {
        //-- Prefix for atlas elements to prevent conflicts
        IL.FAtlas.LoadAtlasData += FAtlas_LoadAtlasData;
        
        //-- Making sure there isn't a leftover UI when starting a new game
        On.RainWorldGame.ctor += RainWorldGame_ctor;
        
        //-- Updating the PromptMenu
        On.RainWorldGame.Update += RainWorldGame_Update;
        On.RainWorldGame.GrafUpdate += RainWorldGame_GrafUpdate;
        
        //-- Disabling inputs while PromptMenu is active
        On.Player.checkInput += Player_checkInput;
        
        //-- Save data stuff
        SaveDataHooks.Apply();
        
        //-- Make the CustomItem DevTools object able to be consumed
        On.AbstractConsumable.IsTypeConsumable += AbstractConsumable_IsTypeConsumable;
        
        //-- Play NPC voices
        On.HUD.DialogBox.Update += DialogBox_Update;
        
        //-- Variable filter
        On.RoomSettings.LoadPlacedObjects += RoomSettings_LoadPlacedObjects;
    }

    private static void RoomSettings_LoadPlacedObjects(On.RoomSettings.orig_LoadPlacedObjects orig, RoomSettings self, string[] s, SlugcatStats.Name playerchar)
    {
        orig(self, s, playerchar);

        if (Custom.rainWorld.processManager.currentMainLoop is not RainWorldGame game || game.session is not StoryGameSession session) return;

        var saveData = session.saveState.miscWorldSaveData.GetNPCSaveData();
        var filters = self.placedObjects.Where(x => x.data is VariableFilterData && x.active).ToArray();

        foreach (var filter in filters)
        {
            var filterData = (VariableFilterData)filter.data;

            saveData.TryGet<string>(filterData.variable, out var savedValue);

            //-- Don't deactivate the objects if the condition is satisfied
            if (Utils.LogicOperation(filterData.operation, savedValue, filterData.value)) continue;

            foreach (var placedObj in self.placedObjects.Where(x => x.active && x.deactivattable && x.data is not VariableFilterData))
            {
                if (Custom.DistLess(placedObj.pos, filter.pos, filterData.size.x))
                {
                    placedObj.active = false;
                }
            }
        }
    }

    private static void DialogBox_Update(On.HUD.DialogBox.orig_Update orig, HUD.DialogBox self)
    {
        var lastCharacter = self.showCharacter;
        orig(self);

        if (self.showCharacter > lastCharacter &&
            self.CurrentMessage is MessageWithSound message &&
            message.soundID != null &&
            (message.currentSound == null || message.currentSound.Done) &&
            Custom.rainWorld.processManager.currentMainLoop is RainWorldGame game &&
            message.text.Substring(lastCharacter, self.showCharacter - lastCharacter).Any(char.IsLetterOrDigit))
        {
            var mic = game.cameras[0].virtualMicrophone;
            
            var soundData = mic.GetSoundData(message.soundID, -1);
            if (mic.SoundClipReady(soundData))
            {
                message.currentSound = new VirtualMicrophone.DisembodiedSound(mic, soundData, 0, 1, Random.Range(message.pitchMin, message.pitchMax), false, 0);
                mic.soundObjects.Add(message.currentSound);
            }
        }
    }

    private static bool AbstractConsumable_IsTypeConsumable(On.AbstractConsumable.orig_IsTypeConsumable orig, AbstractPhysicalObject.AbstractObjectType type)
    {
        return orig(type) || ItemRegistry.LoadedItems.Any(x => x.AbstractObjectType == type);
    }

    private static void RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager processManager)
    {
        PromptMenu.CurrentPrompt = null;

        orig(self, processManager);
    }


    private static void Player_checkInput(On.Player.orig_checkInput orig, Player self)
    {
        orig(self);
        if (PromptMenu.CurrentPrompt != null)
        {
            self.input[0] = default;
            self.input[1] = default;
        }
    }

    private static void RainWorldGame_GrafUpdate(On.RainWorldGame.orig_GrafUpdate orig, RainWorldGame self, float timestacker)
    {
        orig(self, timestacker);
        if (PromptMenu.CurrentPrompt != null)
        {
            PromptMenu.CurrentPrompt.GrafUpdate(timestacker);
        }
    }

    private static void RainWorldGame_Update(On.RainWorldGame.orig_Update orig, RainWorldGame self)
    {
        orig(self);
        if (PromptMenu.CurrentPrompt != null)
        {
            PromptMenu.CurrentPrompt.Update();
        }
    }

    private static void FAtlas_LoadAtlasData(ILContext il)
    {
        var cursor = new ILCursor(il);

        var loc = 0;
        try
        {
            cursor.GotoNext(MoveType.After, 
                i => i.MatchLdloc(out loc),
                i => i.MatchLdloc(out _),
                i => i.MatchStfld<FAtlasElement>(nameof(FAtlasElement.name)));
        }
        catch
        {
            Debug.LogError("Exception when matching IL for FAtlas_LoadAtlasData!");
            Debug.LogError(il);
            throw;
        }

        cursor.MoveAfterLabels();
        cursor.Emit(OpCodes.Ldloc, loc);
        cursor.EmitDelegate((FAtlasElement element) =>
        {
            if (!string.IsNullOrEmpty(Utils.AtlasElementPrefix))
            {
                element.name = Utils.AtlasElementPrefix + element.name;
            }
        });
    }
}