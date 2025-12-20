using System.Collections.Generic;
using HomosexualPrecepts.DefModExtensions;
using RimWorld;
using Verse;

namespace HomosexualPrecepts.ThoughtWorkers;

public class ThoughtWorker_Orientation_Social: ThoughtWorker_Precept_Social
{
    protected override ThoughtState ShouldHaveThought(Pawn p, Pawn otherPawn)
    {
        if (!ModsConfig.IdeologyActive) return ThoughtState.Inactive;
        // 查找 nullifyingTraits 自己如果已经是了，就无视这个想法
        if(ThoughtUtility.ThoughtNullified(p, def)) return ThoughtState.Inactive;

        ThoughtConfigModExtension modDefExtension = def.GetModExtension<ThoughtConfigModExtension>();

        // 如果违反了戒律就生效
        if (IsViolate(otherPawn, modDefExtension))
        {
            return ThoughtState.ActiveDefault;
        }

        return ThoughtState.Inactive;
    }
    // 通用判断方法，只针对 opinion 不针对 mood
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
