using System;
using HomosexualPrecepts.Precepts;
using RimWorld;

namespace HomosexualPrecepts.HarmonyPatches;

public class IdeoUIUtilityPatches
{
    // 呸！傻逼泰南
    public static void DoPreceptsIntPrefix(bool mainPrecepts, ref Func<PreceptDef, bool> filter)
    {
        if (!mainPrecepts) return;

        var originalFilter = filter;
        filter = p => originalFilter(p) || p.preceptClass == typeof(Precept_ForcedTrait);
    }
}
