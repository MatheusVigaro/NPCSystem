using System.Linq;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using UnityEngine;

namespace NPCSystem;

public class Hooks
{
    public static void Apply()
    {
        //-- Prefix for atlas elements to prevent conflicts
        IL.FAtlas.LoadAtlasData += FAtlas_LoadAtlasData;
        
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