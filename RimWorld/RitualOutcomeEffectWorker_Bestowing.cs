using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class RitualOutcomeEffectWorker_Bestowing : RitualOutcomeEffectWorker_FromQuality
{
	public RitualOutcomeEffectWorker_Bestowing()
	{
	}

	public RitualOutcomeEffectWorker_Bestowing(RitualOutcomeEffectDef def)
		: base(def)
	{
	}

	public override void Apply(float progress, Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual)
	{
		LordJob_BestowingCeremony lordJob_BestowingCeremony = (LordJob_BestowingCeremony)jobRitual;
		Pawn target = lordJob_BestowingCeremony.target;
		Pawn bestower = lordJob_BestowingCeremony.bestower;
		Hediff_Psylink mainPsylinkSource = target.GetMainPsylinkSource();
		float quality = GetQuality(jobRitual, progress);
		RitualOutcomePossibility outcome = GetOutcome(quality, jobRitual);
		LookTargets letterLookTargets = target;
		string extraLetterText = null;
		if (jobRitual.Ritual != null)
		{
			ApplyAttachableOutcome(totalPresence, jobRitual, outcome, out extraLetterText, ref letterLookTargets);
		}
		RoyalTitleDef currentTitle = target.royalty.GetCurrentTitle(bestower.Faction);
		RoyalTitleDef titleAwardedWhenUpdating = target.royalty.GetTitleAwardedWhenUpdating(bestower.Faction, target.royalty.GetFavor(bestower.Faction));
		Pawn_RoyaltyTracker.MakeLetterTextForTitleChange(target, bestower.Faction, currentTitle, titleAwardedWhenUpdating, out var headline, out var body);
		if (target.royalty != null)
		{
			target.royalty.TryUpdateTitle(bestower.Faction, sendLetter: false, titleAwardedWhenUpdating);
		}
		List<AbilityDef> abilitiesPreUpdate = ((mainPsylinkSource == null) ? new List<AbilityDef>() : target.abilities.abilities.Select((Ability a) => a.def).ToList());
		ThingOwner<Thing> innerContainer = bestower.inventory.innerContainer;
		Thing thing = innerContainer.First((Thing t) => t.def == ThingDefOf.PsychicAmplifier);
		innerContainer.Remove(thing);
		thing.Destroy();
		for (int num = target.GetPsylinkLevel(); num < target.GetMaxPsylinkLevelByTitle(); num++)
		{
			target.ChangePsylinkLevel(1, sendLetter: false);
			Find.History.Notify_PsylinkAvailable();
		}
		foreach (KeyValuePair<Pawn, int> item in totalPresence)
		{
			Pawn key = item.Key;
			if (key != target)
			{
				key.needs.mood.thoughts.memories.TryGainMemory(outcome.memory);
			}
		}
		int num2 = 0;
		for (int num3 = def.honorFromQuality.PointsCount - 1; num3 >= 0; num3--)
		{
			if (quality >= def.honorFromQuality[num3].x)
			{
				num2 = (int)def.honorFromQuality[num3].y;
				break;
			}
		}
		if (num2 > 0)
		{
			target.royalty.GainFavor(bestower.Faction, num2);
		}
		List<AbilityDef> newAbilities = ((mainPsylinkSource == null) ? new List<AbilityDef>() : (from a in target.abilities.abilities
			select a.def into def
			where !abilitiesPreUpdate.Contains(def)
			select def).ToList());
		string text = headline;
		text = text + "\n\n" + Hediff_Psylink.MakeLetterTextNewPsylinkLevel(lordJob_BestowingCeremony.target, target.GetPsylinkLevel(), newAbilities);
		text = text + "\n\n" + body;
		if (extraLetterText != null)
		{
			text = text + "\n\n" + extraLetterText;
		}
		Find.LetterStack.ReceiveLetter("LetterLabelGainedRoyalTitle".Translate(titleAwardedWhenUpdating.GetLabelCapFor(target).Named("TITLE"), target.Named("PAWN")), text, LetterDefOf.RitualOutcomePositive, letterLookTargets, lordJob_BestowingCeremony.bestower.Faction);
		string text2 = OutcomeDesc(quality, progress, lordJob_BestowingCeremony, num2, totalPresence.Count);
		Find.LetterStack.ReceiveLetter("OutcomeLetterLabel".Translate(outcome.label.Named("OUTCOMELABEL"), "RitualBestowingCeremony".Translate().Named("RITUALLABEL")), text2, outcome.Positive ? LetterDefOf.RitualOutcomePositive : LetterDefOf.RitualOutcomeNegative, target);
	}

	private string OutcomeDesc(float quality, float progress, LordJob_BestowingCeremony jobRitual, int honor, int totalPresence)
	{
		TaggedString taggedString = "BestowingOutcomeQualitySpecific".Translate(quality.ToStringPercent()) + ":\n";
		Pawn target = jobRitual.target;
		Pawn bestower = jobRitual.bestower;
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
		if (progress < 1f)
		{
			taggedString += "\n  - " + "RitualOutcomeProgress".Translate("RitualBestowingCeremony".Translate()) + ": x" + Mathf.Lerp(RitualOutcomeEffectWorker_FromQuality.ProgressToQualityMapping.min, RitualOutcomeEffectWorker_FromQuality.ProgressToQualityMapping.max, progress).ToStringPercent();
		}
		taggedString += "\n\n";
		if (honor > 0)
		{
			taggedString += "LetterPartBestowingExtraHonor".Translate(target.Named("PAWN"), honor, bestower.Faction.Named("FACTION"), totalPresence);
		}
		else
		{
			taggedString += "LetterPartNoExtraHonor".Translate(target.Named("PAWN"));
		}
		taggedString += "\n\n" + "LordJobOutcomeChances".Translate(quality.ToStringPercent()) + ":\n";
		float num = 0f;
		foreach (RitualOutcomePossibility outcomeChance in def.outcomeChances)
		{
			num += (outcomeChance.Positive ? (outcomeChance.chance * quality) : outcomeChance.chance);
		}
		foreach (RitualOutcomePossibility outcomeChance2 in def.outcomeChances)
		{
			taggedString += "\n  - ";
			if (outcomeChance2.Positive)
			{
				taggedString += outcomeChance2.memory.stages[0].LabelCap + ": " + (outcomeChance2.chance * quality / num).ToStringPercent();
			}
			else
			{
				taggedString += outcomeChance2.memory.stages[0].LabelCap + ": " + (outcomeChance2.chance / num).ToStringPercent();
			}
		}
		return taggedString;
	}
}
