using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class RitualOutcomeEffectWorker_FromQuality : RitualOutcomeEffectWorker
{
	public static FloatRange ProgressToQualityMapping = new FloatRange(0.25f, 1f);

	public override bool SupportsAttachableOutcomeEffect => def.allowAttachableOutcome;

	public virtual bool GivesDevelopmentPoints => def.givesDevelopmentPoints;

	public RitualOutcomeEffectWorker_FromQuality()
	{
	}

	public RitualOutcomeEffectWorker_FromQuality(RitualOutcomeEffectDef def)
		: base(def)
	{
	}

	public virtual RitualOutcomePossibility GetOutcome(float quality, LordJob_Ritual ritual)
	{
		return def.outcomeChances.Where((RitualOutcomePossibility o) => OutcomePossible(o, ritual)).RandomElementByWeight((RitualOutcomePossibility c) => ChanceWithQuality(c, quality));
	}

	public virtual float GetOutcomeChanceAtQuality(LordJob_Ritual ritual, RitualOutcomePossibility outcome, float quality)
	{
		float num = def.outcomeChances.Where((RitualOutcomePossibility o) => OutcomePossible(o, ritual)).Sum((RitualOutcomePossibility c) => ChanceWithQuality(c, quality));
		return ChanceWithQuality(outcome, quality) / num;
	}

	protected static float ChanceWithQuality(RitualOutcomePossibility outcome, float quality)
	{
		if (!outcome.Positive)
		{
			return outcome.chance;
		}
		return Mathf.Max(outcome.chance * quality, 0f);
	}

	protected virtual void ApplyAttachableOutcome(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, RitualOutcomePossibility outcomeChance, out string extraLetterText, ref LookTargets letterLookTargets)
	{
		extraLetterText = null;
		if (jobRitual.Ritual.attachableOutcomeEffect != null && jobRitual.Ritual.attachableOutcomeEffect.AppliesToOutcome(jobRitual.Ritual.outcomeEffect.def, outcomeChance))
		{
			jobRitual.Ritual.attachableOutcomeEffect.Worker.Apply(totalPresence, jobRitual, outcomeChance, out extraLetterText, ref letterLookTargets);
		}
	}

	public override void Apply(float progress, Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual)
	{
		float quality = GetQuality(jobRitual, progress);
		RitualOutcomePossibility outcome = GetOutcome(quality, jobRitual);
		LookTargets letterLookTargets = jobRitual.selectedTarget;
		ApplyExtraOutcome(totalPresence, jobRitual, outcome, out var extraOutcomeDesc, ref letterLookTargets);
		string extraLetterText = null;
		if (jobRitual.Ritual != null)
		{
			ApplyAttachableOutcome(totalPresence, jobRitual, outcome, out extraLetterText, ref letterLookTargets);
		}
		string text = outcome.description.Formatted(jobRitual.Ritual.Label).CapitalizeFirst();
		string text2 = def.OutcomeMoodBreakdown(outcome);
		if (!text2.NullOrEmpty())
		{
			text = text + "\n\n" + text2;
		}
		if (extraOutcomeDesc != null)
		{
			text = text + "\n\n" + extraOutcomeDesc;
		}
		if (extraLetterText != null)
		{
			text = text + "\n\n" + extraLetterText;
		}
		text = text + "\n\n" + OutcomeQualityBreakdownDesc(quality, progress, jobRitual);
		ApplyDevelopmentPoints(jobRitual.Ritual, outcome, out var extraOutcomeDesc2);
		if (extraOutcomeDesc2 != null)
		{
			text = text + "\n\n" + extraOutcomeDesc2;
		}
		Find.LetterStack.ReceiveLetter("OutcomeLetterLabel".Translate(outcome.label.Named("OUTCOMELABEL"), jobRitual.Ritual.Label.Named("RITUALLABEL")), text, outcome.Positive ? LetterDefOf.RitualOutcomePositive : LetterDefOf.RitualOutcomeNegative, letterLookTargets);
		foreach (KeyValuePair<Pawn, int> item in totalPresence)
		{
			if (!outcome.roleIdsNotGainingMemory.NullOrEmpty())
			{
				RitualRole ritualRole = jobRitual.assignments.RoleForPawn(item.Key);
				if (ritualRole != null && outcome.roleIdsNotGainingMemory.Contains(ritualRole.id))
				{
					continue;
				}
			}
			if (outcome.memory != null)
			{
				GiveMemoryToPawn(item.Key, outcome.memory, jobRitual);
			}
		}
	}

	protected void GiveMemoryToPawn(Pawn pawn, ThoughtDef memory, LordJob_Ritual jobRitual)
	{
		if (pawn.needs?.mood != null)
		{
			Thought_AttendedRitual newThought = (Thought_AttendedRitual)MakeMemory(pawn, jobRitual, memory);
			pawn.needs.mood.thoughts.memories.TryGainMemory(newThought);
		}
	}

	protected virtual void ApplyExtraOutcome(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, RitualOutcomePossibility outcome, out string extraOutcomeDesc, ref LookTargets letterLookTargets)
	{
		extraOutcomeDesc = null;
	}

	protected virtual void ApplyDevelopmentPoints(Precept_Ritual ritual, RitualOutcomePossibility outcome, out string extraOutcomeDesc)
	{
		if (ritual?.ideo != null && ritual.ideo.Fluid)
		{
			if (ritual.ideo.development.Points == ritual.ideo.development.NextReformationDevelopmentPoints)
			{
				extraOutcomeDesc = "RitualOutcomeExtraDesc_DevelopmentPointsAwardedCapped".Translate(ritual.ideo.development.NextReformationDevelopmentPoints);
				return;
			}
			int num = def.outcomeChances.IndexOf(outcome);
			if (num >= 0 && ritual.ideo.development.TryGainDevelopmentPointsForRitualOutcome(ritual, num, out var developmentPoints))
			{
				if (developmentPoints > 0)
				{
					extraOutcomeDesc = "RitualOutcomeExtraDesc_DevelopmentPointsAwarded".Translate(ritual.ideo.development.Points - developmentPoints, ritual.ideo.development.Points, developmentPoints.ToStringWithSign());
				}
				else
				{
					extraOutcomeDesc = "RitualOutcomeExtraDesc_NoDevelopmentPointsAwarded".Translate();
				}
				return;
			}
		}
		extraOutcomeDesc = null;
	}

	protected float GetQuality(LordJob_Ritual jobRitual, float progress)
	{
		float num = def.startingQuality;
		foreach (RitualOutcomeComp comp in def.comps)
		{
			if (comp is RitualOutcomeComp_Quality && comp.Applies(jobRitual))
			{
				num += comp.QualityOffset(jobRitual, DataForComp(comp));
			}
		}
		if (jobRitual.repeatPenalty && jobRitual.Ritual != null)
		{
			num += jobRitual.Ritual.RepeatQualityPenalty;
		}
		Tuple<ExpectationDef, float> expectationsOffset = GetExpectationsOffset(jobRitual.Map, jobRitual.Ritual?.def);
		if (expectationsOffset != null)
		{
			num += expectationsOffset.Item2;
		}
		return Mathf.Clamp(num * Mathf.Lerp(ProgressToQualityMapping.min, ProgressToQualityMapping.max, progress), def.minQuality, def.maxQuality);
	}

	public static Tuple<ExpectationDef, float> GetExpectationsOffset(Map map, PreceptDef ritual)
	{
		if (ritual == null || !ritual.receivesExpectationsQualityOffset)
		{
			return null;
		}
		ExpectationDef expectationDef = ExpectationsUtility.CurrentExpectationFor(map);
		if (Math.Abs(expectationDef.ritualQualityOffset) > float.Epsilon)
		{
			return new Tuple<ExpectationDef, float>(expectationDef, expectationDef.ritualQualityOffset);
		}
		return null;
	}

	public virtual string OutcomeQualityBreakdownDesc(float quality, float progress, LordJob_Ritual jobRitual)
	{
		TaggedString taggedString = "RitualOutcomeQualitySpecific".Translate(jobRitual.Ritual.Label, quality.ToStringPercent()).CapitalizeFirst() + ":\n";
		if (def.startingQuality > 0f)
		{
			taggedString += "\n  - " + "StartingRitualQuality".Translate(def.startingQuality.ToStringPercent()) + ".";
		}
		foreach (RitualOutcomeComp comp in def.comps)
		{
			if (comp is RitualOutcomeComp_Quality && comp.Applies(jobRitual) && Mathf.Abs(comp.QualityOffset(jobRitual, DataForComp(comp))) >= float.Epsilon)
			{
				taggedString += "\n  - " + comp.GetDesc(jobRitual, DataForComp(comp)).CapitalizeFirst();
			}
		}
		if (jobRitual.repeatPenalty && jobRitual.Ritual != null)
		{
			taggedString += "\n  - " + "RitualOutcomePerformedRecently".Translate() + ": " + jobRitual.Ritual.RepeatQualityPenalty.ToStringPercent();
		}
		Tuple<ExpectationDef, float> expectationsOffset = GetExpectationsOffset(jobRitual.Map, jobRitual.Ritual?.def);
		if (expectationsOffset != null)
		{
			taggedString += "\n  - " + "RitualQualityExpectations".Translate(expectationsOffset.Item1.LabelCap) + ": " + expectationsOffset.Item2.ToStringPercent();
		}
		if (progress < 1f)
		{
			taggedString += "\n  - " + "RitualOutcomeProgress".Translate(jobRitual.Ritual.Label).CapitalizeFirst() + ": x" + Mathf.Lerp(ProgressToQualityMapping.min, ProgressToQualityMapping.max, progress).ToStringPercent();
		}
		return taggedString;
	}

	protected virtual bool OutcomePossible(RitualOutcomePossibility chance, LordJob_Ritual ritual)
	{
		return true;
	}

	public override string ExtraAlertParagraph(Precept_Ritual ritual)
	{
		string text = "";
		foreach (RitualOutcomeComp comp in def.comps)
		{
			if (comp is RitualOutcomeComp_Quality)
			{
				string desc = comp.GetDesc();
				if (!desc.NullOrEmpty())
				{
					text = text + "\n  - " + desc.CapitalizeFirst();
				}
			}
		}
		string text2 = ("RitualOutcomeQualityAbstract".Translate(ritual.Label).Resolve().CapitalizeFirst() + ":").Colorize(ColoredText.TipSectionTitleColor) + text;
		return text2 + "\n  - " + "RitualOutcomeProgress".Translate(ritual.Label).Resolve().CapitalizeFirst() + ": " + "OutcomeBonusDesc_QualitySingleOffset".Translate("x" + ProgressToQualityMapping.min * 100f + "-" + ProgressToQualityMapping.max.ToStringPercent()).Resolve() + ".";
	}
}
