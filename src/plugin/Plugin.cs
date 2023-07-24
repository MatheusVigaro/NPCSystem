using BepInEx;
using System.Security.Permissions;
using System.Security;
using System;
using UnityEngine;
using System.Linq;
using NPCSystem.DevTools;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace NPCSystem;

[BepInPlugin(MOD_ID, "NPCSystem", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    public const string MOD_ID = "vigaro.npcsystem";
    public const string BaseDirectory = "npc";

    public bool IsInit;
    public bool IsPreInit;
    public bool IsPostInit;

    private void OnEnable()
    {
        On.RainWorld.PreModsInit += RainWorld_PreModsInit;
        On.RainWorld.OnModsInit += RainWorld_OnModsInit;
        On.RainWorld.PostModsInit += RainWorld_PostModsInit;
    }

    private void RainWorld_PreModsInit(On.RainWorld.orig_PreModsInit orig, RainWorld self)
    {
        orig(self);
  
        try
        {
            if (IsPreInit) return;
            IsPreInit = true;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);

        try
        {
            if (IsInit) return;
            IsInit = true;
            
            NPCEnums.Init();
            Hooks.Apply();
            InitRegistries();
            
            Pom.Pom.RegisterManagedObject<NPCObject, NPCData, Pom.Pom.ManagedRepresentation>("NPC", "NPC");
            Pom.Pom.RegisterManagedObject<NPCTriggerZoneObject, NPCTriggerZoneData, Pom.Pom.ManagedRepresentation>("NPCTriggerZone", "NPC");
            
            ObjectSpawner.RegisterSafeSpawners();
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    private void RainWorld_PostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
    {
        orig(self);

        try
        {
            if (IsPostInit) return;
            IsPostInit = true;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public void ReloadRegistries()
    {
        SpriteRegistry.Reload();
        AnimationRegistry.Reload();
        NPCRegistry.Reload();
        ActionRegistry.Reload();
    }
    
    public void InitRegistries()
    {
        SpriteRegistry.Init();
        AnimationRegistry.Init();
        NPCRegistry.Init();
        ActionRegistry.Init();
    }
}