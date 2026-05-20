using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RitualOutcomeComp_RitualTargetDefs : RitualOutcomeComp_QualitySingleOffset
{
	[MustTranslate]
	public string expectedThingLabelTip;

	public List<ThingDef> defs;

	public bool allowAltars;

	public bool autoPoweredNeedsToBeOn;

	public bool autoApplyInClassicMode;

	public override bool Applies(LordJob_Ritual ritual)
	{
		if (autoApplyInClassicMode && Find.IdeoManager.classicMode)
		{
			return true;
		}
		Thing thing = ritual.selectedTarget.Thing;
		if (thing == null)
		{
			return false;
		}
		if (!CheckIdeoCorrectness(ritual?.Ritual, thing))
		{
			return false;
		}
		if (allowAltars && thing.def.isAltar)
		{
			return true;
		}
		if (!defs.NullOrEmpty())
		{
			return defs.Contains(ritual.selectedTarget.Thing?.def);
		}
		return false;
	}

	protected bool CheckIdeoCorrectness(Precept_Ritual ritual, Thing thing)
	{
		Ideo ideo = ritual?.ideo;
		if (thing.StyleSourcePrecept != null)
		{
			if (ideo != null)
			{
				return thing.StyleSourcePrecept.ideo == ideo;
			}
			return false;
		}
		return true;
	}

	public override QualityFactor GetQualityFactor(Precept_Ritual ritual, TargetInfo ritualTarget, RitualObligation obligation, RitualRoleAssignments assignments, RitualOutcomeComp_Data data)
	{
		if (autoApplyInClassicMode && Find.IdeoManager.classicMode)
		{
			return new QualityFactor
			{
				quality = qualityOffset
			};
		}
		Thing thing = ritualTarget.Thing;
		if (thing == null)
		{
			return null;
		}
		bool flag = CheckIdeoCorrectness(ritual, thing) && ((allowAltars && thing.def.isAltar) || (!defs.NullOrEmpty() && defs.Contains(ritualTarget.Thing.def)));
		TaggedString taggedString = "RitualOutcomeCompTip_RitualTargetDefsPart1".Translate(ritual.LabelCap, expectedThingLabelTip);
		if (!flag)
		{
			taggedString += "\n\n" + "RitualOutcomeCompTip_RitualTargetDefsPart2".Translate(ritual.LabelCap, thing);
		}
		return new QualityFactor
		{
			label = label.CapitalizeFirst(),
			qualityChange = ExpectedOffsetDesc(flag, -1f),
			positive = flag,
			present = flag,
			quality = (flag ? qualityOffset : 0f),
			priority = 2f,
			toolTip = taggedString
		};
	}

	public override IEnumerable<string> BlockingIssues(Precept_Ritual ritual, TargetInfo target, RitualRoleAssignments assignments)
	{
		Thing thing = target.Thing;
		if (thing != null && autoPoweredNeedsToBeOn)
		{
			CompAutoPowered compAutoPowered = thing.TryGetComp<CompAutoPowered>();
			if (compAutoPowered != null && !compAutoPowered.AppearsPowered)
			{
				yield return "RitualOutcomeBlockedNoPower".Translate(thing.LabelShort).CapitalizeFirst();
			}
		}
	}

	protected override string ExpectedOffsetDesc(bool positive, float quality = -1f)
	{
		quality = ((quality == -1f) ? qualityOffset : quality);
		if (!positive)
		{
			return "";
		}
		return quality.ToStringWithSign("0.#%");
	}
}
