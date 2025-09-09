using Verse;

namespace StuffList;

/// <summary>
///     Definition of the settings for the mod
/// </summary>
internal class StuffListSettings : ModSettings
{
    public float ScreenWidth = 0.95f;

    /// <summary>
    ///     Saving and loading the values
    /// </summary>
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref ScreenWidth, "ScreenWidth", 0.95f);
    }
}