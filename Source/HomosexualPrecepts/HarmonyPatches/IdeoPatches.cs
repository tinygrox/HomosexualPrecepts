using System.Collections.Generic;
using System.Linq;
using HomosexualPrecepts.Precepts;
using RimWorld;
using Verse;

namespace HomosexualPrecepts.HarmonyPatches;

public class IdeoPatches
{
    // public static void AddPreceptPostfix(Ideo __instance, Precept precept)
    // {
    //     if (precept is not Precept_ForcedTrait) return;
    //
    //     foreach (var forcedTrait in __instance.PreceptsListForReading.OfType<Precept_ForcedTrait>())
    //     {
    //         forcedTrait.Notify_IdeoReformed();
    //         Log.Message("forcedTrait.Notify_IdeoReformed();");
    //     }
    // }
    public static void RemovePreceptPrefix(Precept precept)
    {
        if (precept is Precept_ForcedTrait forcedTrait)
        {
            forcedTrait.Notify_RemovedByReforming();
        }
    }
    // public static void RecachePreceptsPostfix(Ideo __instance, List<Precept> ___precepts)
    // {
    //     foreach (var forcedTrait in __instance.PreceptsListForReading.OfType<Precept_ForcedTrait>())
    //     {
    //         // Log.Message("forcedTrait.Notify_RecachedPrecepts();");
    //         forcedTrait.Notify_RecachedPrecepts();
    //     }
    // }
}
