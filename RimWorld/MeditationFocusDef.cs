using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class MeditationFocusDef : Def
	{
		public bool requiresRoyalTitle;

		public List<BackstoryCategoryAndSlot> requiredBackstoriesAny = new List<BackstoryCategoryAndSlot>();

		public List<BackstoryCategoryAndSlot> incompatibleBackstoriesAny = new List<BackstoryCategoryAndSlot>();

		public bool CanPawnUse(Pawn p)
		{
			return MeditationFocusTypeAvailabilityCache.PawnCanUse(p, this);
		}

		public string EnablingThingsExplanation(Pawn pawn)
		{
			List<string> reasons = new List<string>();
			if (requiresRoyalTitle && pawn.royalty != null && pawn.royalty.AllTitlesInEffectForReading.Count > 0)
			{
				RoyalTitle royalTitle = pawn.royalty.AllTitlesInEffectForReading.MaxBy((RoyalTitle t) => t.def.seniority);
				reasons.Add("MeditationFocusEnabledByTitle".Translate(royalTitle.def.GetLabelCapFor(pawn).Named("TITLE"), royalTitle.faction.Named("FACTION")).Resolve());
			}
			if (pawn.story != null)
			{
				Backstory adulthood = pawn.story.adulthood;
				Backstory childhood = pawn.story.childhood;
				if (!requiresRoyalTitle && requiredBackstoriesAny.Count == 0)
				{
					for (int i = 0; i < incompatibleBackstoriesAny.Count; i++)
					{
						BackstoryCategoryAndSlot backstoryCategoryAndSlot = incompatibleBackstoriesAny[i];
						Backstory backstory2 = (backstoryCategoryAndSlot.slot == BackstorySlot.Adulthood) ? adulthood : childhood;
						if (!backstory2.spawnCategories.Contains(backstoryCategoryAndSlot.categoryName))
						{
							AddBackstoryReason(backstoryCategoryAndSlot.slot, backstory2);
						}
					}
					for (int j = 0; j < DefDatabase<TraitDef>.AllDefsListForReading.Count; j++)
					{
						TraitDef traitDef = DefDatabase<TraitDef>.AllDefsListForReading[j];
						List<MeditationFocusDef> disallowedMeditationFocusTypes = traitDef.degreeDatas[0].disallowedMeditationFocusTypes;
						if (disallowedMeditationFocusTypes != null && disallowedMeditationFocusTypes.Contains(this))
						{
							reasons.Add("MeditationFocusDisabledByTrait".Translate() + ": " + traitDef.degreeDatas[0].LabelCap + ".");
						}
					}
				}
				for (int k = 0; k < requiredBackstoriesAny.Count; k++)
				{
					BackstoryCategoryAndSlot backstoryCategoryAndSlot2 = requiredBackstoriesAny[k];
					Backstory backstory3 = (backstoryCategoryAndSlot2.slot == BackstorySlot.Adulthood) ? adulthood : childhood;
					if (backstory3.spawnCategories.Contains(backstoryCategoryAndSlot2.categoryName))
					{
						AddBackstoryReason(backstoryCategoryAndSlot2.slot, backstory3);
					}
				}
				for (int l = 0; l < pawn.story.traits.allTraits.Count; l++)
				{
					Trait trait = pawn.story.traits.allTraits[l];
					List<MeditationFocusDef> allowedMeditationFocusTypes = trait.CurrentData.allowedMeditationFocusTypes;
					if (allowedMeditationFocusTypes != null && allowedMeditationFocusTypes.Contains(this))
					{
						reasons.Add("MeditationFocusEnabledByTrait".Translate() + ": " + trait.LabelCap + ".");
					}
				}
			}
			return reasons.ToLineList("  - ", capitalizeItems: true);
			void AddBackstoryReason(BackstorySlot slot, Backstory backstory)
			{
				if (slot == BackstorySlot.Adulthood)
				{
					reasons.Add("MeditationFocusEnabledByAdulthood".Translate() + ": " + backstory.title.CapitalizeFirst() + ".");
				}
				else
				{
					reasons.Add("MeditationFocusEnabledByChildhood".Translate() + ": " + backstory.title.CapitalizeFirst() + ".");
				}
			}
		}
	}
}
