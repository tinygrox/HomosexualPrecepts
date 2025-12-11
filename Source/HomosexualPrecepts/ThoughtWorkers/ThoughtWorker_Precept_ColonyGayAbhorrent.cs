using System.Collections.Generic;
using RimWorld;
using Verse;

namespace HomosexualPrecepts.ThoughtWorkers;

public class ThoughtWorker_Precept_ColonyGayAbhorrent: ThoughtWorker_Precept
{
    protected override ThoughtState ShouldHaveThought(Pawn p)
    {
        if (!ModsConfig.IdeologyActive) return ThoughtState.Inactive;

        if(ThoughtUtility.ThoughtNullified(p, def)) return ThoughtState.Inactive;

        if (p.MapHeld == null) return ThoughtState.Inactive;

        List<Pawn> colonyPawnList = p.MapHeld.mapPawns.SpawnedPawnsInFaction(p.Faction);
        foreach (Pawn pawn in colonyPawnList)
        {
            if(pawn.gender != Gender.Male || pawn == p) continue;

            if(pawn.story is { traits: not null } && pawn.story.traits.HasTrait(TraitDefOf.Gay)) return ThoughtState.ActiveDefault;

            List<DirectPawnRelation> allLoveRelations = LovePartnerRelationUtility.ExistingLovePartners(pawn, false);
            foreach (DirectPawnRelation relation in allLoveRelations)
            {
                if(relation.otherPawn.gender != pawn.gender) continue;

                return ThoughtState.ActiveDefault;
            }
        }

        return ThoughtState.Inactive;
    }
}
