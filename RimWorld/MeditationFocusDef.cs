using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class MeditationFocusDef : Def
{
	public bool requiresRoyalTitle;

	public List<BackstoryCategoryAndSlot> requiredBackstoriesAny = new List<BackstoryCategoryAndSlot>();

	public List<BackstoryCategoryAndSlot> incompatibleBackstoriesAny = new List<BackstoryCategoryAndSlot>();

	private static List<string> tmpReasons = new List<string>();

	public bool CanPawnUse(Pawn p)
	{
		return MeditationFocusTypeAvailabilityCache.PawnCanUse(p, this);
	}

	public string EnablingThingsExplanation(Pawn pawn)
	{
		tmpReasons.Clear();
		if (requiresRoyalTitle && pawn.royalty != null && pawn.royalty.AllTitlesInEffectForReading.Count > 0)
		{
			RoyalTitle royalTitle = pawn.royalty.AllTitlesInEffectForReading.MaxBy((RoyalTitle t) => t.def.seniority);
			tmpReasons.Add("MeditationFocusEnabledByTitle".Translate(royalTitle.def.GetLabelCapFor(pawn).Named("TITLE"), royalTitle.faction.Named("FACTION")).Resolve());
		}
		if (pawn.story != null)
		{
			BackstoryDef adulthood = pawn.story.Adulthood;
			BackstoryDef childhood = pawn.story.Childhood;
			if (!requiresRoyalTitle && requiredBackstoriesAny.Count == 0)
			{
				for (int num = 0; num < incompatibleBackstoriesAny.Count; num++)
				{
					BackstoryCategoryAndSlot backstoryCategoryAndSlot = incompatibleBackstoriesAny[num];
					BackstoryDef backstoryDef = ((backstoryCategoryAndSlot.slot == BackstorySlot.Adulthood) ? adulthood : childhood);
					if (!backstoryDef.spawnCategories.Contains(backstoryCategoryAndSlot.categoryName))
					{
						AddBackstoryReason(backstoryCategoryAndSlot.slot, backstoryDef);
					}
				}
				for (int num2 = 0; num2 < DefDatabase<TraitDef>.AllDefsListForReading.Count; num2++)
				{
					TraitDef traitDef = DefDatabase<TraitDef>.AllDefsListForReading[num2];
					List<MeditationFocusDef> disallowedMeditationFocusTypes = traitDef.degreeDatas[0].disallowedMeditationFocusTypes;
					if (disallowedMeditationFocusTypes != null && disallowedMeditationFocusTypes.Contains(this))
					{
						tmpReasons.Add("MeditationFocusDisabledByTrait".Translate() + ": " + traitDef.degreeDatas[0].GetLabelCapFor(pawn) + ".");
					}
				}
			}
			for (int num3 = 0; num3 < requiredBackstoriesAny.Count; num3++)
			{
				BackstoryCategoryAndSlot backstoryCategoryAndSlot2 = requiredBackstoriesAny[num3];
				BackstoryDef backstoryDef2 = ((backstoryCategoryAndSlot2.slot == BackstorySlot.Adulthood) ? adulthood : childhood);
				if (backstoryDef2.spawnCategories.Contains(backstoryCategoryAndSlot2.categoryName))
				{
					AddBackstoryReason(backstoryCategoryAndSlot2.slot, backstoryDef2);
				}
			}
			for (int num4 = 0; num4 < pawn.story.traits.allTraits.Count; num4++)
			{
				Trait trait = pawn.story.traits.allTraits[num4];
				if (!trait.Suppressed)
				{
					List<MeditationFocusDef> allowedMeditationFocusTypes = trait.CurrentData.allowedMeditationFocusTypes;
					if (allowedMeditationFocusTypes != null && allowedMeditationFocusTypes.Contains(this))
					{
						tmpReasons.Add("MeditationFocusEnabledByTrait".Translate() + ": " + trait.LabelCap + ".");
					}
				}
			}
		}
		for (int num5 = 0; num5 < pawn.health.hediffSet.hediffs.Count; num5++)
		{
			HediffDef def = pawn.health.hediffSet.hediffs[num5].def;
			if (def.allowedMeditationFocusTypes.NotNullAndContains(this))
			{
				tmpReasons.Add("MeditationFocusEnabledByHediff".Translate() + ": " + def.LabelCap + ".");
			}
		}
		return tmpReasons.ToLineList("  - ", capitalizeItems: true);
		static void AddBackstoryReason(BackstorySlot slot, BackstoryDef backstory)
		{
			if (slot == BackstorySlot.Adulthood)
			{
				tmpReasons.Add("MeditationFocusEnabledByAdulthood".Translate() + ": " + backstory.title.CapitalizeFirst() + ".");
			}
			else
			{
				tmpReasons.Add("MeditationFocusEnabledByChildhood".Translate() + ": " + backstory.title.CapitalizeFirst() + ".");
			}
		}
	}
}
