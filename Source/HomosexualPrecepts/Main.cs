using HarmonyLib;
using HomosexualPrecepts.HarmonyPatches;
// using HomosexualPrecepts.HarmonyPatches;
using RimWorld;
using Verse;

namespace HomosexualPrecepts;

[StaticConstructorOnStartup]
public class Main
{
    private static readonly Harmony s_harmony = new Harmony("tinygrox.rimworldMods.HomosexualPrecepts");

    static Main()
    {
        s_harmony.Patch(
            AccessTools.Method(typeof(IdeoUIUtility), "DoPreceptsInt"),
            prefix: new HarmonyMethod(typeof(IdeoUIUtilityPatches), nameof(IdeoUIUtilityPatches.DoPreceptsIntPrefix))
        );
        // s_harmony.Patch(
        //     AccessTools.Method(typeof(Ideo), nameof(Ideo.AddPrecept)),
        //     postfix: new HarmonyMethod(typeof(IdeoPatches), nameof(IdeoPatches.AddPreceptPostfix))
        // );
        s_harmony.Patch(
            AccessTools.Method(typeof(Ideo), nameof(Ideo.RemovePrecept)),
            prefix: new HarmonyMethod(typeof(IdeoPatches), nameof(IdeoPatches.RemovePreceptPrefix))
        );
        // s_harmony.Patch(
        //     AccessTools.Method(typeof(Ideo), nameof(Ideo.RecachePrecepts)),
        //     postfix: new HarmonyMethod(typeof(IdeoPatches), nameof(IdeoPatches.RecachePreceptsPostfix))
        // );
        s_harmony.Patch(
            AccessTools.Method(typeof(Trait), nameof(Trait.TipString), new[] { typeof(Pawn) }),
            postfix: new HarmonyMethod(typeof(TraitPatches), nameof(TraitPatches.TipStringPostfix))
        );
        Log.Message("[HomosexualPrecepts] Harmony patches applied.");
    }
}
