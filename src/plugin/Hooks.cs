using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;

namespace NPCSystem;

public class Hooks
{
    public static void Apply()
    {
        IL.FAtlas.LoadAtlasData += FAtlas_LoadAtlasData;
        
        On.RainWorldGame.Update += RainWorldGame_Update;
        On.RainWorldGame.GrafUpdate += RainWorldGame_GrafUpdate;
        
        On.Player.checkInput += Player_checkInput;
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