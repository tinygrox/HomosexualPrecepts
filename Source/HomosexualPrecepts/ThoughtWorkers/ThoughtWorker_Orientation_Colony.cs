using System.Collections.Generic;
using HomosexualPrecepts.DefModExtensions;
using RimWorld;
using Verse;

namespace HomosexualPrecepts.ThoughtWorkers;

public class ThoughtWorker_Orientation_Colony: ThoughtWorker_Precept
{
    protected override ThoughtState ShouldHaveThought(Pawn p)
    {
        if (!ModsConfig.IdeologyActive) return ThoughtState.Inactive;

        if(ThoughtUtility.ThoughtNullified(p, def)) return ThoughtState.Inactive;

        if (p.MapHeld == null) return ThoughtState.Inactive;

        ThoughtConfigModExtension modDefExtension = def.GetModExtension<ThoughtConfigModExtension>();
        List<Pawn> colonyPawnList = p.MapHeld.mapPawns.SpawnedPawnsInFaction(p.Faction);

        foreach (Pawn pawn in colonyPawnList)
        {
            if(pawn == p) continue; // 不能是我自己吧

            if (IsViolate(pawn, modDefExtension)) // 只对其他人进行成分判定
            {
                return ThoughtState.ActiveDefault;
            }
        }

        return ThoughtState.Inactive;
    }

    private bool IsViolate(Pawn pawn, ThoughtConfigModExtension modConfig)
    {
        if (modConfig == null) return false;

        if(pawn.story?.traits == null) return false;

        if (modConfig.genderRequirement != Gender.None && pawn.gender != modConfig.genderRequirement) return false;

        if (modConfig.traitsAbhorrent.Count > 0)
        {
            foreach (TraitDef traitDef in modConfig.traitsAbhorrent)
            {
                if (pawn.story.traits.HasTrait(traitDef))
                {
                    return true;
                }
            }
        }

        if (modConfig.traitsAccepted.Count > 0)
        {
            foreach (TraitDef traitDef in modConfig.traitsAccepted)
            {
                if (pawn.story.traits.HasTrait(traitDef))
                    return false;
            }
        }

        // 单身异性恋的判断
        if (modConfig.heterosexualAbhorrent && modConfig.traitsAccepted.Count > 0)
        {
            // 能到这一步说明不是 gay
            return true;
        }

        List<DirectPawnRelation> allLoveRelations = LovePartnerRelationUtility.ExistingLovePartners(pawn, allowDead: false);

        foreach (DirectPawnRelation relation in allLoveRelations)
        {
            if (modConfig.sameSexAbhorrent && relation.otherPawn.gender == pawn.gender)
            {
                return true;
            }

            if (modConfig.heterosexualAbhorrent && relation.otherPawn.gender != pawn.gender)
            {
                return true;
            }
        }

        return false;
    }
}
