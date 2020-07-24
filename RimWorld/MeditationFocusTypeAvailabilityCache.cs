using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class MeditationFocusTypeAvailabilityCache
	{
		private static Dictionary<Pawn, Dictionary<MeditationFocusDef, bool>> pawnCanUseMeditationTypeCached = new Dictionary<Pawn, Dictionary<MeditationFocusDef, bool>>();

		public static bool PawnCanUse(Pawn p, MeditationFocusDef type)
		{
			if (!pawnCanUseMeditationTypeCached.ContainsKey(p))
			{
				pawnCanUseMeditationTypeCached[p] = new Dictionary<MeditationFocusDef, bool>();
			}
			if (!pawnCanUseMeditationTypeCached[p].ContainsKey(type))
			{
				pawnCanUseMeditationTypeCached[p][type] = PawnCanUseInt(p, type);
			}
			return pawnCanUseMeditationTypeCached[p][type];
		}

		public static void ClearFor(Pawn p)
		{
			if (pawnCanUseMeditationTypeCached.ContainsKey(p))
			{
				pawnCanUseMeditationTypeCached[p].Clear();
			}
		}

		private static bool PawnCanUseInt(Pawn p, MeditationFocusDef type)
		{
			if (p.story != null)
			{
				for (int i = 0; i < p.story.traits.allTraits.Count; i++)
				{
					List<MeditationFocusDef> disallowedMeditationFocusTypes = p.story.traits.allTraits[i].CurrentData.disallowedMeditationFocusTypes;
					if (disallowedMeditationFocusTypes != null && disallowedMeditationFocusTypes.Contains(type))
					{
						return false;
					}
				}
				List<string> list = p.story.adulthood?.spawnCategories;
				List<string> list2 = p.story.childhood?.spawnCategories;
				for (int j = 0; j < type.incompatibleBackstoriesAny.Count; j++)
				{
					BackstoryCategoryAndSlot backstoryCategoryAndSlot = type.incompatibleBackstoriesAny[j];
					List<string> list3 = (backstoryCategoryAndSlot.slot == BackstorySlot.Adulthood) ? list : list2;
					if (list3 != null && list3.Contains(backstoryCategoryAndSlot.categoryName))
					{
						return false;
					}
				}
			}
			if (type.requiresRoyalTitle)
			{
				if (p.royalty != null)
				{
					return p.royalty.AllTitlesInEffectForReading.Any((RoyalTitle t) => t.def.allowDignifiedMeditationFocus);
				}
				return false;
			}
			if (p.story != null)
			{
				for (int k = 0; k < p.story.traits.allTraits.Count; k++)
				{
					List<MeditationFocusDef> allowedMeditationFocusTypes = p.story.traits.allTraits[k].CurrentData.allowedMeditationFocusTypes;
					if (allowedMeditationFocusTypes != null && allowedMeditationFocusTypes.Contains(type))
					{
						return true;
					}
				}
				List<string> list4 = p.story.adulthood?.spawnCategories;
				List<string> list5 = p.story.childhood?.spawnCategories;
				for (int l = 0; l < type.requiredBackstoriesAny.Count; l++)
				{
					BackstoryCategoryAndSlot backstoryCategoryAndSlot2 = type.requiredBackstoriesAny[l];
					List<string> list6 = (backstoryCategoryAndSlot2.slot == BackstorySlot.Adulthood) ? list4 : list5;
					if (list6 != null && list6.Contains(backstoryCategoryAndSlot2.categoryName))
					{
						return true;
					}
				}
			}
			if (type.requiredBackstoriesAny.Count == 0)
			{
				bool flag = false;
				for (int m = 0; m < DefDatabase<TraitDef>.AllDefsListForReading.Count; m++)
				{
					if (flag)
					{
						break;
					}
					TraitDef traitDef = DefDatabase<TraitDef>.AllDefsListForReading[m];
					for (int n = 0; n < traitDef.degreeDatas.Count; n++)
					{
						List<MeditationFocusDef> allowedMeditationFocusTypes2 = traitDef.degreeDatas[n].allowedMeditationFocusTypes;
						if (allowedMeditationFocusTypes2 != null && allowedMeditationFocusTypes2.Contains(type))
						{
							flag = true;
							break;
						}
					}
				}
				if (!flag)
				{
					return true;
				}
			}
			return false;
		}
	}
}
