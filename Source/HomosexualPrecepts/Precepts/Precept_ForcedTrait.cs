using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HomosexualPrecepts.PreceptComps;
using RimWorld;
using Verse;

namespace HomosexualPrecepts.Precepts;

public class Precept_ForcedTrait : Precept
{
    private PreceptComp_ForceMemberTrait _cachedComp;
    private Dictionary<int, TraitDef> _trackedTraits = new Dictionary<int, TraitDef>();

    public Dictionary<int, TraitDef> TrackedTraits => _trackedTraits;

    private PreceptComp_ForceMemberTrait ForcedTraitComp
    {
        get
        {
            _cachedComp ??= def.comps?.OfType<PreceptComp_ForceMemberTrait>().FirstOrDefault();

            return _cachedComp;
        }
    }

    public override string GetTip()
    {
        if(ForcedTraitComp == null) return base.GetTip();

        StringBuilder sb = new StringBuilder(base.GetTip());
        if (ForcedTraitComp.addTrait != null)
        {
            sb.AppendLine();
            sb.AppendInNewLine(ColorizeDescTitle("ForcedTraits".Translate()) + ":");
            sb.AppendInNewLine("  - " + ForcedTraitComp.addTrait.DataAtDegree(ForcedTraitComp.degree).LabelCap);
        }
        return sb.ToString();
    }

    public override IEnumerable<FloatMenuOption> EditFloatMenuOptions()
    {

        var otherPrecepts = DefDatabase<PreceptDef>.AllDefs
            .Where(p => p.issue == def.issue && p != def);

        foreach (PreceptDef p in otherPrecepts)
        {
            AcceptanceReport acceptanceReport = ideo.CanAddPreceptAllFactions(p);
            if (acceptanceReport.Accepted || !string.IsNullOrWhiteSpace(acceptanceReport.Reason))
            {
                string label = p.LabelCap;
                Action action = null;
                if (acceptanceReport.Accepted)
                {
                    action = () =>
                    {
                        var precept = PreceptMaker.MakePrecept(p);
                        if (precept is not Precept_ForcedTrait pf)
                        {
                            return;
                        }

                        // 保证字典能代代相传
                        // Log.Message($"0action => {def.defName} ---> {pf.def.defName}");
                        pf._trackedTraits = this._trackedTraits.ToDictionary(entry => entry.Key, entry => entry.Value);
                        // Log.Message($"2action => {_trackedTraits.Count} ---> {pf._trackedTraits.Count}");
                        ideo.RemovePrecept(this, true);
                        // Log.Message($"1action => {_trackedTraits.Count} ---> {pf._trackedTraits.Count}");
                        ideo.AddPrecept(pf, true);
                        // Log.Message($"3action => {_trackedTraits.Count} ---> {pf._trackedTraits.Count}");

                    };
                }
                else
                {
                    label += " (" + acceptanceReport.Reason + ")";
                }
                yield return new FloatMenuOption(label, action, p.Icon, IdeoUIUtility.GetIconAndLabelColor(p.impact));
            }
        }
    }
    // 对非玩家阵营的 SetIdeo 不会触发此方法
    public override void Notify_MemberLost(Pawn pawn)
    {
        base.Notify_MemberLost(pawn);
        // 死亡时不移除
        if (pawn.Dead && pawn.ideo?.Ideo == ideo) return;

        TryRemoveTrackedTrait(pawn);
    }

    // public override void Notify_MemberChangedFaction(Pawn p, Faction oldFaction, Faction newFaction)
    // {
    //     base.Notify_MemberChangedFaction(p, oldFaction, newFaction);
    // }

    public override void Notify_MemberGained(Pawn pawn)
    {
        base.Notify_MemberGained(pawn);
        EvaluatePawnTrait(pawn);
    }

    public override void Notify_MemberGuestStatusChanged(Pawn pawn)
    {
        base.Notify_MemberGuestStatusChanged(pawn);
        EvaluatePawnTrait(pawn);
    }

    public override void Notify_IdeoReformed()
    {
        base.Notify_IdeoReformed();
        SyncAllPawns();
    }

    public override void Notify_MemberGenerated(Pawn pawn, bool newborn, bool ignoreApparel = false)
    {
        base.Notify_MemberGenerated(pawn, newborn, ignoreApparel);
        EvaluatePawnTrait(pawn);
    }

    public override void Notify_RecachedPrecepts()
    {
        base.Notify_RecachedPrecepts();
        // Log.Message($"{def.defName}Precept_ForcedTrait::Notify_RecachedPrecepts()");
        SyncAllPawns();
    }

    public override void Notify_RemovedByReforming()
    {
        base.Notify_RemovedByReforming();
        RemoveTraitFromAllTrackedPawns();
        // SyncAllPawns();
    }

    public override void Notify_MemberCorpseDestroyed(Pawn p)
    {
        base.Notify_MemberCorpseDestroyed(p);
        TryRemoveTrackedTrait(p);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref _trackedTraits, "trackedTraits", LookMode.Value, LookMode.Def);
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            _trackedTraits ??= new Dictionary<int, TraitDef>();
        }
    }

    public override void CopyTo(Precept other)
    {
        // Log.Message($"{def.defName}Precept_ForcedTrait::CopyTo();");
        base.CopyTo(other);
        if (other is Precept_ForcedTrait otherForced)
        {
            otherForced._trackedTraits = _trackedTraits.ToDictionary(entry => entry.Key, entry => entry.Value);
            // Log.Message($"[{def.defName}]_trackedTraits.Count: {_trackedTraits.Count} Precept_ForcedTrait.CopyTo() => otherForced({otherForced.def.defName})._trackedTraits.Count: {otherForced._trackedTraits.Count}");
        }
    }

    private void SyncAllPawns()
    {
        // Log.Message($"[{def.defName}] Precept_ForcedTrait.SyncAllPawns() _trackedTraits.Count: {_trackedTraits.Count}");//, Stack: {new System.Diagnostics.StackTrace()}");
        List<Pawn> allPawns = PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead.Where(x => x.ideo?.Ideo == ideo).ToList();

        foreach (Pawn pawn in allPawns)
        {
            EvaluatePawnTrait(pawn);
        }
    }


    private void EvaluatePawnTrait(Pawn pawn)
    {
        if (pawn?.story?.traits == null) return;
        // Log.Message($"[{def.defName}] {pawn.thingIDNumber}: {pawn.Name}");
        // ForcedTraitComp 为空就意味着没有强制特性，把字典里的全部清掉
        if (ForcedTraitComp == null)
        {
            // Log.Message($"[{def.defName}] ForcedTraitComp == null");
            if (_trackedTraits.TryGetValue(pawn.thingIDNumber, out TraitDef traitDef))
            {
                // Log.Message($"[{def.defName}] ForcedTraitComp == null -> {pawn.NameFullColored}: {traitDef.DataAtDegree(0).LabelCap}");
                RemoveSpecificTrait(pawn, traitDef);
                _trackedTraits.Remove(pawn.thingIDNumber);
            }
            return;
        }

        bool hasTrait = pawn.story.traits.HasTrait(ForcedTraitComp.addTrait);
        // 先查一下字典是否符合当前的情况
        if (_trackedTraits.ContainsKey(pawn.thingIDNumber))
        {
            // 有记录但里面的 Trait 不等于当前 TraitComp 的 addTrait，那就删掉
            if (_trackedTraits[pawn.thingIDNumber].defName != ForcedTraitComp.addTrait.defName)
            {
                var trait = _trackedTraits[pawn.thingIDNumber];
                // Log.Message($"[{def.defName}] {pawn.Name}: {_trackedTraits[pawn.thingIDNumber]} addTrait: {ForcedTraitComp.addTrait.DataAtDegree(ForcedTraitComp.degree).LabelCap}");
                // TryRemoveTrackedTrait(pawn);
                RemoveSpecificTrait(pawn, trait);
                // Log.Message($"[{def.defName}] HasTrait: {pawn.story.traits.HasTrait(trait)}");
                _trackedTraits.Remove(pawn.thingIDNumber);
            }
        }


        // 有记录但是莫名其妙没加上 Trait，补加 Trait
        if (_trackedTraits.ContainsKey(pawn.thingIDNumber) && !hasTrait && pawn.gender != ForcedTraitComp.notForGender)
        {
            pawn.story.traits.GainTrait(new Trait(ForcedTraitComp.addTrait, ForcedTraitComp.degree));
            // Log.Message($"[{def.defName}] ContainsKey but none Trait, add Trait");
            return;
        }

        // 没有记录但是已经有 Trait 了，那你就是社区需要的人才，不用记录
        if (hasTrait) return;

        // 没有记录，没有 Trait，但是性别不符
        if (pawn.gender == ForcedTraitComp.notForGender) return;

        // 没有记录，但是例外人群，同是社区需要的人才，不用记录
        if (ForcedTraitComp.nullifyingTraits != null && ForcedTraitComp.nullifyingTraits.Any(t => t.HasTrait(pawn)))
        {
            return;
        }

        // 接下来就是没有记录的，也没有 Trait 的，那要被受戒了
        // Log.Message($"[{def.defName}] {pawn.Name} add Trait: {ForcedTraitComp.addTrait.DataAtDegree(0).LabelCap})");
        pawn.story.traits.GainTrait(new Trait(ForcedTraitComp.addTrait, ForcedTraitComp.degree));
        _trackedTraits[pawn.thingIDNumber] = ForcedTraitComp.addTrait;
    }

    private void TryRemoveTrackedTrait(Pawn pawn)
    {
        if (pawn?.story?.traits == null) return;

        if (!_trackedTraits.TryGetValue(pawn.thingIDNumber, out TraitDef traitDef)) return;

        RemoveSpecificTrait(pawn, traitDef);
        _trackedTraits.Remove(pawn.thingIDNumber);
    }

    private void RemoveSpecificTrait(Pawn pawn, TraitDef traitDef)
    {
        if (pawn?.story?.traits == null) return;

        // Log.Message($"[{def.defName}] traitDef: {traitDef.defName}");
        Trait existingTrait = pawn.story.traits.GetTrait(traitDef);
        if (existingTrait != null)
        {
            // Log.Message($"[{def.defName}] existing trait: {existingTrait.def.DataAtDegree(0).LabelCap}");
            pawn.story.traits.RemoveTrait(existingTrait);
        }
    }

    private void RemoveTraitFromAllTrackedPawns()
    {
        var allPawns = PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead;
        foreach (Pawn pawn in allPawns)
        {
            if (_trackedTraits.ContainsKey(pawn.thingIDNumber))
            {
                RemoveSpecificTrait(pawn, _trackedTraits[pawn.thingIDNumber]);
                // Log.Warning($"[{def.defName}]SyncAllPawns: Remove in _trackedTraits: {pawn.Name}'s {_trackedTraits[pawn.thingIDNumber].DataAtDegree(0).LabelCap}");
                _trackedTraits.Remove(pawn.thingIDNumber);
            }
        }
        _trackedTraits.Clear();

        // foreach (var kvp in _trackedTraits.ToList())
        // {
        //     Pawn pawn = .FirstOrDefault(p => p.thingIDNumber == kvp.Key);
        //     if (pawn != null)
        //     {
        //         RemoveSpecificTrait(pawn, kvp.Value);
        //         Log.Warning($"[{def.defName}]SyncAllPawns: Remove in _trackedTraits: {pawn.Name}'s {_trackedTraits[pawn.thingIDNumber].DataAtDegree(0).LabelCap}");
        //         _trackedTraits.Remove(kvp.Key);
        //     }
        // }
    }
}
