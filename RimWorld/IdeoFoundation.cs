using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace RimWorld;

public abstract class IdeoFoundation : IExposable
{
	public Ideo ideo;

	public IdeoFoundationDef def;

	public PlaceDef place;

	public static readonly IntRange MemeCountRangeAbsolute = new IntRange(1, 4);

	public static readonly IntRange MemeCountRangeNPCInitial = new IntRange(1, 3);

	public static readonly IntRange MemeCountRangeFluidAbsolute = new IntRange(1, 1);

	private static IntRange PreceptCountRange_HighImpact = new IntRange(0, 2);

	private static IntRange PreceptCountRange_MediumImpact = new IntRange(5, 5);

	private static IntRange PreceptCountRange_LowImpact = new IntRange(5, 5);

	private static IntRange InitialVeneratedAnimalsCountRange = new IntRange(0, 1);

	public const int MaxStyleCategories = 3;

	public const int MaxRituals = 6;

	public const int MaxMultiRoles = 2;

	private static List<MemeDef> tmpMemes = new List<MemeDef>();

	private static Dictionary<MemeDef, int> tmpMemesNumRitualsToMake = new Dictionary<MemeDef, int>();

	private static List<IdeoWeaponClassPair> tmpWeaponClassPairs = new List<IdeoWeaponClassPair>();

	private static List<Precept> tmpInitializedPrecepts = new List<Precept>();

	private List<Precept> tmpPreceptsToRemove = new List<Precept>();

	private List<MemeDef> tmpAddedMemes = new List<MemeDef>();

	public abstract void Init(IdeoGenerationParms parms);

	public abstract void DoInfo(ref float curY, float width, IdeoEditMode editMode);

	public abstract void GenerateTextSymbols();

	public virtual void ExposeData()
	{
		Scribe_Defs.Look(ref def, "def");
		Scribe_Defs.Look(ref place, "place");
	}

	public static IdeoIconDef GetRandomIconDef(Ideo ideo)
	{
		IEnumerable<IdeoIconDef> enumerable = null;
		if (Find.World != null)
		{
			enumerable = DefDatabase<IdeoIconDef>.AllDefs.Where((IdeoIconDef x) => x.CanBeChosenForIdeo(ideo) && !Find.IdeoManager.IdeosListForReading.Any((Ideo y) => y.iconDef == x));
		}
		if (enumerable.EnumerableNullOrEmpty())
		{
			enumerable = DefDatabase<IdeoIconDef>.AllDefs.Where((IdeoIconDef x) => x.CanBeChosenForIdeo(ideo));
		}
		return enumerable.RandomElement();
	}

	public static ColorDef GetRandomColorDef(Ideo ideo)
	{
		IEnumerable<ColorDef> enumerable = null;
		if (Find.World != null)
		{
			enumerable = from x in DefDatabase<IdeoColorDef>.AllDefs
				where x.CanBeChosenForIdeo(ideo) && x.colorDef.colorType == ColorType.Ideo && !Find.IdeoManager.IdeosListForReading.Any((Ideo y) => y.colorDef == x.colorDef)
				select x.colorDef;
		}
		if (enumerable.EnumerableNullOrEmpty())
		{
			enumerable = from x in DefDatabase<IdeoColorDef>.AllDefs
				where x.CanBeChosenForIdeo(ideo) && x.colorDef.colorType == ColorType.Ideo
				select x.colorDef;
		}
		return enumerable.RandomElement();
	}

	public virtual void RandomizeCulture(IdeoGenerationParms parms)
	{
		if (parms.forFaction != null && parms.forFaction.allowedCultures != null)
		{
			ideo.culture = parms.forFaction.allowedCultures.RandomElement();
		}
		else
		{
			ideo.culture = DefDatabase<CultureDef>.AllDefsListForReading.RandomElement();
		}
	}

	public virtual void RandomizePlace()
	{
		place = DefDatabase<PlaceDef>.AllDefsListForReading.Where((PlaceDef p) => p.tags.SharesElementWith(ideo.culture.allowedPlaceTags)).RandomElement();
	}

	protected virtual void RandomizeMemes(IdeoGenerationParms parms)
	{
		ideo.memes.Clear();
		ideo.memes.AddRange(IdeoUtility.GenerateRandomMemes(parms));
		ideo.SortMemesInDisplayOrder();
	}

	public virtual void RandomizeStyles()
	{
		if (ideo == null)
		{
			return;
		}
		List<ThingStyleCategoryWithPriority> list = new List<ThingStyleCategoryWithPriority>();
		if (!ideo.culture.thingStyleCategories.NullOrEmpty())
		{
			list.AddRange(ideo.culture.thingStyleCategories);
		}
		List<MemeDef> memes = ideo.memes;
		for (int i = 0; i < memes.Count; i++)
		{
			if (!memes[i].thingStyleCategories.NullOrEmpty())
			{
				list.AddRange(memes[i].thingStyleCategories);
			}
		}
		list.Sort((ThingStyleCategoryWithPriority first, ThingStyleCategoryWithPriority second) => (first.priority == second.priority) ? ((!(Rand.Value < 0.5f)) ? 1 : (-1)) : (-first.priority.CompareTo(second.priority)));
		ideo.thingStyleCategories = new List<ThingStyleCategoryWithPriority>();
		while (ideo.thingStyleCategories.Count < 3 && list.Any())
		{
			ThingStyleCategoryWithPriority thingStyleCategoryWithPriority = list.First();
			list.Remove(thingStyleCategoryWithPriority);
			if (CanUseStyleCategory(thingStyleCategoryWithPriority.category))
			{
				ideo.thingStyleCategories.Add(thingStyleCategoryWithPriority);
				ideo.SortStyleCategories();
			}
		}
		ideo.style.ResetStylesForThingDef();
		bool CanUseStyleCategory(StyleCategoryDef cat)
		{
			if (ideo.thingStyleCategories.Any((ThingStyleCategoryWithPriority x) => x.category == cat))
			{
				return false;
			}
			foreach (ThingStyleCategoryWithPriority thingStyleCategory in ideo.thingStyleCategories)
			{
				if (thingStyleCategory.category.thingDefStyles.All((ThingDefStyle x) => cat.thingDefStyles.Contains(x)))
				{
					return false;
				}
			}
			if (cat.fixedIdeoOnly)
			{
				return false;
			}
			return true;
		}
	}

	public bool CanAddForFaction(PreceptDef precept, FactionDef forFaction, List<PreceptDef> disallowedPrecepts, bool checkDuplicates, bool ignoreMemeRequirements = false, bool ignoreConflictingMemes = false, bool classic = false)
	{
		if (!CanAdd(precept, checkDuplicates).Accepted)
		{
			return false;
		}
		if (disallowedPrecepts != null && disallowedPrecepts.Contains(precept))
		{
			return false;
		}
		if (precept.classicModeOnly)
		{
			return false;
		}
		if (classic)
		{
			if (!precept.classicExtra)
			{
				return precept.classic;
			}
			return true;
		}
		if (forFaction != null)
		{
			if (forFaction.disallowedPrecepts != null && forFaction.disallowedPrecepts.Contains(precept))
			{
				return false;
			}
			if (forFaction.classicIdeo)
			{
				if (precept.classic)
				{
					return true;
				}
				if (precept.impact == PreceptImpact.High)
				{
					return false;
				}
			}
		}
		return true;
	}

	public virtual void RandomizePrecepts(bool init, IdeoGenerationParms parms)
	{
		ideo.ClearPrecepts();
		tmpWeaponClassPairs.Clear();
		tmpInitializedPrecepts.Clear();
		int high = PreceptCountRange_HighImpact.RandomInRange;
		int medium = PreceptCountRange_MediumImpact.RandomInRange;
		int low = PreceptCountRange_LowImpact.RandomInRange;
		bool flag = !ideo.Fluid && !parms.forceNoWeaponPreference && Rand.Bool;
		AddRequiredPreceptsForMemes(ideo.memes, parms, flag, tmpWeaponClassPairs, delegate(PreceptDef p)
		{
			if (p.impact == PreceptImpact.High)
			{
				high--;
			}
			else if (p.impact == PreceptImpact.Medium)
			{
				medium--;
			}
			else
			{
				low--;
			}
		});
		if (parms.requiredPreceptsOnly)
		{
			AddSpecialPrecepts(parms, DefDatabase<PreceptDef>.AllDefsListForReading);
			FinalizeIdeo(ideo);
			if (init)
			{
				InitPrecepts(parms, tmpInitializedPrecepts);
				ideo.RecachePrecepts();
			}
			tmpInitializedPrecepts.Clear();
			return;
		}
		List<PreceptDef> allDefsListForReading = DefDatabase<PreceptDef>.AllDefsListForReading;
		if (parms.classicExtra)
		{
			for (int num = 0; num < allDefsListForReading.Count; num++)
			{
				if (allDefsListForReading[num].classic)
				{
					ideo.AddPrecept(PreceptMaker.MakePrecept(allDefsListForReading[num]));
				}
			}
		}
		AddPreceptsOfImpact(PreceptImpact.High, high);
		AddPreceptsOfImpact(PreceptImpact.Medium, medium);
		AddPreceptsOfImpact(PreceptImpact.Low, low);
		List<IssueDef> allIssueDefs = DefDatabase<IssueDef>.AllDefsListForReading;
		int i;
		for (i = 0; i < allIssueDefs.Count; i++)
		{
			PreceptDef result2;
			if (allDefsListForReading.Where((PreceptDef x) => x.issue == allIssueDefs[i] && x.associatedMemes.Any((MemeDef y) => ideo.memes.Contains(y)) && CanAddForFaction(x, parms.forFaction, parms.disallowedPrecepts, checkDuplicates: true, ignoreMemeRequirements: false, ignoreConflictingMemes: false, parms.classicExtra)).TryRandomElementByWeight((PreceptDef x) => x.defaultSelectionWeight, out var result))
			{
				ideo.AddPrecept(PreceptMaker.MakePrecept(result));
			}
			else if (allDefsListForReading.Where((PreceptDef x) => x.issue == allIssueDefs[i] && CanAddForFaction(x, parms.forFaction, parms.disallowedPrecepts, checkDuplicates: true, ignoreMemeRequirements: false, ignoreConflictingMemes: false, parms.classicExtra)).TryRandomElementByWeight((PreceptDef x) => x.defaultSelectionWeight, out result2))
			{
				ideo.AddPrecept(PreceptMaker.MakePrecept(result2));
			}
		}
		AddSpecialPrecepts(parms, allDefsListForReading);
		IEnumerable<PreceptDef> source = DefDatabase<PreceptDef>.AllDefs.Where((PreceptDef x) => x.preceptClass == typeof(Precept_RoleMulti) && CanAddForFaction(x, parms.forFaction, parms.disallowedPrecepts, checkDuplicates: true, ignoreMemeRequirements: false, ignoreConflictingMemes: false, parms.classicExtra));
		if (source.Any())
		{
			for (int num2 = 0; num2 < 2; num2++)
			{
				if (source.TryRandomElementByWeight((PreceptDef x) => (!ideo.HasPrecept(x)) ? 1f : 0f, out var result3))
				{
					ideo.AddPrecept(PreceptMaker.MakePrecept(result3));
				}
			}
		}
		if (!parms.classicExtra)
		{
			int num3 = InitialVeneratedAnimalsCountRange.RandomInRange;
			for (int num4 = 0; num4 < ideo.memes.Count; num4++)
			{
				if (ideo.memes[num4].veneratedAnimalsCountOverride >= 0)
				{
					num3 = ideo.memes[num4].veneratedAnimalsCountOverride;
					break;
				}
				num3 += ideo.memes[num4].veneratedAnimalsCountOffset;
			}
			for (int num5 = 0; num5 < Mathf.Min(num3, PreceptDefOf.AnimalVenerated.maxCount); num5++)
			{
				Precept_Animal precept = (Precept_Animal)PreceptMaker.MakePrecept(PreceptDefOf.AnimalVenerated);
				ideo.AddPrecept(precept);
			}
			if (flag)
			{
				if (ideo.culture.preferredWeaponClasses != null)
				{
					tmpWeaponClassPairs.Add(ideo.culture.preferredWeaponClasses);
				}
				Precept_Weapon precept_Weapon = (Precept_Weapon)PreceptMaker.MakePrecept(PreceptDefOf.NobleDespisedWeapons);
				if (!tmpWeaponClassPairs.NullOrEmpty())
				{
					IdeoWeaponClassPair ideoWeaponClassPair = tmpWeaponClassPairs.RandomElement();
					precept_Weapon.noble = ideoWeaponClassPair.noble;
					precept_Weapon.despised = ideoWeaponClassPair.despised;
				}
				else
				{
					WeaponClassPairDef weaponClassPairDef = DefDatabase<WeaponClassPairDef>.AllDefs.RandomElement();
					bool flag2 = Rand.Bool;
					precept_Weapon.noble = (flag2 ? weaponClassPairDef.second : weaponClassPairDef.first);
					precept_Weapon.despised = (flag2 ? weaponClassPairDef.first : weaponClassPairDef.second);
				}
				ideo.AddPrecept(precept_Weapon);
			}
			if (ModsConfig.BiotechActive && parms.forFaction != null)
			{
				XenotypeSet xenotypeSet = parms.forFaction.xenotypeSet;
				if (xenotypeSet != null && xenotypeSet.Count == 1 && Rand.Bool)
				{
					Precept_Xenotype precept_Xenotype = (Precept_Xenotype)PreceptMaker.MakePrecept(PreceptDefOf.PreferredXenotype);
					precept_Xenotype.xenotype = xenotypeSet[0].xenotype;
					ideo.AddPrecept(precept_Xenotype);
				}
			}
		}
		FinalizeIdeo(ideo);
		if (init)
		{
			InitPrecepts(parms, tmpInitializedPrecepts);
			ideo.RecachePrecepts();
		}
		tmpInitializedPrecepts.Clear();
		void AddPreceptsOfImpact(PreceptImpact impact, int countToAdd)
		{
			tmpMemes.Clear();
			tmpMemes.AddRange(ideo.memes);
			tmpMemes.Shuffle();
			tmpMemesNumRitualsToMake.Clear();
			for (int j = 0; j < tmpMemes.Count; j++)
			{
				if (tmpMemes[j].ritualsToMake != IntRange.Zero)
				{
					tmpMemesNumRitualsToMake.Add(tmpMemes[j], tmpMemes[j].ritualsToMake.RandomInRange);
				}
			}
			int num6 = 0;
			for (int k = 0; k < countToAdd; k++)
			{
				bool flag3 = false;
				int l = num6;
				for (int num7 = num6 + tmpMemes.Count; l < num7; l++)
				{
					MemeDef memeToTry = tmpMemes[l % tmpMemes.Count];
					if (DefDatabase<PreceptDef>.AllDefsListForReading.Where((PreceptDef x) => x.countsTowardsPreceptLimit && x.impact == impact && x.associatedMemes.Contains(memeToTry) && CanAddForFaction(x, parms.forFaction, parms.disallowedPrecepts, checkDuplicates: true, ignoreMemeRequirements: false, ignoreConflictingMemes: false, parms.classicExtra)).TryRandomElementByWeight((PreceptDef x) => x.selectionWeight, out var result4))
					{
						ideo.AddPrecept(PreceptMaker.MakePrecept(result4));
						num6++;
						flag3 = true;
						break;
					}
				}
				if (!flag3)
				{
					break;
				}
			}
		}
	}

	private void AddSpecialPrecepts(IdeoGenerationParms parms, List<PreceptDef> allPreceptDefs)
	{
		for (int i = 0; i < allPreceptDefs.Count; i++)
		{
			PreceptDef preceptDef = allPreceptDefs[i];
			if (preceptDef.countsTowardsPreceptLimit || !preceptDef.canGenerateAsSpecialPrecept || !CanAddForFaction(preceptDef, parms.forFaction, parms.disallowedPrecepts, checkDuplicates: true, ignoreMemeRequirements: false, ignoreConflictingMemes: false, parms.classicExtra))
			{
				continue;
			}
			if (preceptDef.preceptInstanceCountCurve != null)
			{
				int num = Mathf.CeilToInt(preceptDef.preceptInstanceCountCurve.Evaluate(Rand.Value));
				for (int j = 0; j < num; j++)
				{
					ideo.AddPrecept(PreceptMaker.MakePrecept(preceptDef));
				}
			}
			else
			{
				ideo.AddPrecept(PreceptMaker.MakePrecept(preceptDef));
			}
		}
	}

	private void FinalizeIdeo(Ideo ideo)
	{
		if (!ideo.IdeoPrefersNudity())
		{
			return;
		}
		List<Precept> preceptsListForReading = ideo.PreceptsListForReading;
		for (int num = preceptsListForReading.Count - 1; num >= 0; num--)
		{
			if (preceptsListForReading[num] is Precept_Apparel)
			{
				ideo.RemovePrecept(preceptsListForReading[num]);
			}
		}
	}

	private void AddRequiredPreceptsForMemes(List<MemeDef> memes, IdeoGenerationParms parms, bool generateWeaponPrecept, List<IdeoWeaponClassPair> weaponClassPairsOut = null, Action<PreceptDef> preceptGenerated = null)
	{
		for (int i = 0; i < memes.Count; i++)
		{
			if (memes[i].selectOneOrNone != null && Rand.Value >= memes[i].selectOneOrNone.noneChance && memes[i].selectOneOrNone.preceptThingPairs.Where((PreceptThingPair x) => CanAddForFaction(x.precept, parms.forFaction, parms.disallowedPrecepts, checkDuplicates: true, ignoreMemeRequirements: false, ignoreConflictingMemes: false, parms.classicExtra)).TryRandomElementByWeight((PreceptThingPair x) => x.precept.selectionWeight, out var result))
			{
				Precept precept = PreceptMaker.MakePrecept(result.precept);
				if (precept is Precept_Apparel precept_Apparel)
				{
					precept_Apparel.apparelDef = result.thing;
				}
				else if (precept is Precept_ThingDef precept_ThingDef)
				{
					precept_ThingDef.ThingDef = result.thing;
				}
				ideo.AddPrecept(precept, init: true);
			}
			if (memes[i].requiredRituals != null && !parms.classicExtra)
			{
				for (int num = 0; num < memes[i].requiredRituals.Count; num++)
				{
					RequiredRitualAndBuilding requiredRitualAndBuilding = memes[i].requiredRituals[num];
					Precept_Ritual precept_Ritual = (Precept_Ritual)PreceptMaker.MakePrecept(requiredRitualAndBuilding.precept);
					ideo.AddPrecept(precept_Ritual, init: true, parms.forFaction, requiredRitualAndBuilding.pattern);
					precept_Ritual.RegenerateName();
					tmpInitializedPrecepts.Add(precept_Ritual);
					if (requiredRitualAndBuilding.building != null)
					{
						Precept_Building precept_Building = (Precept_Building)PreceptMaker.MakePrecept(PreceptDefOf.IdeoBuilding);
						ideo.AddPrecept(precept_Building, init: true, parms.forFaction);
						precept_Building.ThingDef = requiredRitualAndBuilding.building;
						precept_Building.RegenerateName();
						tmpInitializedPrecepts.Add(precept_Building);
					}
				}
			}
			if (memes[i].requireOne != null)
			{
				for (int num2 = 0; num2 < memes[i].requireOne.Count; num2++)
				{
					PreceptDef preceptDef = memes[i].requireOne[num2].FirstOrDefault();
					if (preceptDef != null && !preceptDef.allowDuplicates)
					{
						IssueDef issue = memes[i].requireOne[num2].First().issue;
						Precept precept2 = ideo.PreceptsListForReading.FirstOrDefault((Precept x) => x.def.issue == issue && !ideo.PreceptIsRequired(x.def));
						if (precept2 != null)
						{
							ideo.RemovePrecept(precept2);
						}
					}
					if (memes[i].requireOne[num2].Where((PreceptDef x) => CanAddForFaction(x, parms.forFaction, parms.disallowedPrecepts, checkDuplicates: true, ignoreMemeRequirements: false, ignoreConflictingMemes: false, parms.classicExtra)).TryRandomElementByWeight((PreceptDef x) => x.selectionWeight, out var result2))
					{
						ideo.AddPrecept(PreceptMaker.MakePrecept(result2), init: true);
						preceptGenerated?.Invoke(result2);
					}
				}
			}
			if (generateWeaponPrecept && memes[i].preferredWeaponClasses != null)
			{
				weaponClassPairsOut?.Add(memes[i].preferredWeaponClasses);
			}
		}
	}

	public void EnsurePreceptsCompatibleWithMemes(List<MemeDef> oldMemes, List<MemeDef> newMemes, IdeoGenerationParms parms)
	{
		tmpPreceptsToRemove.Clear();
		tmpPreceptsToRemove.AddRange(GetPreceptsToRemoveFromMemeChanges(oldMemes, newMemes));
		for (int i = 0; i < tmpPreceptsToRemove.Count; i++)
		{
			ideo.RemovePrecept(tmpPreceptsToRemove[i]);
		}
		tmpPreceptsToRemove.Clear();
		tmpAddedMemes.Clear();
		tmpAddedMemes.AddRange(newMemes.Where((MemeDef m) => !oldMemes.Contains(m)));
		tmpInitializedPrecepts.Clear();
		AddRequiredPreceptsForMemes(tmpAddedMemes, parms, generateWeaponPrecept: false);
		tmpInitializedPrecepts.Clear();
		AddRitualsForMemes(tmpAddedMemes);
		FinalizeIdeo(ideo);
		tmpInitializedPrecepts.Clear();
		tmpAddedMemes.Clear();
	}

	public IEnumerable<Precept> GetPreceptsToRemoveFromMemeChanges(List<MemeDef> prevMemes, List<MemeDef> futureMemes)
	{
		List<Precept> existingPrecepts = ideo.PreceptsListForReading;
		List<MemeDef> newMemes = futureMemes.Where((MemeDef m) => !prevMemes.Contains(m)).ToList();
		for (int i = 0; i < existingPrecepts.Count; i++)
		{
			if (!HasRequiredMemes(existingPrecepts[i].def, futureMemes))
			{
				yield return existingPrecepts[i];
			}
			else if (ConflictsWithNewMemes(existingPrecepts[i], newMemes))
			{
				yield return existingPrecepts[i];
			}
		}
	}

	private bool ConflictsWithNewMemes(Precept precept, List<MemeDef> newMemes)
	{
		for (int i = 0; i < newMemes.Count; i++)
		{
			if (precept.def.conflictingMemes.Contains(newMemes[i]))
			{
				return true;
			}
			if (precept.def.allowDuplicates || newMemes[i].requireOne.NullOrEmpty())
			{
				continue;
			}
			for (int j = 0; j < newMemes[i].requireOne.Count; j++)
			{
				List<PreceptDef> list = newMemes[i].requireOne[j];
				if (list.Count == 0)
				{
					continue;
				}
				int num = 0;
				for (int k = 0; k < list.Count; k++)
				{
					if (list[k] != precept.def && list[k].issue == precept.def.issue)
					{
						num++;
					}
				}
				if (num == list.Count)
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool HasRequiredMemes(PreceptDef preceptDef, List<MemeDef> memes)
	{
		if (!preceptDef.requiredMemes.NullOrEmpty())
		{
			return memes.Any((MemeDef x) => preceptDef.requiredMemes.Contains(x));
		}
		return true;
	}

	public void InitPrecepts(IdeoGenerationParms parms, List<Precept> initializedPrecepts = null)
	{
		Dictionary<MemeDef, int> dictionary = new Dictionary<MemeDef, int>();
		List<Precept> extraPrecepts = new List<Precept>();
		List<Precept> filledPrecepts = new List<Precept>();
		List<Precept> initedPrecepts = new List<Precept>();
		foreach (Precept_Ritual item in ideo.PreceptsListForReading.OfType<Precept_Ritual>())
		{
			RitualPatternDef ritualPatternDef = null;
			string groupTag = item.patternGroupTag ?? item.def.ritualPatternBase?.patternGroupTag;
			if (!groupTag.NullOrEmpty() && (initializedPrecepts == null || !initializedPrecepts.Contains(item)))
			{
				ritualPatternDef = DefDatabase<RitualPatternDef>.AllDefs.Where((RitualPatternDef d) => d.patternGroupTag == groupTag && d.CanFactionUse(parms.forFaction) && !ideo.PreceptsListForReading.OfType<Precept_Ritual>().Any((Precept_Ritual p) => p.behavior != null && p.sourcePattern == d)).RandomElementWithFallback();
			}
			if (ritualPatternDef != null)
			{
				ritualPatternDef.Fill(item);
				filledPrecepts.Add(item);
			}
		}
		HashSet<RitualPatternDef> usedMemePatternDefs = new HashSet<RitualPatternDef>();
		foreach (MemeDef tmpMeme in tmpMemes)
		{
			if (!tmpMemesNumRitualsToMake.ContainsKey(tmpMeme) || (dictionary.ContainsKey(tmpMeme) && dictionary[tmpMeme] >= tmpMemesNumRitualsToMake[tmpMeme]) || tmpMeme.replacementPatterns.NullOrEmpty())
			{
				continue;
			}
			foreach (Precept item2 in ideo.PreceptsListForReading.OrderByDescending((Precept p) => p is Precept_Ritual))
			{
				RitualPatternDef ritualPatternBase = item2.def.ritualPatternBase;
				if (ritualPatternBase != null && !ritualPatternBase.tags.NullOrEmpty() && tmpMeme.replaceRitualsWithTags.Any(ritualPatternBase.tags.Contains))
				{
					int num = 0;
					if (dictionary.TryGetValue(tmpMeme, out var value))
					{
						num = value;
					}
					RitualPatternDef ritualPatternDef2 = tmpMeme.replacementPatterns.Where((RitualPatternDef p) => p.CanFactionUse(parms.forFaction) && !usedMemePatternDefs.Contains(p)).RandomElementWithFallback();
					if (ritualPatternDef2 != null)
					{
						ritualPatternDef2.Fill((Precept_Ritual)item2);
						dictionary.SetOrAdd(tmpMeme, num + 1);
						filledPrecepts.Add(item2);
						usedMemePatternDefs.Add(ritualPatternDef2);
					}
				}
			}
			if (dictionary.ContainsKey(tmpMeme) && dictionary[tmpMeme] >= tmpMemesNumRitualsToMake[tmpMeme])
			{
				continue;
			}
			foreach (PreceptDef item3 in DefDatabase<PreceptDef>.AllDefsListForReading)
			{
				RitualPatternDef pattern = item3.ritualPatternBase;
				if (pattern != null && !pattern.tags.NullOrEmpty() && tmpMeme.replaceRitualsWithTags.Any(pattern.tags.Contains) && !usedMemePatternDefs.Contains(pattern) && !ideo.PreceptsListForReading.OfType<Precept_Ritual>().Any((Precept_Ritual r) => r.behavior != null && r.behavior.def == pattern.ritualBehavior))
				{
					Precept precept = PreceptMaker.MakePrecept(item3);
					ideo.AddPrecept(precept);
					pattern?.Fill((Precept_Ritual)precept);
					filledPrecepts.Add(precept);
					usedMemePatternDefs.Add(pattern);
				}
			}
		}
		AddRitualsForMemes(tmpMemes, filledPrecepts);
		AddAndInitPrecepts();
		foreach (Precept_Ritual item4 in ideo.PreceptsListForReading.OfType<Precept_Ritual>())
		{
			if (item4.behavior == null || item4.behavior.def.preceptRequirements.NullOrEmpty())
			{
				continue;
			}
			foreach (PreceptRequirement preceptRequirement in item4.behavior.def.preceptRequirements)
			{
				if (!preceptRequirement.Met(ideo.PreceptsListForReading) && !preceptRequirement.Met(extraPrecepts))
				{
					extraPrecepts.Add(preceptRequirement.MakePrecept(ideo));
				}
			}
		}
		AddAndInitPrecepts();
		void AddAndInitPrecepts()
		{
			foreach (Precept item5 in extraPrecepts)
			{
				ideo.AddPrecept(item5);
			}
			foreach (Precept item6 in ideo.PreceptsListForReading)
			{
				if (!initedPrecepts.Contains(item6) && (initializedPrecepts == null || !initializedPrecepts.Contains(item6)))
				{
					if (item6 is Precept_Ritual ritual && !filledPrecepts.Contains(item6))
					{
						item6.def.ritualPatternBase?.Fill(ritual);
					}
					item6.Init(ideo, parms.forFaction);
					initedPrecepts.Add(item6);
				}
			}
		}
	}

	private void AddRitualsForMemes(List<MemeDef> memes, List<Precept> outPrecepts = null)
	{
		foreach (PreceptDef p in DefDatabase<PreceptDef>.AllDefsListForReading)
		{
			if (p.ritualPatternBase != null && !p.requiredMemes.NullOrEmpty() && p.requiredMemes.Any(memes.Contains) && !ideo.PreceptsListForReading.OfType<Precept_Ritual>().Any((Precept_Ritual r) => r.sourcePattern == p.ritualPatternBase))
			{
				Precept_Ritual precept_Ritual = (Precept_Ritual)PreceptMaker.MakePrecept(p);
				ideo.AddPrecept(precept_Ritual);
				p.ritualPatternBase.Fill(precept_Ritual);
				precept_Ritual.RegenerateName();
				outPrecepts?.Add(precept_Ritual);
			}
		}
	}

	public virtual void RandomizeIcon()
	{
		LongEventHandler.ExecuteWhenFinished(delegate
		{
			ideo.SetIcon(GetRandomIconDef(ideo), GetRandomColorDef(ideo));
		});
	}

	public virtual void GenerateLeaderTitle()
	{
		if (ideo.classicMode)
		{
			ideo.leaderTitleMale = PreceptDefOf.IdeoRole_Leader.label;
			ideo.leaderTitleFemale = ideo.leaderTitleMale;
			return;
		}
		if (ideo.culture.leaderTitleMaker == null)
		{
			ideo.leaderTitleMale = null;
			ideo.leaderTitleFemale = null;
			return;
		}
		GrammarRequest request = new GrammarRequest
		{
			Includes = { ideo.culture.leaderTitleMaker }
		};
		for (int i = 0; i < ideo.memes.Count; i++)
		{
			if (ideo.memes[i].generalRules != null)
			{
				request.IncludesBare.Add(ideo.memes[i].generalRules);
			}
		}
		ideo.leaderTitleMale = NameGenerator.GenerateName(request, null, appendNumberIfNameUsed: false, "r_leaderTitle");
		ideo.leaderTitleFemale = ideo.leaderTitleMale;
	}

	public AcceptanceReport CanAdd(PreceptDef precept, bool checkDuplicates = true)
	{
		List<Precept> preceptsListForReading = ideo.PreceptsListForReading;
		if (precept.takeNameFrom != null)
		{
			bool flag = false;
			foreach (Precept item in ideo.PreceptsListForReading)
			{
				if (item.def == precept.takeNameFrom)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		if (precept.preceptClass == typeof(Precept_RoleMulti) && ideo.PreceptsListForReading.Count((Precept p) => p is Precept_RoleMulti && p.def.visible) >= 2)
		{
			return "MaxMultiRolesCount".Translate(2);
		}
		if (!precept.requiredMemes.NullOrEmpty())
		{
			bool flag2 = false;
			foreach (MemeDef requiredMeme in precept.requiredMemes)
			{
				if (ideo.memes.Contains(requiredMeme))
				{
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				if (precept.requiredMemes.Count == 1)
				{
					return new AcceptanceReport("RequiresMeme".Translate() + ": " + precept.requiredMemes[0].LabelCap);
				}
				return new AcceptanceReport("RequiresOneOfMemes".Translate() + ": " + precept.RequiredMemeLabels.ToCommaList().CapitalizeFirst());
			}
		}
		for (int num = 0; num < precept.conflictingMemes.Count; num++)
		{
			if (ideo.memes.Contains(precept.conflictingMemes[num]))
			{
				if (precept.conflictingMemes.Count == 1)
				{
					return new AcceptanceReport("ConflictsWithMeme".Translate() + ": " + precept.conflictingMemes[0].LabelCap);
				}
				return new AcceptanceReport("ConflictsWithMemes".Translate() + ": " + precept.conflictingMemes.Select((MemeDef m) => m.label).ToCommaList().CapitalizeFirst());
			}
		}
		for (int num2 = 0; num2 < preceptsListForReading.Count; num2++)
		{
			if (checkDuplicates)
			{
				if (precept == preceptsListForReading[num2].def)
				{
					if (!precept.allowDuplicates)
					{
						return false;
					}
				}
				else if (!precept.issue.allowMultiplePrecepts && precept.issue == preceptsListForReading[num2].def.issue)
				{
					return false;
				}
			}
			else if (precept.issue == preceptsListForReading[num2].def.issue && ideo.PreceptIsRequired(preceptsListForReading[num2].def))
			{
				return ideo.PreceptIsRequired(precept);
			}
			for (int num3 = 0; num3 < precept.exclusionTags.Count; num3++)
			{
				if (preceptsListForReading[num2].def.exclusionTags.Contains(precept.exclusionTags[num3]) && (preceptsListForReading[num2].def.issue != precept.issue || checkDuplicates))
				{
					return false;
				}
			}
		}
		return true;
	}

	public void AddPlaceRules(ref GrammarRequest request)
	{
		if (place?.placeRules != null)
		{
			request.IncludesBare.Add(place.placeRules);
		}
	}

	public virtual void CopyTo(IdeoFoundation other)
	{
		other.place = place;
		other.def = def;
	}
}
