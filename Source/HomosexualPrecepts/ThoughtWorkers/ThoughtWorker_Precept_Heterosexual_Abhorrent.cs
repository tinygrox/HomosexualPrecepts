using System.Collections.Generic;
using RimWorld;
using Verse;

namespace HomosexualPrecepts.ThoughtWorkers;

public class ThoughtWorker_Precept_Heterosexual_Abhorrent: ThoughtWorker_Precept_Social
{
    protected override ThoughtState ShouldHaveThought(Pawn p, Pawn otherPawn)
    {
        if (!ModsConfig.IdeologyActive) return ThoughtState.Inactive;

        if(ThoughtUtility.ThoughtNullified(p, def)) return ThoughtState.Inactive;

        if (otherPawn.story?.traits != null)
        {
            bool hasBisexualTrait = otherPawn.story.traits.HasTrait(TraitDefOf.Bisexual);
            bool hasGayTrait = otherPawn.story.traits.HasTrait(TraitDefOf.Gay);
            bool hasAsexualTrait = otherPawn.story.traits.HasTrait(TraitDefOf.Asexual);
            if (!hasGayTrait && !hasBisexualTrait && !hasAsexualTrait)
            {
                return ThoughtState.ActiveDefault;
            }
        }

        List<DirectPawnRelation> allLoveRelations = LovePartnerRelationUtility.ExistingLovePartners(otherPawn, allowDead: false);

        foreach (DirectPawnRelation relation in allLoveRelations)
        {
            if(relation.otherPawn.gender == otherPawn.gender) continue;

            return ThoughtState.ActiveDefault;
        }

        return ThoughtState.Inactive;
    }
}
