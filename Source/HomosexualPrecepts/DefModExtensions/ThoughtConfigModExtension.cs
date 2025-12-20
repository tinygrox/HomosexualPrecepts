using System.Collections.Generic;
using RimWorld;
using Verse;

namespace HomosexualPrecepts.DefModExtensions;

public class ThoughtConfigModExtension: DefModExtension
{
    // 针对哪个性别
    public Gender genderRequirement = Gender.None; // Gender.Male;
    // 厌恶哪些 Trait
    public List<TraitDef> traitsAbhorrent = new List<TraitDef>();
    // 哪些 Trait 可以接受
    public List<TraitDef> traitsAccepted = new List<TraitDef>();
    // 厌恶同性伴侣
    public bool sameSexAbhorrent = false;
    // 厌恶异性伴侣
    public bool heterosexualAbhorrent = false;

    public override IEnumerable<string> ConfigErrors()
    {
        if (sameSexAbhorrent && heterosexualAbhorrent)
        {
            yield return "[ThoughtConfigModExtension] 'sameSexAbhorrent' and 'heterosexualAbhorrent' cannot BOTH be true.";
        }

        if (traitsAbhorrent == null || traitsAccepted == null)
        {
            yield break;
        }

        foreach (var trait in traitsAbhorrent)
        {
            if (traitsAccepted.Contains(trait))
            {
                yield return $"[ThoughtConfigModExtension] Trait '{trait.defName}' is present in both 'traitsAbhorrent' and 'traitsAccepted'. It cannot be both.";
            }
        }
    }
}
