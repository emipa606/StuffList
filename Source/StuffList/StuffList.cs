using RimWorld;
using Verse;

namespace StuffList;

[StaticConstructorOnStartup]
public static class StuffList
{
    public static readonly bool SoftWarmBedsLoaded;

    static StuffList()
    {
        SoftWarmBedsLoaded = DefDatabase<StatDef>.GetNamedSilentFail("Textile_Softness") != null;
    }
}