using System.Collections.Generic;
using System.Linq;
using HomosexualPrecepts.Precepts;
using RimWorld;
using Verse;

namespace HomosexualPrecepts.PreceptComps;

public class PreceptComp_ForceMemberTrait : PreceptComp
{
    public TraitDef addTrait;
    public int degree = 0;
    public Gender notForGender = Gender.None; // Gender.Male; Gender.Female;
    public List<TraitRequirement> nullifyingTraits;

    public override IEnumerable<TraitRequirement> TraitsAffecting
    {
        get
        {
            if(nullifyingTraits == null) yield break;

            foreach (TraitRequirement nullifyingTrait in nullifyingTraits)
            {
                yield return nullifyingTrait;
            }
        }
    }

    public override IEnumerable<string> ConfigErrors(PreceptDef parent)
    {
        foreach (string error in base.ConfigErrors(parent))
        {
            yield return error;
        }

        if (addTrait == null)
        {
            yield return "PreceptComp_ForceMemberTrait: addTrait is null";
        }

        if (parent.preceptClass != typeof(Precept_ForcedTrait))
        {
            yield return $"PreceptComp_ForceMemberTrait: {parent.defName}'s preceptClass is not Precept_ForcedTrait.";
        }
    }
}
