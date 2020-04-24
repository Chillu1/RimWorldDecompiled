using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public class PawnKindDef : Def
	{
		public ThingDef race;

		public FactionDef defaultFactionType;

		[NoTranslate]
		public List<BackstoryCategoryFilter> backstoryFilters;

		[NoTranslate]
		public List<BackstoryCategoryFilter> backstoryFiltersOverride;

		[NoTranslate]
		public List<string> backstoryCategories;

		[MustTranslate]
		public string labelPlural;

		public List<PawnKindLifeStage> lifeStages = new List<PawnKindLifeStage>();

		public List<AlternateGraphic> alternateGraphics;

		public List<TraitDef> disallowedTraits;

		public float alternateGraphicChance;

		public float backstoryCryptosleepCommonality;

		public int minGenerationAge;

		public int maxGenerationAge = 999999;

		public bool factionLeader;

		public bool destroyGearOnDrop;

		public float defendPointRadius = -1f;

		public float royalTitleChance;

		public RoyalTitleDef titleRequired;

		public List<RoyalTitleDef> titleSelectOne;

		public bool allowRoyalRoomRequirements = true;

		public bool allowRoyalApparelRequirements = true;

		public bool isFighter = true;

		public float combatPower = -1f;

		public bool canArriveManhunter = true;

		public bool canBeSapper;

		public float baseRecruitDifficulty = 0.5f;

		public bool aiAvoidCover;

		public FloatRange fleeHealthThresholdRange = new FloatRange(-0.4f, 0.4f);

		public QualityCategory itemQuality = QualityCategory.Normal;

		public bool forceNormalGearQuality;

		public FloatRange gearHealthRange = FloatRange.One;

		public FloatRange weaponMoney = FloatRange.Zero;

		[NoTranslate]
		public List<string> weaponTags;

		public FloatRange apparelMoney = FloatRange.Zero;

		public List<ThingDef> apparelRequired;

		[NoTranslate]
		public List<string> apparelTags;

		[NoTranslate]
		public List<string> apparelDisallowTags;

		[NoTranslate]
		public List<string> hairTags;

		public float apparelAllowHeadgearChance = 1f;

		public bool apparelIgnoreSeasons;

		public Color apparelColor = Color.white;

		public List<SpecificApparelRequirement> specificApparelRequirements;

		public List<ThingDef> techHediffsRequired;

		public FloatRange techHediffsMoney = FloatRange.Zero;

		[NoTranslate]
		public List<string> techHediffsTags;

		[NoTranslate]
		public List<string> techHediffsDisallowTags;

		public float techHediffsChance;

		public int techHediffsMaxAmount = 1;

		public float biocodeWeaponChance;

		public List<ThingDefCountClass> fixedInventory = new List<ThingDefCountClass>();

		public PawnInventoryOption inventoryOptions;

		public float invNutrition;

		public ThingDef invFoodDef;

		public float chemicalAddictionChance;

		public float combatEnhancingDrugsChance;

		public IntRange combatEnhancingDrugsCount = IntRange.zero;

		public bool trader;

		public List<SkillRange> skills;

		[MustTranslate]
		public string labelMale;

		[MustTranslate]
		public string labelMalePlural;

		[MustTranslate]
		public string labelFemale;

		[MustTranslate]
		public string labelFemalePlural;

		public IntRange wildGroupSize = IntRange.one;

		public float ecoSystemWeight = 1f;

		private const int MaxWeaponMoney = 999999;

		public RaceProperties RaceProps => race.race;

		public override void ResolveReferences()
		{
			base.ResolveReferences();
			for (int i = 0; i < lifeStages.Count; i++)
			{
				lifeStages[i].ResolveReferences();
			}
		}

		public string GetLabelPlural(int count = -1)
		{
			if (!labelPlural.NullOrEmpty())
			{
				return labelPlural;
			}
			return Find.ActiveLanguageWorker.Pluralize(label, count);
		}

		public override void PostLoad()
		{
			if (backstoryCategories != null && backstoryCategories.Count > 0)
			{
				if (backstoryFilters == null)
				{
					backstoryFilters = new List<BackstoryCategoryFilter>();
				}
				backstoryFilters.Add(new BackstoryCategoryFilter
				{
					categories = backstoryCategories
				});
			}
		}

		public float GetAnimalPointsToHuntOrSlaughter()
		{
			return combatPower * 5f * (1f + RaceProps.manhunterOnDamageChance * 0.5f) * (1f + RaceProps.manhunterOnTameFailChance * 0.2f) * (1f + RaceProps.wildness) + race.BaseMarketValue;
		}

		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string item in base.ConfigErrors())
			{
				yield return item;
			}
			if (backstoryFilters != null && backstoryFiltersOverride != null)
			{
				yield return "both backstoryCategories and backstoryCategoriesOverride are defined";
			}
			if (race == null)
			{
				yield return "no race";
			}
			if (baseRecruitDifficulty > 1.0001f)
			{
				yield return defName + " recruitDifficulty is greater than 1. 1 means impossible to recruit.";
			}
			if (combatPower < 0f)
			{
				yield return defName + " has no combatPower.";
			}
			if (weaponMoney != FloatRange.Zero)
			{
				float num = 999999f;
				int k;
				for (k = 0; k < weaponTags.Count; k++)
				{
					IEnumerable<ThingDef> source = DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => d.weaponTags != null && d.weaponTags.Contains(weaponTags[k]));
					if (source.Any())
					{
						num = Mathf.Min(num, source.Min((ThingDef d) => PawnWeaponGenerator.CheapestNonDerpPriceFor(d)));
					}
				}
				if (num < 999999f && num > weaponMoney.min)
				{
					yield return "Cheapest weapon with one of my weaponTags costs " + num + " but weaponMoney min is " + weaponMoney.min + ", so could end up weaponless.";
				}
			}
			if (!RaceProps.Humanlike && lifeStages.Count != RaceProps.lifeStageAges.Count)
			{
				yield return "PawnKindDef defines " + lifeStages.Count + " lifeStages while race def defines " + RaceProps.lifeStageAges.Count;
			}
			if (apparelRequired != null)
			{
				for (int j = 0; j < apparelRequired.Count; j++)
				{
					for (int i = j + 1; i < apparelRequired.Count; i++)
					{
						if (!ApparelUtility.CanWearTogether(apparelRequired[j], apparelRequired[i], race.race.body))
						{
							yield return "required apparel can't be worn together (" + apparelRequired[j] + ", " + apparelRequired[i] + ")";
						}
					}
				}
			}
			if (alternateGraphics != null)
			{
				foreach (AlternateGraphic alternateGraphic in alternateGraphics)
				{
					if (alternateGraphic.Weight < 0f)
					{
						yield return "alternate graphic has negative weight.";
					}
				}
			}
		}

		public static PawnKindDef Named(string defName)
		{
			return DefDatabase<PawnKindDef>.GetNamed(defName);
		}
	}
}
