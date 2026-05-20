using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse.AI;

namespace Verse;

public class RaceProperties
{
	public Intelligence intelligence;

	private FleshTypeDef fleshType;

	private ThingDef bloodDef;

	private ThingDef bloodSmearDef;

	public bool hasGenders = true;

	public Gender forceGender;

	public bool needsRest = true;

	public ThinkTreeDef thinkTreeMain;

	public ThinkTreeDef thinkTreeConstant;

	public DutyDef dutyBoss;

	public PawnNameCategory nameCategory;

	public FoodTypeFlags foodType;

	public BodyDef body;

	public DeathActionProperties deathAction = new DeathActionProperties();

	public List<AnimalBiomeRecord> wildBiomes;

	public SimpleCurve ageGenerationCurve;

	public bool makesFootprints;

	public int executionRange = 2;

	public float lifeExpectancy = 10f;

	public List<HediffGiverSetDef> hediffGiverSets;

	public float? roamMtbDays;

	public bool allowedOnCaravan = true;

	public bool canReleaseToWild = true;

	public bool playerCanChangeMaster = true;

	public bool showTrainables = true;

	public bool hideTrainingTab;

	public bool doesntMove;

	public PawnRenderTreeDef renderTree;

	public AnimationDef startingAnimation;

	public ThingDef linkedCorpseKind;

	public bool canOpenFactionlessDoors = true;

	public bool alwaysAwake;

	public bool alwaysViolent;

	public bool isImmuneToInfections;

	public float bleedRateFactor = 1f;

	public bool canBecomeShambler;

	public bool neverIncludeInQuests;

	public bool canBeVacuumBurnt = true;

	public bool herdAnimal;

	public bool packAnimal;

	public bool predator;

	public float maxPreyBodySize = 99999f;

	public float petness;

	public float nuzzleMtbHours = -1f;

	public float manhunterOnDamageChance;

	public float manhunterOnTameFailChance;

	public bool canBePredatorPrey = true;

	public bool herdMigrationAllowed = true;

	public AnimalType animalType;

	public List<ThingDef> willNeverEat;

	public bool giveNonToolUserBeatFireVerb;

	public bool disableIgniteVerb;

	public bool disableAreaControl;

	public bool waterSeeker;

	public int? waterCellCost;

	public bool disableMating;

	public List<ThingDef> canCrossBreedWith;

	public List<ThingDef> crossAggroWith;

	public bool canFishForFood;

	public bool canFlyInVacuum;

	public PawnKindDef manhunterPackUseLabelFrom;

	public float flightStartChanceOnJobStart;

	public float flightSpeedFactor = 2.8f;

	public bool canFlyIntoMap;

	public bool canLeaveMapFlying;

	public float leaveMapOnFleeChance;

	public int maxMechEnergy = 100;

	public List<WorkTypeDef> mechEnabledWorkTypes = new List<WorkTypeDef>();

	public int mechFixedSkillLevel = 10;

	public List<MechWorkTypePriority> mechWorkTypePriorities;

	public int? bulletStaggerDelayTicks;

	public float? bulletStaggerSpeedFactor;

	public EffecterDef bulletStaggerEffecterDef;

	public bool bulletStaggerIgnoreBodySize;

	public MechWeightClassDef mechWeightClass;

	public List<DetritusLeavingType> detritusLeavings = new List<DetritusLeavingType>();

	public float gestationPeriodDays = -1f;

	public SimpleCurve litterSizeCurve;

	public float mateMtbHours = 12f;

	[NoTranslate]
	public List<string> untrainableTags;

	[NoTranslate]
	public List<string> trainableTags;

	public TrainabilityDef trainability;

	public List<TrainableDef> specialTrainables;

	private RulePackDef nameGenerator;

	private RulePackDef nameGeneratorFemale;

	public float nameOnTameChance;

	public float baseBodySize = 1f;

	public float baseHealthScale = 1f;

	public float baseHungerRate = 1f;

	public List<LifeStageAge> lifeStageAges = new List<LifeStageAge>();

	public List<LifeStageWorkSettings> lifeStageWorkSettings = new List<LifeStageWorkSettings>();

	public bool hasMeat = true;

	[MustTranslate]
	public string meatLabel;

	public Color meatColor = Color.white;

	public float meatMarketValue = 2f;

	public ThingDef specificMeatDef;

	public ThingDef useMeatFrom;

	public ThingDef useLeatherFrom;

	public bool hasCorpse = true;

	public bool hasUnnaturalCorpse;

	public bool corpseHiddenWhileUndiscovered;

	public ThingDef leatherDef;

	public ShadowData specialShadowData;

	public List<Vector3> headPosPerRotation;

	public IntRange soundCallIntervalRange = new IntRange(2000, 4000);

	public float soundCallIntervalFriendlyFactor = 1f;

	public float soundCallIntervalAggressiveFactor = 0.25f;

	public SoundDef soundMeleeHitPawn;

	public SoundDef soundMeleeHitBuilding;

	public SoundDef soundMeleeMiss;

	public SoundDef soundMeleeDodge;

	public SoundDef soundAmbience;

	public SoundDef soundMoving;

	public SoundDef soundEating;

	public KnowledgeCategoryDef knowledgeCategory;

	public int anomalyKnowledge;

	[Unsaved(false)]
	public ThingDef meatDef;

	[Unsaved(false)]
	public ThingDef corpseDef;

	[Unsaved(false)]
	public ThingDef unnaturalCorpseDef;

	[Unsaved(false)]
	private PawnKindDef cachedAnyPawnKind;

	public bool Humanlike => (int)intelligence >= 2;

	public bool ToolUser => (int)intelligence >= 1;

	public bool Animal
	{
		get
		{
			if (!ToolUser && IsFlesh)
			{
				return !IsAnomalyEntity;
			}
			return false;
		}
	}

	public bool Insect => FleshType == FleshTypeDefOf.Insectoid;

	public bool Dryad => animalType == AnimalType.Dryad;

	public bool ShouldHaveAbilityTracker
	{
		get
		{
			if (!Humanlike)
			{
				return IsMechanoid;
			}
			return true;
		}
	}

	public bool EatsFood => foodType != FoodTypeFlags.None;

	public float FoodLevelPercentageWantEat => ResolvedDietCategory switch
	{
		DietCategory.NeverEats => 0.3f, 
		DietCategory.Omnivorous => 0.3f, 
		DietCategory.Carnivorous => 0.3f, 
		DietCategory.Ovivorous => 0.4f, 
		DietCategory.Herbivorous => 0.45f, 
		DietCategory.Dendrovorous => 0.45f, 
		_ => throw new InvalidOperationException(), 
	};

	public DietCategory ResolvedDietCategory
	{
		get
		{
			if (!EatsFood)
			{
				return DietCategory.NeverEats;
			}
			if (Eats(FoodTypeFlags.Tree))
			{
				return DietCategory.Dendrovorous;
			}
			if (Eats(FoodTypeFlags.Meat))
			{
				if (Eats(FoodTypeFlags.VegetableOrFruit) || Eats(FoodTypeFlags.Plant))
				{
					return DietCategory.Omnivorous;
				}
				return DietCategory.Carnivorous;
			}
			if (Eats(FoodTypeFlags.AnimalProduct))
			{
				return DietCategory.Ovivorous;
			}
			return DietCategory.Herbivorous;
		}
	}

	public DeathActionWorker DeathActionWorker => deathAction.Worker;

	public FleshTypeDef FleshType => fleshType ?? FleshTypeDefOf.Normal;

	public bool IsMechanoid => FleshType == FleshTypeDefOf.Mechanoid;

	public bool IsFlesh => FleshType.isOrganic;

	public bool IsAnomalyEntity
	{
		get
		{
			if (ModsConfig.AnomalyActive)
			{
				if (FleshType != FleshTypeDefOf.EntityMechanical && FleshType != FleshTypeDefOf.EntityFlesh)
				{
					return FleshType == FleshTypeDefOf.Fleshbeast;
				}
				return true;
			}
			return false;
		}
	}

	public bool IsDrone
	{
		get
		{
			if (ModsConfig.OdysseyActive)
			{
				return FleshType == FleshTypeDefOf.Drone;
			}
			return false;
		}
	}

	public ThingDef BloodDef => bloodDef;

	public ThingDef BloodSmearDef => bloodSmearDef;

	public bool CanDoHerdMigration
	{
		get
		{
			if (Animal)
			{
				return herdMigrationAllowed;
			}
			return false;
		}
	}

	public bool CanPassFences => !FenceBlocked;

	public bool FenceBlocked => Roamer;

	public bool Roamer => roamMtbDays.HasValue;

	public bool IsWorkMech => !mechEnabledWorkTypes.NullOrEmpty();

	public PawnKindDef AnyPawnKind
	{
		get
		{
			if (cachedAnyPawnKind == null)
			{
				List<PawnKindDef> allDefsListForReading = DefDatabase<PawnKindDef>.AllDefsListForReading;
				for (int i = 0; i < allDefsListForReading.Count; i++)
				{
					if (allDefsListForReading[i].race.race == this)
					{
						cachedAnyPawnKind = allDefsListForReading[i];
						break;
					}
				}
			}
			return cachedAnyPawnKind;
		}
	}

	public RulePackDef GetNameGenerator(Gender gender)
	{
		if (gender == Gender.Female && nameGeneratorFemale != null)
		{
			return nameGeneratorFemale;
		}
		return nameGenerator;
	}

	public bool CanEverEat(Thing t)
	{
		return CanEverEat(t.def);
	}

	public bool CanEverEat(ThingDef t)
	{
		if (!EatsFood)
		{
			return false;
		}
		if (t.ingestible == null)
		{
			return false;
		}
		if (t.ingestible.preferability == FoodPreferability.Undefined)
		{
			return false;
		}
		if (willNeverEat != null && willNeverEat.Contains(t))
		{
			return false;
		}
		return Eats(t.ingestible.foodType);
	}

	public bool Eats(FoodTypeFlags food)
	{
		if (!EatsFood)
		{
			return false;
		}
		return (foodType & food) != 0;
	}

	public void ResolveReferencesSpecial()
	{
		if (specificMeatDef != null)
		{
			meatDef = specificMeatDef;
		}
		else if (useMeatFrom != null)
		{
			meatDef = useMeatFrom.race.meatDef;
		}
		if (useLeatherFrom != null)
		{
			leatherDef = useLeatherFrom.race.leatherDef;
		}
	}

	public IEnumerable<string> ConfigErrors(ThingDef thingDef)
	{
		if (thingDef.IsCorpse)
		{
			yield break;
		}
		if (predator && !Eats(FoodTypeFlags.Meat))
		{
			yield return "predator but doesn't eat meat";
		}
		if (canFishForFood && !Eats(FoodTypeFlags.Meat))
		{
			yield return "canFishForFood but doesn't eat meat";
		}
		for (int i = 0; i < lifeStageAges.Count; i++)
		{
			for (int j = 0; j < i; j++)
			{
				if (lifeStageAges[j].minAge > lifeStageAges[i].minAge)
				{
					yield return "lifeStages minAges are not in ascending order";
				}
			}
		}
		if (thingDef.IsCaravanRideable() && !lifeStageAges.Any((LifeStageAge s) => s.def.caravanRideable))
		{
			yield return "must contain at least one lifeStage with caravanRideable when CaravanRidingSpeedFactor is defined";
		}
		if (litterSizeCurve != null)
		{
			foreach (string item in litterSizeCurve.ConfigErrors("litterSizeCurve"))
			{
				yield return item;
			}
		}
		if (nameOnTameChance > 0f && nameGenerator == null)
		{
			yield return "can be named, but has no nameGenerator";
		}
		if (specificMeatDef != null && useMeatFrom != null)
		{
			yield return "specificMeatDef and useMeatFrom are both non-null. specificMeatDef will be chosen.";
		}
		if (useMeatFrom != null && useMeatFrom.category != ThingCategory.Pawn)
		{
			yield return "tries to use meat from non-pawn " + useMeatFrom;
		}
		if (useMeatFrom?.race.useMeatFrom != null)
		{
			yield return "tries to use meat from " + useMeatFrom?.ToString() + " which uses meat from " + useMeatFrom.race.useMeatFrom;
		}
		if (useLeatherFrom != null && useLeatherFrom.category != ThingCategory.Pawn)
		{
			yield return "tries to use leather from non-pawn " + useLeatherFrom;
		}
		if (useLeatherFrom != null && useLeatherFrom.race.useLeatherFrom != null)
		{
			yield return "tries to use leather from " + useLeatherFrom?.ToString() + " which uses leather from " + useLeatherFrom.race.useLeatherFrom;
		}
		if (Animal && trainability == null)
		{
			yield return "animal has trainability = null";
		}
		if (fleshType == FleshTypeDefOf.Normal && gestationPeriodDays < 0f)
		{
			yield return "normal flesh but gestationPeriodDays not configured.";
		}
		if (IsMechanoid && thingDef.butcherProducts.NullOrEmpty())
		{
			yield return thingDef.label + " mech has no butcher products set";
		}
		foreach (string item2 in deathAction.ConfigErrors())
		{
			yield return item2;
		}
		if (renderTree == null)
		{
			yield return "renderTree is null";
		}
		if (canCrossBreedWith == null)
		{
			yield break;
		}
		foreach (ThingDef item3 in canCrossBreedWith)
		{
			if (item3.race == null || !item3.race.Animal)
			{
				yield return "tries to set canCrossBreedWith " + item3.defName + " but that is not an animal";
			}
		}
	}

	public IEnumerable<StatDrawEntry> SpecialDisplayStats(ThingDef parentDef, StatRequest req)
	{
		Pawn pawnThing = req.Pawn ?? (req.Thing as Pawn);
		if (!ModsConfig.BiotechActive || !Humanlike || pawnThing?.genes == null || pawnThing.genes.Xenotype == XenotypeDefOf.Baseliner)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "Race".Translate(), parentDef.LabelCap, parentDef.description, 4205, null, null, forceUnfinalizedMode: false, overridesHideStats: true);
		}
		if (pawnThing != null && pawnThing.IsMutant && pawnThing.mutant.Def.overrideFoodType)
		{
			string text = pawnThing.mutant.Def.foodType.ToHumanString().CapitalizeFirst();
			yield return new StatDrawEntry(StatCategoryDefOf.PawnFood, "Diet".Translate(), text, "Stat_Race_Diet_Desc".Translate(text), 1500);
		}
		else if (!parentDef.race.IsMechanoid && !parentDef.race.IsAnomalyEntity && !parentDef.race.IsDrone)
		{
			string text2 = foodType.ToHumanString().CapitalizeFirst();
			yield return new StatDrawEntry(StatCategoryDefOf.PawnFood, "Diet".Translate(), text2, "Stat_Race_Diet_Desc".Translate(text2), 1500);
		}
		if (pawnThing != null && pawnThing.needs?.food != null)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.PawnFood, "FoodConsumption".Translate(), NutritionEatenPerDay(pawnThing), NutritionEatenPerDayExplanation(pawnThing), 1600);
		}
		if (parentDef.race.leatherDef != null)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "LeatherType".Translate(), parentDef.race.leatherDef.LabelCap, "Stat_Race_LeatherType_Desc".Translate(), 3550, null, new Dialog_InfoCard.Hyperlink[1]
			{
				new Dialog_InfoCard.Hyperlink(parentDef.race.leatherDef)
			});
		}
		if (parentDef.race.Animal || ((pawnThing?.IsWildMan() ?? false) && parentDef.GetStatValueAbstract(StatDefOf.Wildness) > 0f))
		{
			float f = ((pawnThing == null) ? PawnUtility.GetManhunterOnDamageChance(parentDef) : PawnUtility.GetManhunterOnDamageChance(pawnThing));
			yield return new StatDrawEntry(StatCategoryDefOf.Animals, "HarmedRevengeChance".Translate(), f.ToStringPercent(), PawnUtility.GetManhunterOnDamageChanceExplanation(parentDef, pawnThing), 510);
			float f2 = ((pawnThing == null) ? PawnUtility.GetManhunterOnTameFailChance(parentDef) : PawnUtility.GetManhunterOnTameFailChance(pawnThing));
			yield return new StatDrawEntry(StatCategoryDefOf.Animals, "TameFailedRevengeChance".Translate(), f2.ToStringPercent(), PawnUtility.GetManhunterOnTameFailChanceExplanation(parentDef, pawnThing), 511);
		}
		TrainabilityDef trainabilityDef = TrainableUtility.GetTrainability(pawnThing) ?? trainability;
		if ((int)intelligence < 2 && trainabilityDef != null && !parentDef.race.IsAnomalyEntity && !parentDef.race.IsDrone)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("Stat_Race_Trainability_Desc".Translate());
			if (ModsConfig.OdysseyActive && (pawnThing?.health.hediffSet.HasHediff(HediffDefOf.SentienceCatalyst) ?? false))
			{
				sb.AppendLine();
				sb.AppendLine("StatsReport_SentienceCatalystInstalled".Translate());
			}
			yield return new StatDrawEntry(StatCategoryDefOf.Animals, "Trainability".Translate(), trainabilityDef.LabelCap, sb.ToString(), 2500);
			sb.Clear();
			sb.AppendLine("StatsReport_AvailableTraining_Desc".Translate());
			bool visible;
			IOrderedEnumerable<TrainableDef> orderedEnumerable = from td in DefDatabase<TrainableDef>.AllDefsListForReading
				where Pawn_TrainingTracker.CanAssignToTrain(td, parentDef, out visible, pawnThing)
				orderby td.listPriority descending
				select td;
			foreach (TrainableDef item in orderedEnumerable)
			{
				sb.AppendLine();
				sb.AppendLine(item.LabelCap.Colorize(ColorLibrary.Yellow).ResolveTags());
				sb.AppendLine(item.description);
			}
			if (sb.Length != 0)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Animals, "StatsReport_AvailableTraining".Translate(), orderedEnumerable.Select((TrainableDef td) => td.label).ToCommaList().CapitalizeFirst(), sb.ToString(), 2499);
			}
		}
		if (!parentDef.race.IsDrone)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.PawnHealth, "StatsReport_LifeExpectancy".Translate(), lifeExpectancy.ToStringByStyle(ToStringStyle.Integer), "Stat_Race_LifeExpectancy_Desc".Translate(), 4300);
		}
		if (parentDef.race.Animal || (pawnThing?.FenceBlocked ?? FenceBlocked))
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Animals, "StatsReport_BlockedByFences".Translate(), (pawnThing?.FenceBlocked ?? FenceBlocked) ? "Yes".Translate() : "No".Translate(), "Stat_Race_BlockedByFences_Desc".Translate(), 2040);
		}
		if (parentDef.race.Animal)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Animals, "PackAnimal".Translate(), packAnimal ? "Yes".Translate() : "No".Translate(), "PackAnimalExplanation".Translate(), 2202);
			if (pawnThing != null && pawnThing.gender != Gender.None)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "Sex".Translate(), pawnThing.gender.GetLabel(animal: true).CapitalizeFirst(), pawnThing.gender.GetLabel(animal: true).CapitalizeFirst(), 4204);
			}
			if (parentDef.race.nuzzleMtbHours > 0f)
			{
				float num = ((pawnThing != null) ? NuzzleUtility.GetNuzzleMTBHours(pawnThing) : parentDef.race.nuzzleMtbHours);
				yield return new StatDrawEntry(StatCategoryDefOf.Animals, "NuzzleInterval".Translate(), Mathf.RoundToInt(num * 2500f).ToStringTicksToPeriod(), "NuzzleIntervalExplanation".Translate(), 500);
			}
			if (parentDef.race.roamMtbDays.HasValue && (pawnThing?.Roamer ?? true))
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Animals, "StatsReport_RoamInterval".Translate(), Mathf.RoundToInt((pawnThing?.RoamMtbDays ?? parentDef.race.roamMtbDays).Value * 60000f).ToStringTicksToPeriod(), "Stat_Race_RoamInterval_Desc".Translate(), 2030);
			}
			foreach (StatDrawEntry item2 in AnimalProductionUtility.AnimalProductionStats(parentDef))
			{
				yield return item2;
			}
		}
		if (!ModsConfig.BiotechActive || !IsMechanoid)
		{
			yield break;
		}
		if (mechWeightClass != null)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Mechanoid, "MechWeightClass".Translate(), mechWeightClass.LabelCap, "MechWeightClassExplanation".Translate(), 500);
		}
		ThingDef thingDef = MechanitorUtility.RechargerForMech(parentDef);
		if (thingDef != null)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Mechanoid, "StatsReport_RechargerNeeded".Translate(), thingDef.LabelCap, "StatsReport_RechargerNeeded_Desc".Translate(), 503, null, new Dialog_InfoCard.Hyperlink[1]
			{
				new Dialog_InfoCard.Hyperlink(thingDef)
			});
		}
		foreach (StatDrawEntry item3 in MechWorkUtility.SpecialDisplayStats(parentDef, req))
		{
			yield return item3;
		}
	}

	public static string NutritionEatenPerDay(Pawn p)
	{
		return (p.needs.food.FoodFallPerTickAssumingCategory(HungerCategory.Fed) * 60000f).ToString("0.##");
	}

	public static string NutritionEatenPerDayExplanation(Pawn p, bool showDiet = false, bool showLegend = false, bool showCalculations = true)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("NutritionEatenPerDayTip".Translate(ThingDefOf.MealSimple.GetStatValueAbstract(StatDefOf.Nutrition).ToString("0.##")));
		stringBuilder.AppendLine();
		if (showDiet)
		{
			stringBuilder.AppendLine("CanEat".Translate() + ": " + p.RaceProps.foodType.ToHumanString());
			stringBuilder.AppendLine();
		}
		if (showLegend)
		{
			stringBuilder.AppendLine("Legend".Translate() + ":");
			stringBuilder.AppendLine("NoDietCategoryLetter".Translate() + " - " + DietCategory.Omnivorous.ToStringHuman());
			DietCategory[] array = (DietCategory[])Enum.GetValues(typeof(DietCategory));
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] != DietCategory.NeverEats && array[i] != DietCategory.Omnivorous)
				{
					stringBuilder.AppendLine(array[i].ToStringHumanShort() + " - " + array[i].ToStringHuman());
				}
			}
			stringBuilder.AppendLine();
		}
		if (showCalculations)
		{
			stringBuilder.AppendLine("StatsReport_BaseValue".Translate() + ": " + (p.ageTracker.CurLifeStage.hungerRateFactor * p.RaceProps.baseHungerRate * 2.6666667E-05f * 60000f).ToStringByStyle(ToStringStyle.FloatTwo));
			if (p.health.hediffSet.HungerRateFactor != 1f)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("StatsReport_RelevantHediffs".Translate() + ": " + p.health.hediffSet.HungerRateFactor.ToStringByStyle(ToStringStyle.PercentOne, ToStringNumberSense.Factor));
				foreach (Hediff hediff in p.health.hediffSet.hediffs)
				{
					if (hediff.CurStage != null && hediff.CurStage.hungerRateFactor != 1f)
					{
						stringBuilder.AppendLine("    " + hediff.LabelCap + ": " + hediff.CurStage.hungerRateFactor.ToStringByStyle(ToStringStyle.PercentOne, ToStringNumberSense.Factor));
					}
				}
				foreach (Hediff hediff2 in p.health.hediffSet.hediffs)
				{
					if (hediff2.CurStage != null && hediff2.CurStage.hungerRateFactorOffset != 0f)
					{
						stringBuilder.AppendLine("    " + hediff2.LabelCap + ": " + hediff2.CurStage.hungerRateFactorOffset.ToStringByStyle(ToStringStyle.FloatMaxOne, ToStringNumberSense.Offset));
					}
				}
			}
			if (p.story?.traits != null && p.story.traits.HungerRateFactor != 1f)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("StatsReport_RelevantTraits".Translate() + ": " + p.story.traits.HungerRateFactor.ToStringByStyle(ToStringStyle.PercentOne, ToStringNumberSense.Factor));
				foreach (Trait allTrait in p.story.traits.allTraits)
				{
					if (!allTrait.Suppressed && allTrait.CurrentData.hungerRateFactor != 1f)
					{
						stringBuilder.AppendLine("    " + allTrait.LabelCap + ": " + allTrait.CurrentData.hungerRateFactor.ToStringByStyle(ToStringStyle.PercentOne, ToStringNumberSense.Factor));
					}
				}
			}
			Building_Bed building_Bed = p.CurrentBed() ?? p.CurrentCaravanBed();
			if (building_Bed != null)
			{
				float statValue = building_Bed.GetStatValue(StatDefOf.BedHungerRateFactor);
				if (statValue != 1f)
				{
					stringBuilder.AppendLine().AppendLine("StatsReport_InBed".Translate() + ": x" + statValue.ToStringPercent());
				}
			}
			if (ModsConfig.BiotechActive)
			{
				Hediff firstHediffOfDef = p.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Lactating);
				if (firstHediffOfDef != null)
				{
					HediffComp_Lactating hediffComp_Lactating = firstHediffOfDef.TryGetComp<HediffComp_Lactating>();
					if (hediffComp_Lactating != null)
					{
						float f = hediffComp_Lactating.AddedNutritionPerDay();
						stringBuilder.AppendLine();
						stringBuilder.AppendLine(firstHediffOfDef.LabelBaseCap + ": " + f.ToStringWithSign());
					}
				}
			}
			if (p.genes != null)
			{
				int num = 0;
				foreach (Gene item in p.genes.GenesListForReading)
				{
					if (!item.Overridden)
					{
						num += item.def.biostatMet;
					}
				}
				float num2 = GeneTuning.MetabolismToFoodConsumptionFactorCurve.Evaluate(num);
				if (num2 != 1f)
				{
					stringBuilder.AppendLine().AppendLine("FactorForMetabolism".Translate() + ": x" + num2.ToStringPercent());
				}
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("StatsReport_FinalValue".Translate() + ": " + NutritionEatenPerDay(p));
		}
		return stringBuilder.ToString().TrimEndNewlines();
	}
}
