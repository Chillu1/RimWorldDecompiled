using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;

namespace Verse;

public class PawnKindDef : Def
{
	public ThingDef race;

	[LoadAlias("defaultFactionType")]
	public FactionDef defaultFactionDef;

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

	[XmlInheritanceAllowDuplicateNodes]
	public List<TraitRequirement> forcedTraits;

	[XmlInheritanceAllowDuplicateNodes]
	public List<TraitRequirement> disallowedTraitsWithDegree;

	public List<TraitDef> disallowedTraits;

	public float alternateGraphicChance;

	public MutantDef mutant;

	public XenotypeSet xenotypeSet;

	public bool useFactionXenotypes = true;

	[LoadAlias("hairTags")]
	public List<StyleItemTagWeighted> styleItemTags;

	public HairDef forcedHair;

	public Color? forcedHairColor;

	public List<MissingPart> missingParts;

	public RulePackDef nameMaker;

	public RulePackDef nameMakerFemale;

	public List<AbilityDef> abilities;

	public bool preventIdeo;

	public bool studiableAsPrisoner;

	public bool isBoss;

	public Dictionary<string, float> moveSpeedFactorByTerrainTag = new Dictionary<string, float>();

	public List<BackstoryDef> fixedChildBackstories = new List<BackstoryDef>();

	public List<BackstoryDef> fixedAdultBackstories = new List<BackstoryDef>();

	public float backstoryCryptosleepCommonality;

	public FloatRange? chronologicalAgeRange;

	public int minGenerationAge;

	public int maxGenerationAge = 999999;

	public bool factionLeader;

	public Gender? fixedGender;

	public bool allowOldAgeInjuries = true;

	public bool generateInitialNonFamilyRelations = true;

	public DevelopmentalStage? pawnGroupDevelopmentStage;

	public bool destroyGearOnDrop;

	public bool canStrip = true;

	public float defendPointRadius = -1f;

	public bool factionHostileOnKill;

	public bool factionHostileOnDeath;

	public bool hostileToAll;

	public FloatRange? initialResistanceRange;

	public FloatRange? initialWillRange;

	public bool forceNoDeathNotification;

	public bool skipResistant;

	public float controlGroupPortraitZoom = 1f;

	public float? overrideDeathOnDownedChance;

	public bool forceDeathOnDowned;

	public bool immuneToGameConditionEffects;

	public bool immuneToTraps;

	public bool collidesWithPawns = true;

	public bool ignoresPainShock;

	public bool canMeleeAttack = true;

	public float basePrisonBreakMtbDays = 60f;

	public bool useFixedRotation;

	public Rot4 fixedRotation;

	public bool showInDebugSpawner = true;

	public bool canOpenAnyDoor;

	public bool canOpenDoors = true;

	[NoTranslate]
	public string overrideDebugActionCategory;

	public float royalTitleChance;

	public RoyalTitleDef titleRequired;

	public RoyalTitleDef minTitleRequired;

	public List<RoyalTitleDef> titleSelectOne;

	public bool allowRoyalRoomRequirements = true;

	public bool allowRoyalApparelRequirements = true;

	public List<InfectionPathwayDef> meleeAttackInfectionPathways;

	public List<InfectionPathwayDef> rangedAttackInfectionPathways;

	public bool isFighter = true;

	public float combatPower = -1f;

	public bool canArriveManhunter = true;

	public bool canBeSapper;

	public bool isGoodBreacher;

	public bool allowInMechClusters = true;

	public int maxPerGroup = int.MaxValue;

	public bool isGoodPsychicRitualInvoker;

	public bool canBeScattered = true;

	public bool appearsRandomlyInCombatGroups = true;

	public bool aiAvoidCover;

	public FloatRange fleeHealthThresholdRange = new FloatRange(-0.4f, 0.4f);

	public float acceptArrestChanceFactor = 1f;

	public bool canUseAvoidGrid;

	public QualityCategory itemQuality = QualityCategory.Normal;

	public QualityCategory? forceWeaponQuality;

	public bool forceNormalGearQuality;

	public FloatRange gearHealthRange = FloatRange.One;

	public FloatRange weaponMoney = FloatRange.Zero;

	[NoTranslate]
	public List<string> weaponTags;

	public ThingDef weaponStuffOverride;

	public ThingStyleDef weaponStyleDef;

	public FloatRange apparelMoney = FloatRange.Zero;

	public List<ThingDef> apparelRequired;

	[NoTranslate]
	public List<string> apparelTags;

	[NoTranslate]
	public List<string> apparelDisallowTags;

	public float apparelAllowHeadgearChance = 1f;

	public bool ignoreApparelAllowChance;

	public bool apparelIgnoreSeasons;

	public bool apparelIgnorePollution;

	public bool ignoreFactionApparelStuffRequirements;

	public Color apparelColor = Color.white;

	public Color? skinColorOverride;

	public ColorDef favoriteColor;

	public bool ignoreIdeoApparelColors;

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

	public float humanPregnancyChance = 0.03f;

	public List<StartingHediff> startingHediffs;

	public float nakedChance;

	public List<MiscDamage> existingDamage = new List<MiscDamage>();

	public QualityCategory minApparelQuality;

	public QualityCategory maxApparelQuality = QualityCategory.Excellent;

	public List<ThingDefCountClass> fixedInventory = new List<ThingDefCountClass>();

	public PawnInventoryOption inventoryOptions;

	public float invNutrition;

	public ThingDef invFoodDef;

	public float chemicalAddictionChance;

	public float combatEnhancingDrugsChance;

	public IntRange combatEnhancingDrugsCount = IntRange.Zero;

	public List<ChemicalDef> forcedAddictions = new List<ChemicalDef>();

	public bool trader;

	public List<SkillRange> skills;

	public WorkTags requiredWorkTags;

	public WorkTags disabledWorkTags;

	public int extraSkillLevels;

	public int minTotalSkillLevels;

	public int minBestSkillLevel;

	[MustTranslate]
	public string labelMale;

	[MustTranslate]
	public string labelMalePlural;

	[MustTranslate]
	public string labelFemale;

	[MustTranslate]
	public string labelFemalePlural;

	public IntRange wildGroupSize = IntRange.One;

	public float ecoSystemWeight = 1f;

	[NoTranslate]
	public string flyingAnimationFramePathPrefix;

	[NoTranslate]
	public string flyingAnimationFramePathPrefixFemale;

	public int flyingAnimationFrameCount;

	public int flyingAnimationTicksPerFrame = -1;

	public float flyingAnimationDrawSize = 1f;

	public bool flyingAnimationDrawSizeIsMultiplier;

	public bool flyingAnimationInheritColors;

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

	public string GetLabelGendered(Gender gender)
	{
		if (gender == Gender.Female && !labelFemale.NullOrEmpty())
		{
			return labelFemale;
		}
		if (gender == Gender.Male && !labelMale.NullOrEmpty())
		{
			return labelMale;
		}
		return label;
	}

	public RulePackDef GetNameMaker(Gender gender)
	{
		if (gender == Gender.Female && nameMakerFemale != null)
		{
			return nameMakerFemale;
		}
		if (nameMaker != null)
		{
			return nameMaker;
		}
		return null;
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
		LongEventHandler.ExecuteWhenFinished(delegate
		{
			foreach (PawnKindLifeStage lifeStage in lifeStages)
			{
				if (lifeStage.swimmingGraphicData != null && lifeStage.swimmingGraphicData.shaderType == null)
				{
					lifeStage.swimmingGraphicData.shaderType = ShaderTypeDefOf.Transparent;
				}
				if (lifeStage.femaleSwimmingGraphicData != null && lifeStage.femaleSwimmingGraphicData.shaderType == null)
				{
					lifeStage.femaleSwimmingGraphicData.shaderType = ShaderTypeDefOf.Transparent;
				}
				if (lifeStage.stationaryGraphicData != null && lifeStage.stationaryGraphicData.shaderType == null)
				{
					lifeStage.stationaryGraphicData.shaderType = ShaderTypeDefOf.Transparent;
				}
				if (lifeStage.femaleStationaryGraphicData != null && lifeStage.femaleStationaryGraphicData.shaderType == null)
				{
					lifeStage.femaleStationaryGraphicData.shaderType = ShaderTypeDefOf.Transparent;
				}
			}
		});
	}

	public float GetAnimalPointsToHuntOrSlaughter()
	{
		return combatPower * 5f * (1f + RaceProps.manhunterOnDamageChance * 0.5f) * (1f + RaceProps.manhunterOnTameFailChance * 0.2f) * (1f + race.GetStatValueAbstract(StatDefOf.Wildness)) + race.BaseMarketValue;
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
		if (combatPower < 0f)
		{
			yield return defName + " has no combatPower.";
		}
		if (weaponMoney != FloatRange.Zero)
		{
			if (weaponTags == null)
			{
				yield return "weaponMoney is set but weaponTags is not.";
			}
			else
			{
				float num = 999999f;
				int i;
				for (i = 0; i < weaponTags.Count; i++)
				{
					IEnumerable<ThingDef> source = DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => d.weaponTags != null && d.weaponTags.Contains(weaponTags[i]));
					if (source.Any())
					{
						num = Mathf.Min(num, source.Min((Func<ThingDef, float>)PawnWeaponGenerator.CheapestNonDerpPriceFor));
					}
				}
				if (num < 999999f && num > weaponMoney.min)
				{
					yield return "Cheapest weapon with one of my weaponTags costs " + num + " but weaponMoney min is " + weaponMoney.min + ", so could end up weaponless.";
				}
			}
		}
		if (!RaceProps.Humanlike && lifeStages.Count != RaceProps.lifeStageAges.Count)
		{
			yield return "PawnKindDef defines " + lifeStages.Count + " lifeStages while race def defines " + RaceProps.lifeStageAges.Count;
		}
		if (apparelRequired != null)
		{
			for (int i2 = 0; i2 < apparelRequired.Count; i2++)
			{
				for (int j = i2 + 1; j < apparelRequired.Count; j++)
				{
					if (!ApparelUtility.CanWearTogether(apparelRequired[i2], apparelRequired[j], race.race.body))
					{
						yield return "required apparel can't be worn together (" + apparelRequired[i2]?.ToString() + ", " + apparelRequired[j]?.ToString() + ")";
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
		if (RaceProps.Humanlike && !initialResistanceRange.HasValue)
		{
			yield return "initial resistance range is undefined for humanlike pawn kind.";
		}
		if (RaceProps.Humanlike && !initialWillRange.HasValue)
		{
			yield return "initial will range is undefined for humanlike pawn kind.";
		}
		if (startingHediffs == null)
		{
			yield break;
		}
		foreach (StartingHediff startingHediff in startingHediffs)
		{
			if (startingHediff.durationTicksRange.HasValue && startingHediff.def.CompProps<HediffCompProperties_Disappears>() == null)
			{
				yield return "starting hediff " + startingHediff.def.defName + " has duration ticks set but doesn't have Disappears comp.";
			}
		}
	}

	public static PawnKindDef Named(string defName)
	{
		return DefDatabase<PawnKindDef>.GetNamed(defName);
	}
}
