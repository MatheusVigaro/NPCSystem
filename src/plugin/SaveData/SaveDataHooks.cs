﻿namespace NPCSystem;

internal static class SaveDataHooks
{
    public static void Apply()
    {
        On.DeathPersistentSaveData.ToString += DeathPersistentSaveData_ToString;
        On.MiscWorldSaveData.ToString += MiscWorldSaveData_ToString;
        On.PlayerProgression.MiscProgressionData.ToString += MiscProgressionData_ToString;
    }

    private static string MiscProgressionData_ToString(On.PlayerProgression.MiscProgressionData.orig_ToString orig, PlayerProgression.MiscProgressionData self)
    {
        self.GetNPCSaveData().SaveToStrings(self.unrecognizedSaveStrings);
        return orig(self);
    }

    private static string MiscWorldSaveData_ToString(On.MiscWorldSaveData.orig_ToString orig, MiscWorldSaveData self)
    {
        self.GetNPCSaveData().SaveToStrings(self.unrecognizedSaveStrings);
        return orig(self);
    }

    private static string DeathPersistentSaveData_ToString(On.DeathPersistentSaveData.orig_ToString orig, DeathPersistentSaveData self)
    {
        self.GetNPCSaveData().SaveToStrings(self.unrecognizedSaveStrings);
        return orig(self);
    }
}