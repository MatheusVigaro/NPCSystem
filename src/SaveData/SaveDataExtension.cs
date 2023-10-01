namespace NPCSystem;

/// <summary>
/// Extensions to generate the <see cref="NPCSaveData"/> helper from the game's save data.
/// </summary>
public static class SaveDataExtension
{
    /// <summary>
    /// Gets a <see cref="NPCSaveData"/> from the game's <see cref="DeathPersistentSaveData"/>.
    /// </summary>
    /// <param name="data">The <see cref="DeathPersistentSaveData"/> instance.</param>
    /// <returns>A <see cref="NPCSaveData"/> bound to the <see cref="DeathPersistentSaveData"/>.</returns>
    public static NPCSaveData GetNPCSaveData(this DeathPersistentSaveData data)
    {
        return NPCSaveData.DeathPersistentData.GetValue(data, dpsd => new(dpsd.unrecognizedSaveStrings));
    }

    /// <summary>
    /// Gets a <see cref="NPCSaveData"/> from the game's <see cref="MiscWorldSaveData"/>.
    /// </summary>
    /// <param name="data">The <see cref="MiscWorldSaveData"/> instance.</param>
    /// <returns>A <see cref="NPCSaveData"/> bound to the <see cref="MiscWorldSaveData"/> instance.</returns>
    public static NPCSaveData GetNPCSaveData(this MiscWorldSaveData data)
    {
        return NPCSaveData.WorldData.GetValue(data, mwsd => new(mwsd.unrecognizedSaveStrings));
    }

    /// <summary>
    /// Gets a <see cref="NPCSaveData"/> from the game's <see cref="PlayerProgression.MiscProgressionData"/>.
    /// </summary>
    /// <param name="data">The <see cref="PlayerProgression.MiscProgressionData"/> instance.</param>
    /// <returns>A <see cref="NPCSaveData"/> bound to the <see cref="PlayerProgression.MiscProgressionData"/> instance.</returns>
    public static NPCSaveData GetNPCSaveData(this PlayerProgression.MiscProgressionData data)
    {
        return NPCSaveData.ProgressionData.GetValue(data, mpd => new(mpd.unrecognizedSaveStrings));
    }
}