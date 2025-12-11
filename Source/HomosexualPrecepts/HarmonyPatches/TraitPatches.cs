using System.Collections.Generic;
using System.Text;
using HomosexualPrecepts.Precepts;
using RimWorld;
using Verse;

namespace HomosexualPrecepts.HarmonyPatches;

public class TraitPatches
{
    public static void TipStringPostfix(Trait __instance, Pawn pawn, ref string __result)
    {
        if(__instance.sourceGene != null) return;

        List<Precept> pawnIdeoPreceptsList = pawn?.ideo?.Ideo?.PreceptsListForReading;
        if (pawnIdeoPreceptsList == null) return;

        foreach (Precept precept in pawnIdeoPreceptsList)
        {
            if (precept is not Precept_ForcedTrait preceptForcedTrait)
            {
                continue;
            }

            if(preceptForcedTrait.TrackedTraits is not { Count: > 0 }) continue;

            if (!preceptForcedTrait.TrackedTraits.TryGetValue(pawn.thingIDNumber, out var trackedTrait))
            {
                continue;
            }

            if (trackedTrait != __instance.def)
            {
                continue;
            }

            StringBuilder sb = new StringBuilder(__result);
            sb.AppendLine().AppendLine("HSP_AddedByPrecept".Translate().Colorize(ColoredText.TipSectionTitleColor) + ": " + (preceptForcedTrait.def.issue.LabelCap + ": " + preceptForcedTrait.def.LabelCap).Colorize(ColoredText.FactionColor_Ally));
            __result = sb.ToString();
        }
    }
}
