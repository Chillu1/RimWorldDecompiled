using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public static class FoodUtility
{
	public struct ThoughtFromIngesting
	{
		public ThoughtDef thought;

		public Precept fromPrecept;
	}

	public const int FoodPoisoningStageInitial = 2;

	public const int FoodPoisoningStageMajor = 1;

	public const int FoodPoisoningStageRecovering = 0;

	public const Danger DefaultFoodSearchDanger = Danger.Some;

	private static HashSet<Thing> filtered = new HashSet<Thing>();

	private static readonly SimpleCurve FoodOptimalityEffectFromMoodCurve = new SimpleCurve
	{
		new CurvePoint(-100f, -600f),
		new CurvePoint(-10f, -100f),
		new CurvePoint(-5f, -70f),
		new CurvePoint(-1f, -50f),
		new CurvePoint(0f, 0f),
		new CurvePoint(100f, 800f)
	};

	private static List<Pawn> tmpPredatorCandidates = new List<Pawn>();

	private static List<ThoughtFromIngesting> ingestThoughts = new List<ThoughtFromIngesting>();

	private static List<ThoughtDef> extraIngestThoughtsFromTraits = new List<ThoughtDef>();

	private static Dictionary<Ideo, Dictionary<HistoryEventDef, List<Precept>>> ideoIngestThoughtsCache = new Dictionary<Ideo, Dictionary<HistoryEventDef, List<Precept>>>();

	public static bool WillEat(this Pawn p, Thing food, Pawn getter = null, bool careIfNotAcceptableForTitle = true, bool allowVenerated = false)
	{
		if (!p.WillEat(food.def, getter, careIfNotAcceptableForTitle, allowVenerated))
		{
			return false;
		}
		if (!allowVenerated && IsVeneratedAnimalMeatOrCorpseOrHasIngredients(food, p))
		{
			return false;
		}
		if (p.foodRestriction != null && !p.IsSubhuman && !p.DevelopmentalStage.Baby())
		{
			FoodPolicy currentRespectedRestriction = p.foodRestriction.GetCurrentRespectedRestriction(getter);
			if (currentRespectedRestriction != null && !currentRespectedRestriction.Allows(food))
			{
				return false;
			}
		}
		return true;
	}

	public static bool WillEat(this Pawn p, ThingDef food, Pawn getter = null, bool careIfNotAcceptableForTitle = true, bool allowVenerated = false)
	{
		if (!p.FoodIsSuitable(food))
		{
			return false;
		}
		if (p.foodRestriction != null)
		{
			if (p.DevelopmentalStage.Baby())
			{
				if (!p.foodRestriction.BabyFoodAllowed(food))
				{
					return false;
				}
			}
			else
			{
				FoodPolicy currentRespectedRestriction = p.foodRestriction.GetCurrentRespectedRestriction(getter);
				if (currentRespectedRestriction != null && !currentRespectedRestriction.Allows(food) && Dialog_ManageFoodPolicies.FoodGlobalFilter.Allows(food))
				{
					return false;
				}
			}
		}
		if (!allowVenerated && IsVeneratedAnimalMeatOrCorpse(food, p))
		{
			return false;
		}
		if (careIfNotAcceptableForTitle && InappropriateForTitle(food, p, allowIfStarving: true))
		{
			return false;
		}
		return true;
	}

	public static bool FoodIsSuitable(this Pawn p, ThingDef food)
	{
		if (p.needs.food == null)
		{
			return false;
		}
		if (p.IsMutant && p.mutant.Def.overrideFoodType)
		{
			return (p.mutant.Def.foodType & food.ingestible.foodType) != 0;
		}
		if (p.DevelopmentalStage.Baby() && !food.ingestible.babiesCanIngest)
		{
			return false;
		}
		if (!p.RaceProps.CanEverEat(food))
		{
			return false;
		}
		return true;
	}

	public static bool DrugIsSuitable(this Pawn p, ThingDef drug)
	{
		if (p.IsMutant && !MutantUtility.CanUseDrug(p, drug))
		{
			return false;
		}
		return true;
	}

	public static bool HasVeneratedAnimalMeatOrCorpseIngredients(Thing food, Pawn ingester)
	{
		CompIngredients compIngredients = food.TryGetComp<CompIngredients>();
		for (int i = 0; i < compIngredients?.ingredients.Count; i++)
		{
			if (IsVeneratedAnimalMeatOrCorpse(compIngredients.ingredients[i], ingester))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsVeneratedAnimalMeatOrCorpseOrHasIngredients(Thing food, Pawn ingester)
	{
		if (IsVeneratedAnimalMeatOrCorpse(food.def, ingester, food))
		{
			return true;
		}
		if (HasVeneratedAnimalMeatOrCorpseIngredients(food, ingester))
		{
			return true;
		}
		return false;
	}

	public static bool InappropriateForTitle(ThingDef food, Pawn p, bool allowIfStarving)
	{
		if (p.royalty == null)
		{
			return false;
		}
		if (p.needs?.food == null)
		{
			return false;
		}
		if ((allowIfStarving && p.needs.food.Starving) || (p.story != null && p.story.traits.HasTrait(TraitDefOf.Ascetic)) || p.IsPrisoner || (food.ingestible.joyKind != null && food.ingestible.joy > 0f))
		{
			return false;
		}
		RoyalTitle royalTitle = p.royalty?.MostSeniorTitle;
		if (royalTitle != null && royalTitle.conceited && royalTitle.def.foodRequirement.Defined)
		{
			return !royalTitle.def.foodRequirement.Acceptable(food);
		}
		return false;
	}

	public static bool TryFindBestFoodSourceFor(Pawn getter, Pawn eater, bool desperate, out Thing foodSource, out ThingDef foodDef, bool canRefillDispenser = true, bool canUseInventory = true, bool canUsePackAnimalInventory = false, bool allowForbidden = false, bool allowCorpse = true, bool allowSociallyImproper = false, bool allowHarvest = false, bool forceScanWholeMap = false, bool ignoreReservations = false, bool calculateWantedStackCount = false, bool allowVenerated = false, FoodPreferability minPrefOverride = FoodPreferability.Undefined)
	{
		using (new ProfilerBlock("TryFindBestFoodSourceFor"))
		{
			if (allowVenerated && TryFindBestFoodSourceFor(getter, eater, desperate, out foodSource, out foodDef, canRefillDispenser, canUseInventory, canUsePackAnimalInventory, allowForbidden, allowCorpse, allowSociallyImproper, allowHarvest, forceScanWholeMap, ignoreReservations, calculateWantedStackCount, allowVenerated: false, minPrefOverride))
			{
				return true;
			}
			bool canManipulateTools = getter.RaceProps.ToolUser && getter.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation);
			bool allowDrug = !eater.IsTeetotaler();
			Thing thing = null;
			Pawn packAnimal = null;
			if (canUseInventory)
			{
				if (canManipulateTools)
				{
					thing = BestFoodInInventory(getter, eater, (minPrefOverride == FoodPreferability.Undefined) ? FoodPreferability.MealAwful : minPrefOverride, FoodPreferability.MealLavish, 0f, allowDrug: false, allowVenerated);
				}
				if (thing != null)
				{
					if (getter.Faction != Faction.OfPlayer)
					{
						foodSource = thing;
						foodDef = GetFinalIngestibleDef(foodSource);
						return true;
					}
					CompRottable compRottable = thing.TryGetComp<CompRottable>();
					if (compRottable != null && compRottable.Stage == RotStage.Fresh && compRottable.TicksUntilRotAtCurrentTemp < 30000)
					{
						foodSource = thing;
						foodDef = GetFinalIngestibleDef(foodSource);
						return true;
					}
				}
			}
			Pawn getter2 = getter;
			Pawn eater2 = eater;
			bool allowPlant = getter == eater;
			bool allowForbidden2 = allowForbidden;
			bool allowVenerated2 = allowVenerated;
			ThingDef foodDef2;
			Thing thing2 = BestFoodSourceOnMap(getter2, eater2, desperate, out foodDef2, FoodPreferability.MealLavish, allowPlant, allowDrug, allowCorpse, allowDispenserFull: true, canRefillDispenser, allowForbidden2, allowSociallyImproper, allowHarvest, forceScanWholeMap, ignoreReservations, calculateWantedStackCount, minPrefOverride, null, allowVenerated2);
			if (thing2 == null && thing == null)
			{
				thing = FirstFoodInClosestPackAnimalInventory((minPrefOverride == FoodPreferability.Undefined) ? FoodPreferability.MealAwful : minPrefOverride);
			}
			if (thing != null || thing2 != null)
			{
				if (thing == null && thing2 != null)
				{
					foodSource = thing2;
					foodDef = foodDef2;
					return true;
				}
				ThingDef finalIngestibleDef = GetFinalIngestibleDef(thing);
				if (thing2 == null)
				{
					foodSource = thing;
					foodDef = finalIngestibleDef;
					return true;
				}
				float num = FoodOptimality(eater, thing2, foodDef2, (getter.Position - thing2.Position).LengthManhattan);
				float num2 = FoodOptimality(eater, thing, finalIngestibleDef, (packAnimal != null) ? (getter.Position - packAnimal.Position).LengthManhattan : 0);
				num2 -= 32f;
				if (num > num2)
				{
					foodSource = thing2;
					foodDef = foodDef2;
					return true;
				}
				foodSource = thing;
				foodDef = GetFinalIngestibleDef(foodSource);
				return true;
			}
			if (canUseInventory && canManipulateTools)
			{
				thing = BestFoodInInventory(getter, eater, FoodPreferability.DesperateOnly, FoodPreferability.MealLavish, 0f, allowDrug);
				if (thing == null)
				{
					thing = FirstFoodInClosestPackAnimalInventory(FoodPreferability.DesperateOnly);
				}
				if (thing != null)
				{
					foodSource = thing;
					foodDef = GetFinalIngestibleDef(foodSource);
					return true;
				}
			}
			if (thing2 == null && getter == eater && getter.IsPotentiallyPredator())
			{
				Pawn pawn = BestPawnToHuntForPredator(getter, forceScanWholeMap);
				if (pawn != null)
				{
					foodSource = pawn;
					foodDef = GetFinalIngestibleDef(foodSource);
					return true;
				}
			}
			foodSource = null;
			foodDef = null;
			return false;
			Thing FirstFoodInClosestPackAnimalInventory(FoodPreferability foodPref)
			{
				Thing result = null;
				if (canUseInventory && canUsePackAnimalInventory && canManipulateTools && eater.IsColonist && getter.IsColonist && getter.Map != null)
				{
					foreach (Pawn spawnedColonyAnimal in getter.Map.mapPawns.SpawnedColonyAnimals)
					{
						Thing thing3 = BestFoodInInventory(spawnedColonyAnimal, eater, foodPref, FoodPreferability.MealLavish, 0f, allowDrug: false, allowVenerated);
						if (thing3 != null && (packAnimal == null || (getter.Position - packAnimal.Position).LengthManhattan > (getter.Position - spawnedColonyAnimal.Position).LengthManhattan) && !spawnedColonyAnimal.IsForbidden(getter) && getter.CanReach(spawnedColonyAnimal, PathEndMode.OnCell, Danger.Some))
						{
							packAnimal = spawnedColonyAnimal;
							result = thing3;
						}
					}
				}
				return result;
			}
		}
	}

	public static ThingDef GetFinalIngestibleDef(Thing foodSource, bool harvest = false)
	{
		if (foodSource is Building_NutrientPasteDispenser building_NutrientPasteDispenser)
		{
			return building_NutrientPasteDispenser.DispensableDef;
		}
		if (foodSource is Pawn pawn)
		{
			return pawn.RaceProps.corpseDef;
		}
		if (harvest && foodSource is Plant { HarvestableNow: not false } plant && plant.def.plant.harvestedThingDef.IsIngestible)
		{
			return plant.def.plant.harvestedThingDef;
		}
		return foodSource.def;
	}

	public static Thing BestFoodInInventory(Pawn holder, Pawn eater = null, FoodPreferability minFoodPref = FoodPreferability.NeverForNutrition, FoodPreferability maxFoodPref = FoodPreferability.MealLavish, float minStackNutrition = 0f, bool allowDrug = false, bool allowVenerated = false)
	{
		if (holder.inventory == null)
		{
			return null;
		}
		if (eater == null)
		{
			eater = holder;
		}
		ThingOwner<Thing> innerContainer = holder.inventory.innerContainer;
		for (int i = 0; i < innerContainer.Count; i++)
		{
			Thing thing = innerContainer[i];
			if (thing.def.IsNutritionGivingIngestible && thing.IngestibleNow && eater.WillEat(thing, holder, careIfNotAcceptableForTitle: true, allowVenerated) && (int)thing.def.ingestible.preferability >= (int)minFoodPref && (int)thing.def.ingestible.preferability <= (int)maxFoodPref && (allowDrug || !thing.def.IsDrug) && NutritionForEater(eater, thing) * (float)thing.stackCount >= minStackNutrition)
			{
				return thing;
			}
		}
		return null;
	}

	public static int GetMaxAmountToPickup(Thing food, Pawn pawn, int wantedCount)
	{
		if (food == null)
		{
			return 0;
		}
		if (food is Building_NutrientPasteDispenser)
		{
			if (!pawn.CanReserve(food))
			{
				return 0;
			}
			return -1;
		}
		if (food is Corpse)
		{
			if (!pawn.CanReserve(food))
			{
				return 0;
			}
			return 1;
		}
		if (wantedCount < 0)
		{
			wantedCount = 1;
		}
		int num = Math.Min(wantedCount, food.stackCount);
		if (food.Spawned && food.Map != null)
		{
			return Math.Min(num, food.Map.reservationManager.CanReserveStack(pawn, food, 10));
		}
		return num;
	}

	public static Thing BestFoodSourceOnMap(Pawn getter, Pawn eater, bool desperate, out ThingDef foodDef, FoodPreferability maxPref = FoodPreferability.MealLavish, bool allowPlant = true, bool allowDrug = true, bool allowCorpse = true, bool allowDispenserFull = true, bool allowDispenserEmpty = true, bool allowForbidden = false, bool allowSociallyImproper = false, bool allowHarvest = false, bool forceScanWholeMap = false, bool ignoreReservations = false, bool calculateWantedStackCount = false, FoodPreferability minPrefOverride = FoodPreferability.Undefined, float? minNutrition = null, bool allowVenerated = false)
	{
		foodDef = null;
		bool getterCanManipulate = getter.RaceProps.ToolUser && getter.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation);
		if (!getterCanManipulate && getter != eater)
		{
			Log.Error(getter?.ToString() + " tried to find food to bring to " + eater?.ToString() + " but " + getter?.ToString() + " is incapable of Manipulation.");
			return null;
		}
		FoodPreferability minPref;
		if (minPrefOverride == FoodPreferability.Undefined)
		{
			if (eater.NonHumanlikeOrWildMan())
			{
				minPref = FoodPreferability.NeverForNutrition;
			}
			else if (desperate)
			{
				minPref = FoodPreferability.DesperateOnly;
			}
			else
			{
				minPref = (((int)eater.needs.food.CurCategory >= 2) ? FoodPreferability.RawBad : FoodPreferability.MealAwful);
				if (minPref == FoodPreferability.MealAwful && eater.genes != null && eater.genes.DontMindRawFood)
				{
					minPref = FoodPreferability.RawBad;
				}
			}
		}
		else
		{
			minPref = minPrefOverride;
		}
		Predicate<Thing> foodValidator = delegate(Thing t)
		{
			IntVec3 intVec;
			if (t is Building_NutrientPasteDispenser building_NutrientPasteDispenser)
			{
				if (!allowDispenserFull || !getterCanManipulate || (int)ThingDefOf.MealNutrientPaste.ingestible.preferability < (int)minPref || (int)ThingDefOf.MealNutrientPaste.ingestible.preferability > (int)maxPref || !eater.WillEat(ThingDefOf.MealNutrientPaste, getter, careIfNotAcceptableForTitle: true, allowVenerated) || (t.Faction != getter.Faction && t.Faction != getter.HostFaction) || (!allowForbidden && t.IsForbidden(getter)) || !building_NutrientPasteDispenser.powerComp.PowerOn || (!allowDispenserEmpty && !building_NutrientPasteDispenser.HasEnoughFeedstockInHoppers()) || !t.InteractionCell.Standable(t.Map) || !IsFoodSourceOnMapSociallyProper(t, getter, eater, allowSociallyImproper) || !getter.Map.reachability.CanReachNonLocal(getter.Position, new TargetInfo(t.InteractionCell, t.Map), PathEndMode.OnCell, TraverseParms.For(getter, Danger.Some)))
				{
					return false;
				}
				intVec = building_NutrientPasteDispenser.InteractionCell;
			}
			else
			{
				if ((int)t.def.ingestible.preferability < (int)minPref || (int)t.def.ingestible.preferability > (int)maxPref || !eater.WillEat(t, getter, careIfNotAcceptableForTitle: true, allowVenerated) || !t.def.IsNutritionGivingIngestible || !t.IngestibleNow || (!allowCorpse && t is Corpse) || (!allowDrug && t.def.IsDrug) || (!allowForbidden && t.IsForbidden(getter)) || (!desperate && t.IsNotFresh()) || t.IsDessicated() || !IsFoodSourceOnMapSociallyProper(t, getter, eater, allowSociallyImproper) || (!getter.AnimalAwareOf(t) && !forceScanWholeMap))
				{
					return false;
				}
				int stackCount = 1;
				float singleFoodNutrition = NutritionForEater(eater, t);
				if (minNutrition.HasValue)
				{
					stackCount = StackCountForNutrition(minNutrition.Value, singleFoodNutrition);
				}
				else if (calculateWantedStackCount)
				{
					stackCount = WillIngestStackCountOf(eater, t.def, singleFoodNutrition);
				}
				if (!ignoreReservations && !getter.CanReserve(t, 10, stackCount))
				{
					return false;
				}
				intVec = t.PositionHeld;
			}
			return (!getter.roping.IsRoped || intVec.InHorDistOf(getter.roping.RopedTo.Cell, 8f)) ? true : false;
		};
		ThingRequest thingRequest = ((!((eater.RaceProps.foodType & (FoodTypeFlags.Plant | FoodTypeFlags.Tree)) != 0 && allowPlant)) ? ThingRequest.ForGroup(ThingRequestGroup.FoodSourceNotPlantOrTree) : ThingRequest.ForGroup(ThingRequestGroup.FoodSource));
		Thing bestThing;
		if (getter.RaceProps.Humanlike || getter.IsColonyMechPlayerControlled)
		{
			bestThing = SpawnedFoodSearchInnerScan(eater, getter.Position, getter.Map.listerThings.ThingsMatching(thingRequest), PathEndMode.ClosestTouch, TraverseParms.For(getter), 9999f, foodValidator);
			if (allowHarvest && getterCanManipulate)
			{
				Thing thing = GenClosest.ClosestThingReachable(searchRegionsMax: (!forceScanWholeMap || bestThing != null) ? 30 : (-1), root: getter.Position, map: getter.Map, thingReq: ThingRequest.ForGroup(ThingRequestGroup.HarvestablePlant), peMode: PathEndMode.Touch, traverseParams: TraverseParms.For(getter), maxDistance: 9999f, validator: delegate(Thing x)
				{
					Plant plant = (Plant)x;
					if (!plant.HarvestableNow)
					{
						return false;
					}
					ThingDef harvestedThingDef = plant.def.plant.harvestedThingDef;
					if (!harvestedThingDef.IsNutritionGivingIngestible)
					{
						return false;
					}
					if (!eater.WillEat(harvestedThingDef, getter, careIfNotAcceptableForTitle: true, allowVenerated))
					{
						return false;
					}
					if (!getter.CanReserve(plant))
					{
						return false;
					}
					if (!allowForbidden && plant.IsForbidden(getter))
					{
						return false;
					}
					return (bestThing == null || (int)GetFinalIngestibleDef(bestThing).ingestible.preferability < (int)harvestedThingDef.ingestible.preferability) ? true : false;
				});
				if (thing != null)
				{
					bestThing = thing;
					foodDef = GetFinalIngestibleDef(thing, harvest: true);
				}
			}
			if (foodDef == null && bestThing != null)
			{
				foodDef = GetFinalIngestibleDef(bestThing);
			}
		}
		else
		{
			int maxRegionsToScan = GetMaxRegionsToScan(getter, forceScanWholeMap);
			filtered.Clear();
			foreach (Thing item in GenRadial.RadialDistinctThingsAround(getter.Position, getter.Map, 2f, useCenter: true))
			{
				if (item is Pawn pawn && pawn != getter && pawn.IsAnimal && pawn.CurJob != null && pawn.CurJob.def == JobDefOf.Ingest && pawn.CurJob.GetTarget(TargetIndex.A).HasThing)
				{
					filtered.Add(pawn.CurJob.GetTarget(TargetIndex.A).Thing);
				}
			}
			bool ignoreEntirelyForbiddenRegions = !allowForbidden && ForbidUtility.CaresAboutForbidden(getter, cellTarget: true) && getter.playerSettings?.EffectiveAreaRestrictionInPawnCurrentMap != null;
			Predicate<Thing> validator = delegate(Thing t)
			{
				if (!foodValidator(t))
				{
					return false;
				}
				if (filtered.Contains(t))
				{
					return false;
				}
				if (!(t is Building_NutrientPasteDispenser) && (int)t.def.ingestible.preferability <= 2)
				{
					return false;
				}
				return !t.IsNotFresh();
			};
			bestThing = GenClosest.ClosestThingReachable(getter.Position, getter.Map, thingRequest, PathEndMode.OnCell, TraverseParms.For(getter), 9999f, validator, null, 0, maxRegionsToScan, forceAllowGlobalSearch: false, RegionType.Set_Passable, ignoreEntirelyForbiddenRegions);
			filtered.Clear();
			if (bestThing == null)
			{
				desperate = true;
				bestThing = GenClosest.ClosestThingReachable(getter.Position, getter.Map, thingRequest, PathEndMode.OnCell, TraverseParms.For(getter), 9999f, foodValidator, null, 0, maxRegionsToScan, forceAllowGlobalSearch: false, RegionType.Set_Passable, ignoreEntirelyForbiddenRegions);
			}
			if (bestThing != null)
			{
				foodDef = GetFinalIngestibleDef(bestThing);
			}
		}
		return bestThing;
	}

	private static int GetMaxRegionsToScan(Pawn getter, bool forceScanWholeMap)
	{
		if (getter.RaceProps.Humanlike)
		{
			return -1;
		}
		if (forceScanWholeMap)
		{
			return -1;
		}
		if (getter.Faction == Faction.OfPlayer)
		{
			if (getter.Roamer && AnimalPenUtility.GetFixedAnimalFilter().Allows(getter))
			{
				CompAnimalPenMarker currentPenOf = AnimalPenUtility.GetCurrentPenOf(getter, allowUnenclosedPens: false);
				if (currentPenOf != null)
				{
					return Mathf.Min(currentPenOf.PenState.ConnectedRegions.Count, 100);
				}
			}
			return 100;
		}
		return 30;
	}

	private static bool IsFoodSourceOnMapSociallyProper(Thing t, Pawn getter, Pawn eater, bool allowSociallyImproper)
	{
		if (!allowSociallyImproper)
		{
			bool animalsCare = !getter.IsAnimal;
			if (!t.IsSociallyProper(getter) && !t.IsSociallyProper(eater, eater.IsPrisonerOfColony, animalsCare))
			{
				return false;
			}
		}
		return true;
	}

	public static float FoodOptimality(Pawn eater, Thing foodSource, ThingDef foodDef, float dist, bool takingToInventory = false)
	{
		float num = 300f;
		num -= dist;
		switch (foodDef.ingestible.preferability)
		{
		case FoodPreferability.NeverForNutrition:
			return -9999999f;
		case FoodPreferability.DesperateOnly:
			num -= 150f;
			break;
		case FoodPreferability.DesperateOnlyForHumanlikes:
			if (eater.RaceProps.Humanlike)
			{
				num -= 150f;
			}
			break;
		}
		CompRottable compRottable = foodSource.TryGetComp<CompRottable>();
		if (compRottable != null)
		{
			if (compRottable.Stage == RotStage.Dessicated)
			{
				return -9999999f;
			}
			if (!takingToInventory && compRottable.Stage == RotStage.Fresh && compRottable.TicksUntilRotAtCurrentTemp < 30000)
			{
				num += 12f;
			}
		}
		if (eater.needs != null && eater.needs.mood != null)
		{
			List<ThoughtFromIngesting> list = ThoughtsFromIngesting(eater, foodSource, foodDef);
			for (int i = 0; i < list.Count; i++)
			{
				num += FoodOptimalityEffectFromMoodCurve.Evaluate(list[i].thought.stages[0].baseMoodEffect);
			}
		}
		if (foodDef.ingestible != null)
		{
			if (eater.RaceProps.Humanlike)
			{
				num += foodDef.ingestible.optimalityOffsetHumanlikes;
				if (eater.genes != null && foodDef.IsRawHumanFood() && eater.genes.DontMindRawFood)
				{
					num += ThingDefOf.MealSimple.ingestible.optimalityOffsetHumanlikes;
				}
			}
			else if (eater.IsAnimal)
			{
				num += foodDef.ingestible.optimalityOffsetFeedingAnimals;
			}
		}
		if (eater.story != null && eater.story.traits.AnyTraitHasIngestibleOverrides)
		{
			List<Trait> allTraits = eater.story.traits.allTraits;
			for (int j = 0; j < allTraits.Count; j++)
			{
				if (allTraits[j].Suppressed)
				{
					continue;
				}
				List<IngestibleModifiers> ingestibleModifiers = allTraits[j].CurrentData.ingestibleModifiers;
				if (ingestibleModifiers.NullOrEmpty())
				{
					continue;
				}
				for (int k = 0; k < ingestibleModifiers.Count; k++)
				{
					if (ingestibleModifiers[k].ingestible == foodDef)
					{
						num += ingestibleModifiers[k].optimalityOffset;
					}
				}
			}
		}
		return num;
	}

	private static Thing SpawnedFoodSearchInnerScan(Pawn eater, IntVec3 root, List<Thing> searchSet, PathEndMode peMode, TraverseParms traverseParams, float maxDistance = 9999f, Predicate<Thing> validator = null)
	{
		if (searchSet == null)
		{
			return null;
		}
		Pawn pawn = traverseParams.pawn ?? eater;
		int num = 0;
		int num2 = 0;
		Thing result = null;
		float num3 = 0f;
		float num4 = float.MinValue;
		for (int i = 0; i < searchSet.Count; i++)
		{
			Thing thing = searchSet[i];
			num2++;
			float num5 = (root - thing.Position).LengthManhattan;
			if (!(num5 > maxDistance))
			{
				num3 = FoodOptimality(eater, thing, GetFinalIngestibleDef(thing), num5);
				if (!(num3 < num4) && pawn.Map.reachability.CanReach(root, thing, peMode, traverseParams) && thing.Spawned && (validator == null || validator(thing)))
				{
					result = thing;
					num4 = num3;
					num++;
				}
			}
		}
		return result;
	}

	public static void DebugFoodSearchFromMouse_Update()
	{
		IntVec3 root = UI.MouseCell();
		if (Find.Selector.SingleSelectedThing is Pawn pawn && pawn.Map == Find.CurrentMap)
		{
			Thing thing = SpawnedFoodSearchInnerScan(pawn, root, Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.FoodSourceNotPlantOrTree), PathEndMode.ClosestTouch, TraverseParms.For(pawn));
			if (thing != null)
			{
				GenDraw.DrawLineBetween(root.ToVector3Shifted(), thing.Position.ToVector3Shifted());
			}
		}
	}

	public static void DebugFoodSearchFromMouse_OnGUI()
	{
		IntVec3 intVec = UI.MouseCell();
		if (!(Find.Selector.SingleSelectedThing is Pawn pawn) || pawn.Map != Find.CurrentMap)
		{
			return;
		}
		Text.Anchor = TextAnchor.MiddleCenter;
		Text.Font = GameFont.Tiny;
		foreach (Thing item in Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.FoodSourceNotPlantOrTree))
		{
			ThingDef finalIngestibleDef = GetFinalIngestibleDef(item);
			float num = FoodOptimality(pawn, item, finalIngestibleDef, (intVec - item.Position).LengthHorizontal);
			Vector2 vector = item.DrawPos.MapToUIPosition();
			Rect rect = new Rect(vector.x - 100f, vector.y - 100f, 200f, 200f);
			string text = num.ToString("F0");
			List<ThoughtFromIngesting> list = ThoughtsFromIngesting(pawn, item, finalIngestibleDef);
			for (int i = 0; i < list.Count; i++)
			{
				text = text + "\n" + list[i].thought.defName + "(" + FoodOptimalityEffectFromMoodCurve.Evaluate(list[i].thought.stages[0].baseMoodEffect).ToString("F0") + ")";
			}
			Widgets.Label(rect, text);
		}
		Text.Anchor = TextAnchor.UpperLeft;
	}

	private static Pawn BestPawnToHuntForPredator(Pawn predator, bool forceScanWholeMap)
	{
		if (predator.meleeVerbs.TryGetMeleeVerb(null) == null)
		{
			return null;
		}
		bool flag = false;
		if (predator.health.summaryHealth.SummaryHealthPercent < 0.25f)
		{
			flag = true;
		}
		tmpPredatorCandidates.Clear();
		int maxRegionsToScan = GetMaxRegionsToScan(predator, forceScanWholeMap);
		if (maxRegionsToScan < 0)
		{
			tmpPredatorCandidates.AddRange(predator.Map.mapPawns.AllPawnsSpawned);
		}
		else
		{
			TraverseParms traverseParms = TraverseParms.For(predator);
			RegionTraverser.BreadthFirstTraverse(predator.Position, predator.Map, (Region from, Region to) => to.Allows(traverseParms, isDestination: true), delegate(Region x)
			{
				List<Thing> list = x.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn);
				for (int i = 0; i < list.Count; i++)
				{
					tmpPredatorCandidates.Add((Pawn)list[i]);
				}
				return false;
			}, maxRegionsToScan);
		}
		Pawn pawn = null;
		float num = 0f;
		bool tutorialMode = TutorSystem.TutorialMode;
		for (int num2 = 0; num2 < tmpPredatorCandidates.Count; num2++)
		{
			Pawn pawn2 = tmpPredatorCandidates[num2];
			if (predator.GetDistrict() == pawn2.GetDistrict() && predator != pawn2 && (!flag || pawn2.Downed) && IsAcceptablePreyFor(predator, pawn2) && predator.CanReach(pawn2, PathEndMode.ClosestTouch, Danger.Deadly) && !pawn2.IsForbidden(predator) && (!tutorialMode || pawn2.Faction != Faction.OfPlayer))
			{
				float preyScoreFor = GetPreyScoreFor(predator, pawn2);
				if (preyScoreFor > num || pawn == null)
				{
					num = preyScoreFor;
					pawn = pawn2;
				}
			}
		}
		tmpPredatorCandidates.Clear();
		return pawn;
	}

	public static bool IsAcceptablePreyFor(Pawn predator, Pawn prey)
	{
		if (!prey.RaceProps.canBePredatorPrey)
		{
			return false;
		}
		if (!prey.RaceProps.IsFlesh)
		{
			return false;
		}
		if (!Find.Storyteller.difficulty.predatorsHuntHumanlikes && prey.RaceProps.Humanlike)
		{
			return false;
		}
		if (prey.BodySize > predator.RaceProps.maxPreyBodySize)
		{
			return false;
		}
		if (!prey.Downed)
		{
			if (prey.kindDef.combatPower > 2f * predator.kindDef.combatPower)
			{
				return false;
			}
			float num = prey.kindDef.combatPower * prey.health.summaryHealth.SummaryHealthPercent * prey.BodySize;
			float num2 = predator.kindDef.combatPower * predator.health.summaryHealth.SummaryHealthPercent * predator.BodySize;
			if (num >= num2)
			{
				return false;
			}
		}
		if (predator.Faction != null && prey.Faction != null && !predator.HostileTo(prey))
		{
			return false;
		}
		if (predator.Faction != null && prey.HostFaction != null && !predator.HostileTo(prey))
		{
			return false;
		}
		if (predator.Faction == Faction.OfPlayer && prey.Faction == Faction.OfPlayer)
		{
			return false;
		}
		if (predator.RaceProps.herdAnimal && predator.def == prey.def)
		{
			return false;
		}
		if (prey.IsHiddenFromPlayer() || prey.IsPsychologicallyInvisible())
		{
			return false;
		}
		if (ModsConfig.AnomalyActive && prey.IsMutant && !prey.mutant.Def.canBleed)
		{
			return false;
		}
		return true;
	}

	public static float GetPreyScoreFor(Pawn predator, Pawn prey)
	{
		float num = prey.kindDef.combatPower / predator.kindDef.combatPower;
		float num2 = prey.health.summaryHealth.SummaryHealthPercent;
		float bodySizeFactor = prey.ageTracker.CurLifeStage.bodySizeFactor;
		float lengthHorizontal = (predator.Position - prey.Position).LengthHorizontal;
		if (prey.Downed)
		{
			num2 = Mathf.Min(num2, 0.2f);
		}
		float num3 = 0f - lengthHorizontal - 56f * num2 * num2 * num * bodySizeFactor;
		if (prey.RaceProps.Humanlike)
		{
			num3 -= 35f;
		}
		else if (IsPreyProtectedFromPredatorByFence(predator, prey))
		{
			num3 -= 17f;
		}
		return num3;
	}

	private static bool IsPreyProtectedFromPredatorByFence(Pawn predator, Pawn prey)
	{
		if (predator.GetDistrict() == prey.GetDistrict())
		{
			return false;
		}
		TraverseParms traverseParams = TraverseParms.For(predator).WithFenceblocked(forceFenceblocked: true);
		return !predator.Map.reachability.CanReach(predator.Position, prey.Position, PathEndMode.ClosestTouch, traverseParams);
	}

	public static void DebugDrawPredatorFoodSource()
	{
		if (!(Find.Selector.SingleSelectedThing is Pawn pawn) || !TryFindBestFoodSourceFor(pawn, pawn, desperate: true, out var foodSource, out var _, canRefillDispenser: false, canUseInventory: false))
		{
			return;
		}
		GenDraw.DrawLineBetween(pawn.Position.ToVector3Shifted(), foodSource.Position.ToVector3Shifted());
		if (!(foodSource is Pawn))
		{
			Pawn pawn2 = BestPawnToHuntForPredator(pawn, forceScanWholeMap: true);
			if (pawn2 != null)
			{
				GenDraw.DrawLineBetween(pawn.Position.ToVector3Shifted(), pawn2.Position.ToVector3Shifted());
			}
		}
	}

	public static float MoodFromIngesting(Pawn ingester, Thing foodSource, ThingDef foodDef)
	{
		return ThoughtsFromIngesting(ingester, foodSource, foodDef).Sum((ThoughtFromIngesting ingestingThought) => ingestingThought.thought.stages[0].baseMoodEffect);
	}

	public static List<ThoughtFromIngesting> ThoughtsFromIngesting(Pawn ingester, Thing foodSource, ThingDef foodDef)
	{
		ingestThoughts.Clear();
		extraIngestThoughtsFromTraits.Clear();
		if (ingester.needs?.mood == null)
		{
			return ingestThoughts;
		}
		MeatSourceCategory meatSourceCategory = ((!foodSource.def.IsCorpse) ? GetMeatSourceCategory(foodDef) : GetMeatSourceCategoryFromCorpse(foodSource));
		ingester.story?.traits?.GetExtraThoughtsFromIngestion(extraIngestThoughtsFromTraits, foodDef, meatSourceCategory, direct: true);
		if (!ingester.story.traits.HasTrait(TraitDefOf.Ascetic) && foodDef.ingestible.tasteThought != null)
		{
			TryAddIngestThought(ingester, foodDef.ingestible.tasteThought, null, ingestThoughts, foodDef, meatSourceCategory);
		}
		CompIngredients compIngredients = foodSource.TryGetComp<CompIngredients>();
		Building_NutrientPasteDispenser building_NutrientPasteDispenser = foodSource as Building_NutrientPasteDispenser;
		for (int i = 0; i < extraIngestThoughtsFromTraits.Count; i++)
		{
			TryAddIngestThought(ingester, extraIngestThoughtsFromTraits[i], null, ingestThoughts, foodDef, meatSourceCategory);
		}
		if (compIngredients != null)
		{
			bool flag = false;
			bool flag2 = false;
			for (int j = 0; j < compIngredients.ingredients.Count; j++)
			{
				AddIngestThoughtsFromIngredient(compIngredients.ingredients[j], ingester, ingestThoughts, out var ateFungus, out var ateNonFungusRawPlant);
				if (ateFungus)
				{
					flag = true;
				}
				if (ateNonFungusRawPlant)
				{
					flag2 = true;
				}
			}
			if (ModsConfig.IdeologyActive && flag2 && !flag)
			{
				AddThoughtsFromIdeo(HistoryEventDefOf.AteNonFungusMealWithPlants, ingester, foodDef, meatSourceCategory);
			}
		}
		else if (building_NutrientPasteDispenser != null)
		{
			Thing thing = building_NutrientPasteDispenser.FindFeedInAnyHopper();
			if (thing != null)
			{
				AddIngestThoughtsFromIngredient(thing.def, ingester, ingestThoughts, out var _, out var _);
			}
		}
		if (foodDef.ingestible.specialThoughtDirect != null)
		{
			TryAddIngestThought(ingester, foodDef.ingestible.specialThoughtDirect, null, ingestThoughts, foodDef, meatSourceCategory);
		}
		if (foodSource.IsNotFresh())
		{
			TryAddIngestThought(ingester, ThoughtDefOf.AteRottenFood, null, ingestThoughts, foodDef, meatSourceCategory);
		}
		if (ModsConfig.RoyaltyActive && InappropriateForTitle(foodDef, ingester, allowIfStarving: false))
		{
			TryAddIngestThought(ingester, ThoughtDefOf.AteFoodInappropriateForTitle, null, ingestThoughts, foodDef, meatSourceCategory);
		}
		if (ingester.Ideo != null)
		{
			bool flag3 = IsHumanlikeCorpseOrHumanlikeMeat(foodSource, foodDef);
			bool flag4 = IsHumanlikeCorpseOrHumanlikeMeatOrIngredient(foodSource);
			if (flag4)
			{
				AddThoughtsFromIdeo(HistoryEventDefOf.AteHumanMeat, ingester, foodDef, meatSourceCategory);
				if (flag3)
				{
					AddThoughtsFromIdeo(HistoryEventDefOf.AteHumanMeatDirect, ingester, foodDef, meatSourceCategory);
				}
			}
			else if (!AcceptableCannibalNonHumanlikeMeatFood(foodDef))
			{
				AddThoughtsFromIdeo(HistoryEventDefOf.AteNonCannibalFood, ingester, foodDef, meatSourceCategory);
			}
			FoodKind foodKind = GetFoodKind(foodSource);
			FoodKind foodKind2 = GetFoodKind(foodDef);
			if (!AcceptableVegetarian(foodDef, foodKind, foodKind2))
			{
				if (!flag4 && foodKind != FoodKind.NonMeat)
				{
					AddThoughtsFromIdeo(HistoryEventDefOf.AteMeat, ingester, foodDef, meatSourceCategory);
				}
			}
			else if (!AcceptableCarnivore(foodDef, foodKind, foodKind2))
			{
				AddThoughtsFromIdeo(HistoryEventDefOf.AteNonMeat, ingester, foodDef, meatSourceCategory);
			}
			if (IsVeneratedAnimalMeatOrCorpse(foodDef, ingester, foodSource))
			{
				AddThoughtsFromIdeo(HistoryEventDefOf.AteVeneratedAnimalMeat, ingester, foodDef, meatSourceCategory);
			}
			if (meatSourceCategory == MeatSourceCategory.Insect)
			{
				AddThoughtsFromIdeo(HistoryEventDefOf.AteInsectMeatDirect, ingester, foodDef, meatSourceCategory);
			}
			if (ModsConfig.IdeologyActive && foodDef.thingCategories != null && foodDef.thingCategories.Contains(ThingCategoryDefOf.PlantFoodRaw))
			{
				if (foodDef.IsFungus)
				{
					AddThoughtsFromIdeo(HistoryEventDefOf.AteFungus, ingester, foodDef, meatSourceCategory);
				}
				else
				{
					AddThoughtsFromIdeo(HistoryEventDefOf.AteNonFungusPlant, ingester, foodDef, meatSourceCategory);
				}
			}
			if (foodDef.ingestible.ateEvent != null)
			{
				AddThoughtsFromIdeo(foodDef.ingestible.ateEvent, ingester, foodDef, meatSourceCategory);
			}
		}
		return ingestThoughts;
	}

	private static void AddIngestThoughtsFromIngredient(ThingDef ingredient, Pawn ingester, List<ThoughtFromIngesting> ingestThoughts, out bool ateFungus, out bool ateNonFungusRawPlant)
	{
		ateFungus = false;
		ateNonFungusRawPlant = false;
		if (ingredient.ingestible == null)
		{
			return;
		}
		MeatSourceCategory meatSourceCategory = GetMeatSourceCategory(ingredient);
		extraIngestThoughtsFromTraits.Clear();
		ingester.story?.traits?.GetExtraThoughtsFromIngestion(extraIngestThoughtsFromTraits, ingredient, meatSourceCategory, direct: false);
		for (int i = 0; i < extraIngestThoughtsFromTraits.Count; i++)
		{
			TryAddIngestThought(ingester, extraIngestThoughtsFromTraits[i], null, ingestThoughts, ingredient, meatSourceCategory);
		}
		if (ingredient.ingestible.specialThoughtAsIngredient != null)
		{
			TryAddIngestThought(ingester, ingredient.ingestible.specialThoughtAsIngredient, null, ingestThoughts, ingredient, meatSourceCategory);
		}
		if (meatSourceCategory == MeatSourceCategory.Humanlike)
		{
			AddThoughtsFromIdeo(HistoryEventDefOf.AteHumanMeat, ingester, ingredient, meatSourceCategory);
			AddThoughtsFromIdeo(HistoryEventDefOf.AteHumanMeatAsIngredient, ingester, ingredient, meatSourceCategory);
		}
		else if (IsVeneratedAnimalMeatOrCorpse(ingredient, ingester))
		{
			AddThoughtsFromIdeo(HistoryEventDefOf.AteVeneratedAnimalMeat, ingester, ingredient, meatSourceCategory);
		}
		if (meatSourceCategory == MeatSourceCategory.Insect)
		{
			AddThoughtsFromIdeo(HistoryEventDefOf.AteInsectMeatAsIngredient, ingester, ingredient, meatSourceCategory);
		}
		if (ModsConfig.IdeologyActive && ingredient.thingCategories != null && ingredient.thingCategories.Contains(ThingCategoryDefOf.PlantFoodRaw))
		{
			if (ingredient.IsFungus)
			{
				AddThoughtsFromIdeo(HistoryEventDefOf.AteFungusAsIngredient, ingester, ingredient, meatSourceCategory);
				ateFungus = true;
			}
			else
			{
				ateNonFungusRawPlant = true;
			}
		}
	}

	private static void TryAddIngestThought(Pawn ingester, ThoughtDef def, Precept fromPrecept, List<ThoughtFromIngesting> ingestThoughts, ThingDef foodDef, MeatSourceCategory meatSourceCategory)
	{
		if (ThoughtUtility.NullifyingGene(def, ingester) != null || ThoughtUtility.NullifyingHediff(def, ingester) != null || ThoughtUtility.NullifyingPrecept(def, ingester) != null || (fromPrecept != null && !fromPrecept.def.enabledForNPCFactions && !ingester.CountsAsNonNPCForPrecepts()))
		{
			return;
		}
		ThoughtFromIngesting item = new ThoughtFromIngesting
		{
			thought = def,
			fromPrecept = fromPrecept
		};
		if (ingester.story != null && ingester.story.traits != null)
		{
			if (!ingester.story.traits.IsThoughtFromIngestionDisallowed(def, foodDef, meatSourceCategory))
			{
				ingestThoughts.Add(item);
			}
		}
		else
		{
			ingestThoughts.Add(item);
		}
	}

	private static void AddThoughtsFromIdeo(HistoryEventDef eventDef, Pawn ingester, ThingDef foodDef, MeatSourceCategory meatSourceCategory)
	{
		if (ingester.Ideo == null || eventDef == null)
		{
			return;
		}
		if (!ideoIngestThoughtsCache.TryGetValue(ingester.Ideo, out var value))
		{
			value = new Dictionary<HistoryEventDef, List<Precept>>();
			ideoIngestThoughtsCache[ingester.Ideo] = value;
		}
		if (!value.TryGetValue(eventDef, out var value2))
		{
			value2 = new List<Precept>();
			List<Precept> preceptsListForReading = ingester.Ideo.PreceptsListForReading;
			for (int i = 0; i < preceptsListForReading.Count; i++)
			{
				List<PreceptComp> comps = preceptsListForReading[i].def.comps;
				for (int j = 0; j < comps.Count; j++)
				{
					if (comps[j] is PreceptComp_SelfTookMemoryThought preceptComp_SelfTookMemoryThought && preceptComp_SelfTookMemoryThought.eventDef == eventDef)
					{
						value2.Add(preceptsListForReading[i]);
					}
				}
			}
			value[eventDef] = value2;
		}
		for (int k = 0; k < value2.Count; k++)
		{
			Precept precept = value2[k];
			List<PreceptComp> comps2 = precept.def.comps;
			for (int l = 0; l < comps2.Count; l++)
			{
				if (comps2[l] is PreceptComp_SelfTookMemoryThought preceptComp_SelfTookMemoryThought2 && preceptComp_SelfTookMemoryThought2.eventDef == eventDef)
				{
					TryAddIngestThought(ingester, preceptComp_SelfTookMemoryThought2.thought, precept, ingestThoughts, foodDef, meatSourceCategory);
				}
			}
		}
	}

	public static bool IsHumanlikeCorpseOrHumanlikeMeatOrIngredient(Thing food)
	{
		CompIngredients compIngredients = food.TryGetComp<CompIngredients>();
		if (compIngredients == null)
		{
			return IsHumanlikeCorpseOrHumanlikeMeat(food, food.def);
		}
		foreach (ThingDef ingredient in compIngredients.ingredients)
		{
			if (IsHumanlikeCorpseOrHumanlikeMeat(food, ingredient))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsHumanlikeCorpseOrHumanlikeMeat(Thing source, ThingDef foodDef)
	{
		if (source.def.IsCorpse)
		{
			return GetMeatSourceCategoryFromCorpse(source) == MeatSourceCategory.Humanlike;
		}
		return GetMeatSourceCategory(foodDef) == MeatSourceCategory.Humanlike;
	}

	public static bool IsInsectCorpseOrInsectMeatIngredient(Thing food)
	{
		CompIngredients compIngredients = food.TryGetComp<CompIngredients>();
		if (compIngredients != null)
		{
			foreach (ThingDef ingredient in compIngredients.ingredients)
			{
				if (GetMeatSourceCategory(ingredient) == MeatSourceCategory.Insect)
				{
					return true;
				}
			}
			return false;
		}
		if (food.def.IsCorpse)
		{
			return GetMeatSourceCategoryFromCorpse(food) == MeatSourceCategory.Insect;
		}
		return GetMeatSourceCategory(food.def) == MeatSourceCategory.Insect;
	}

	public static bool IsVeneratedAnimalMeatOrCorpse(ThingDef foodDef, Pawn ingester, Thing source = null)
	{
		if (ingester.Ideo == null)
		{
			return false;
		}
		if (source != null && source.def.IsCorpse)
		{
			return ingester.Ideo.IsVeneratedAnimal(((Corpse)source).InnerPawn);
		}
		if (foodDef.IsMeat)
		{
			return ingester.Ideo.IsVeneratedAnimal(foodDef.ingestible.sourceDef);
		}
		return false;
	}

	public static MeatSourceCategory GetMeatSourceCategory(ThingDef source)
	{
		if (source.ingestible == null)
		{
			return MeatSourceCategory.Undefined;
		}
		if ((source.ingestible.foodType & FoodTypeFlags.Meat) != FoodTypeFlags.Meat)
		{
			return MeatSourceCategory.NotMeat;
		}
		if (source.ingestible.sourceDef?.race != null && source.ingestible.sourceDef.race.Humanlike)
		{
			return MeatSourceCategory.Humanlike;
		}
		if (source.ingestible.sourceDef?.race.FleshType != null && source.ingestible.sourceDef.race.FleshType == FleshTypeDefOf.Insectoid)
		{
			return MeatSourceCategory.Insect;
		}
		return MeatSourceCategory.Undefined;
	}

	public static MeatSourceCategory GetMeatSourceCategoryFromCorpse(Thing thing)
	{
		if (!(thing is Corpse corpse))
		{
			return MeatSourceCategory.NotMeat;
		}
		if (corpse.InnerPawn.RaceProps.Humanlike)
		{
			return MeatSourceCategory.Humanlike;
		}
		if (corpse.InnerPawn.RaceProps.Insect)
		{
			return MeatSourceCategory.Insect;
		}
		return MeatSourceCategory.Undefined;
	}

	public static bool HasVegetarianRequiredPrecept(this Ideo ideo)
	{
		if (!ideo.HasPrecept(PreceptDefOf.MeatEating_Disapproved) && !ideo.HasPrecept(PreceptDefOf.MeatEating_Horrible))
		{
			return ideo.HasPrecept(PreceptDefOf.MeatEating_Abhorrent);
		}
		return true;
	}

	public static bool HasMeatEatingRequiredPrecept(this Ideo ideo)
	{
		if (!ideo.HasPrecept(PreceptDefOf.MeatEating_NonMeat_Disapproved) && !ideo.HasPrecept(PreceptDefOf.MeatEating_NonMeat_Horrible))
		{
			return ideo.HasPrecept(PreceptDefOf.MeatEating_NonMeat_Abhorrent);
		}
		return true;
	}

	public static bool HasHumanMeatEatingRequiredPrecept(this Ideo ideo)
	{
		if (!ideo.HasPrecept(PreceptDefOf.Cannibalism_Preferred) && !ideo.HasPrecept(PreceptDefOf.Cannibalism_RequiredRavenous))
		{
			return ideo.HasPrecept(PreceptDefOf.Cannibalism_RequiredStrong);
		}
		return true;
	}

	public static void GenerateGoodIngredients(Thing meal, Ideo ideo)
	{
		CompIngredients compIngredients = meal.TryGetComp<CompIngredients>();
		if (compIngredients == null)
		{
			return;
		}
		compIngredients.ingredients.Clear();
		if (ideo.HasHumanMeatEatingRequiredPrecept())
		{
			compIngredients.ingredients.Add(ThingDefOf.Meat_Human);
		}
		else if (ideo.HasMeatEatingRequiredPrecept())
		{
			compIngredients.ingredients.Add((from d in DefDatabase<ThingDef>.AllDefsListForReading
				where d.race != null && d.race.IsFlesh && d.race.Animal
				select d.race.meatDef).RandomElement());
		}
		else if (ideo.HasVegetarianRequiredPrecept())
		{
			compIngredients.ingredients.Add(DefDatabase<ThingDef>.AllDefsListForReading.Where((ThingDef d) => d.IsNutritionGivingIngestible && d.ingestible.HumanEdible && d.ingestible.foodType.HasFlag(FoodTypeFlags.VegetableOrFruit) && MealCanBeMadeFrom(meal.def, d)).RandomElement());
		}
	}

	public static bool MealCanBeMadeFrom(ThingDef mealDef, ThingDef ingredient)
	{
		foreach (RecipeDef item in DefDatabase<RecipeDef>.AllDefs.Where((RecipeDef x) => x.ProducedThingDef == mealDef))
		{
			if (!item.ingredients.NullOrEmpty() && item.ingredients.Any((IngredientCount x) => x.filter.Allows(ingredient)))
			{
				return true;
			}
		}
		return false;
	}

	public static int WillIngestStackCountOf(Pawn ingester, ThingDef def, float singleFoodNutrition)
	{
		if (ingester.needs?.food == null || def.IsDrug)
		{
			return def.ingestible.defaultNumToIngestAtOnce;
		}
		int num = StackCountForNutrition(ingester.needs.food.NutritionWanted, singleFoodNutrition);
		if (def.ingestible.maxNumToIngestAtOnce > 0)
		{
			num = Mathf.Min(num, def.ingestible.maxNumToIngestAtOnce);
		}
		if (num < 1)
		{
			num = 1;
		}
		return num;
	}

	public static float GetBodyPartNutrition(Corpse corpse, BodyPartRecord part)
	{
		return GetBodyPartNutrition(corpse.GetStatValue(StatDefOf.Nutrition), corpse.InnerPawn, part);
	}

	public static float GetBodyPartNutrition(float currentCorpseNutrition, Pawn pawn, BodyPartRecord part)
	{
		HediffSet hediffSet = pawn.health.hediffSet;
		float coverageOfNotMissingNaturalParts = hediffSet.GetCoverageOfNotMissingNaturalParts(pawn.RaceProps.body.corePart);
		if (coverageOfNotMissingNaturalParts <= 0f)
		{
			return 0f;
		}
		float num = hediffSet.GetCoverageOfNotMissingNaturalParts(part) / coverageOfNotMissingNaturalParts;
		return currentCorpseNutrition * num;
	}

	public static int StackCountForNutrition(float wantedNutrition, float singleFoodNutrition)
	{
		if (wantedNutrition <= 0.0001f)
		{
			return 0;
		}
		return Mathf.Max(Mathf.RoundToInt(wantedNutrition / singleFoodNutrition), 1);
	}

	public static bool ShouldBeFedBySomeone(Pawn pawn)
	{
		if (!FeedPatientUtility.ShouldBeFed(pawn))
		{
			return WardenFeedUtility.ShouldBeFed(pawn);
		}
		return true;
	}

	public static void AddFoodPoisoningHediff(Pawn pawn, Thing ingestible, FoodPoisonCause cause)
	{
		Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.FoodPoisoning);
		if (firstHediffOfDef != null)
		{
			if (firstHediffOfDef.CurStageIndex != 2)
			{
				firstHediffOfDef.Severity = HediffDefOf.FoodPoisoning.stages[2].minSeverity - 0.001f;
			}
		}
		else
		{
			pawn.health.AddHediff(HediffMaker.MakeHediff(HediffDefOf.FoodPoisoning, pawn));
		}
		if (PawnUtility.ShouldSendNotificationAbout(pawn) && MessagesRepeatAvoider.MessageShowAllowed("MessageFoodPoisoning-" + pawn.thingIDNumber, 0.1f))
		{
			Messages.Message("MessageFoodPoisoning".Translate(pawn.LabelShort, ingestible.LabelCapNoCount, cause.ToStringHuman().CapitalizeFirst(), pawn.Named("PAWN"), ingestible.Named("FOOD")).CapitalizeFirst(), pawn, MessageTypeDefOf.NegativeEvent);
		}
	}

	public static float GetFoodPoisonChanceFactor(Pawn ingester)
	{
		if (ModsConfig.AnomalyActive && ingester.IsMutant && ingester.mutant.Def.preventIllnesses)
		{
			return 0f;
		}
		float num = Find.Storyteller.difficulty.foodPoisonChanceFactor;
		if (ingester.health != null && ingester.health.hediffSet != null)
		{
			foreach (Hediff hediff in ingester.health.hediffSet.hediffs)
			{
				HediffStage curStage = hediff.CurStage;
				if (curStage != null)
				{
					num *= curStage.foodPoisoningChanceFactor;
				}
			}
		}
		if (ModsConfig.BiotechActive && ingester.genes != null)
		{
			List<Gene> genesListForReading = ingester.genes.GenesListForReading;
			for (int i = 0; i < genesListForReading.Count; i++)
			{
				num *= genesListForReading[i].def.foodPoisoningChanceFactor;
			}
		}
		return num;
	}

	public static bool TryGetFoodPoisoningChanceOverrideFromTraits(Pawn pawn, Thing ingestible, out float poisonChanceOverride)
	{
		if (ModsConfig.AnomalyActive && pawn.IsMutant && pawn.mutant.Def.preventIllnesses)
		{
			poisonChanceOverride = 0f;
			return false;
		}
		if (pawn.story != null && pawn.story.traits.AnyTraitHasIngestibleOverrides)
		{
			List<Trait> allTraits = pawn.story.traits.allTraits;
			for (int i = 0; i < allTraits.Count; i++)
			{
				if (allTraits[i].Suppressed)
				{
					continue;
				}
				List<IngestibleModifiers> ingestibleModifiers = allTraits[i].CurrentData.ingestibleModifiers;
				if (ingestibleModifiers.NullOrEmpty())
				{
					continue;
				}
				for (int j = 0; j < ingestibleModifiers.Count; j++)
				{
					if (ingestibleModifiers[j].ingestible == ingestible.def)
					{
						poisonChanceOverride = ingestibleModifiers[j].poisonChanceOverride;
						return true;
					}
				}
			}
		}
		poisonChanceOverride = 0f;
		return false;
	}

	public static bool Starving(this Pawn p)
	{
		if (p.needs != null && p.needs.food != null)
		{
			return p.needs.food.Starving;
		}
		return false;
	}

	public static float GetNutrition(Pawn eater, Thing foodSource, ThingDef foodDef)
	{
		if (foodSource == null || foodDef == null)
		{
			return 0f;
		}
		if (foodSource.def == foodDef)
		{
			return NutritionForEater(eater, foodSource);
		}
		return foodDef.GetStatValueAbstract(StatDefOf.Nutrition);
	}

	public static bool WillIngestFromInventoryNow(Pawn pawn, Thing inv)
	{
		if ((inv.def.IsNutritionGivingIngestible && pawn.WillEat(inv)) || pawn.CanTakeDrug(inv.def))
		{
			return inv.IngestibleNow;
		}
		return false;
	}

	public static void IngestFromInventoryNow(Pawn pawn, Thing inv)
	{
		Job job = JobMaker.MakeJob(JobDefOf.Ingest, inv);
		job.count = Mathf.Min(inv.stackCount, WillIngestStackCountOf(pawn, inv.def, NutritionForEater(pawn, inv)));
		pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
	}

	public static FoodKind GetFoodKind(Thing food)
	{
		if (food == null)
		{
			return FoodKind.Any;
		}
		CompIngredients compIngredients = food.TryGetComp<CompIngredients>();
		if (compIngredients != null)
		{
			if (compIngredients.ingredients.NullOrEmpty())
			{
				return compIngredients.Props.noIngredientsFoodKind;
			}
			bool flag = false;
			for (int i = 0; i < compIngredients.ingredients.Count; i++)
			{
				if (GetFoodKind(compIngredients.ingredients[i]) == FoodKind.Meat)
				{
					return FoodKind.Meat;
				}
				if (compIngredients.ingredients[i].IsAnimalProduct)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				return FoodKind.NonMeat;
			}
			return FoodKind.Any;
		}
		return GetFoodKind(food.def);
	}

	public static FoodKind GetFoodKind(ThingDef foodDef)
	{
		if (foodDef == null)
		{
			return FoodKind.Any;
		}
		if (ModsConfig.BiotechActive && foodDef == ThingDefOf.HemogenPack)
		{
			return FoodKind.Any;
		}
		if (!foodDef.IsIngestible)
		{
			return FoodKind.Any;
		}
		if (foodDef.IsMeat)
		{
			return FoodKind.Meat;
		}
		if (foodDef.ingestible != null && (foodDef.ingestible.foodType.HasFlag(FoodTypeFlags.Meat) || foodDef.ingestible.foodType.HasFlag(FoodTypeFlags.Corpse)))
		{
			return FoodKind.Meat;
		}
		if (foodDef.IsAnimalProduct)
		{
			return FoodKind.Any;
		}
		return foodDef.GetCompProperties<CompProperties_Ingredients>()?.noIngredientsFoodKind ?? FoodKind.NonMeat;
	}

	public static bool WillGiveNegativeThoughts(Thing food, Pawn pawn)
	{
		List<ThoughtFromIngesting> list = ThoughtsFromIngesting(pawn, food, GetFinalIngestibleDef(food));
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].thought.stages[0].baseMoodEffect < 0f)
			{
				return true;
			}
		}
		return false;
	}

	public static bool AcceptableVegetarian(Thing food)
	{
		return AcceptableVegetarian(food, food.def);
	}

	private static bool AcceptableVegetarian(Thing source, ThingDef def)
	{
		return AcceptableVegetarian(def, GetFoodKind(source), GetFoodKind(def));
	}

	private static bool AcceptableVegetarian(ThingDef def, FoodKind sourceKind, FoodKind defKind)
	{
		if (!def.IsProcessedFood)
		{
			if (sourceKind != FoodKind.Meat)
			{
				return defKind != FoodKind.Meat;
			}
			return false;
		}
		return true;
	}

	public static bool UnacceptableVegetarian(ThingDef foodDef)
	{
		if (foodDef.IsIngestible && !foodDef.IsProcessedFood)
		{
			return GetFoodKind(foodDef) == FoodKind.Meat;
		}
		return false;
	}

	public static bool AcceptableCannibalNonHumanlikeMeatFood(ThingDef foodDef)
	{
		if (!foodDef.IsDrug && !foodDef.IsProcessedFood && foodDef.ingestible.drugCategory != DrugCategory.Medical && !(foodDef.ingestible.CachedNutrition <= 0f))
		{
			if (ModsConfig.BiotechActive)
			{
				return foodDef == ThingDefOf.HemogenPack;
			}
			return false;
		}
		return true;
	}

	public static bool AcceptableCarnivore(Thing food)
	{
		return AcceptableCarnivore(food, food.def);
	}

	public static bool AcceptableCarnivore(Thing source, ThingDef def)
	{
		return AcceptableCarnivore(def, GetFoodKind(source), GetFoodKind(def));
	}

	private static bool AcceptableCarnivore(ThingDef def, FoodKind sourceKind, FoodKind defKind)
	{
		if (!def.IsProcessedFood && (sourceKind == FoodKind.NonMeat || defKind == FoodKind.NonMeat) && !def.IsDrug)
		{
			return def.ingestible.CachedNutrition <= 0f;
		}
		return true;
	}

	public static bool UnacceptableCarnivore(ThingDef foodDef)
	{
		if (foodDef.IsIngestible && !foodDef.IsProcessedFood && !foodDef.IsDrug)
		{
			return GetFoodKind(foodDef) == FoodKind.NonMeat;
		}
		return false;
	}

	public static bool MaybeAcceptableCannibalDef(ThingDef foodDef)
	{
		if (foodDef.IsIngestible)
		{
			if (GetMeatSourceCategory(foodDef) != MeatSourceCategory.Humanlike && foodDef.ingestible.CachedNutrition != 0f && (!foodDef.IsDrug || foodDef.ingestible.drugCategory != DrugCategory.Medical) && !foodDef.IsProcessedFood && (!foodDef.HasComp(typeof(CompIngredients)) || GetFoodKind(foodDef) == FoodKind.NonMeat))
			{
				if (foodDef.IsCorpse)
				{
					ThingDef sourceDef = foodDef.ingestible.sourceDef;
					if (sourceDef == null)
					{
						return false;
					}
					return sourceDef.race?.Humanlike == true;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public static bool MaybeAcceptableInsectMeatEatersDef(ThingDef foodDef)
	{
		if (foodDef.IsIngestible)
		{
			if (GetMeatSourceCategory(foodDef) != MeatSourceCategory.Insect && foodDef.ingestible.CachedNutrition != 0f && !foodDef.IsDrug && !foodDef.IsProcessedFood && !foodDef.IsAnimalProduct && (!foodDef.HasComp(typeof(CompIngredients)) || GetFoodKind(foodDef) == FoodKind.NonMeat))
			{
				if (foodDef.IsCorpse)
				{
					return foodDef.ingestible.sourceDef?.race?.FleshType == FleshTypeDefOf.Insectoid;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public static float NutritionForEater(Pawn eater, Thing food)
	{
		float num = food.GetStatValue(StatDefOf.Nutrition);
		if (eater != null && ModsConfig.BiotechActive && food.def.IsRawHumanFood())
		{
			num *= eater.GetStatValue(StatDefOf.RawNutritionFactor);
		}
		return num;
	}

	public static bool IsRawHumanFood(this ThingDef thingDef)
	{
		if (thingDef.ingestible == null)
		{
			return false;
		}
		if (!thingDef.ingestible.HumanEdible)
		{
			return false;
		}
		if (thingDef.ingestible.preferability != FoodPreferability.RawBad)
		{
			return thingDef.ingestible.preferability == FoodPreferability.RawTasty;
		}
		return true;
	}

	public static bool IsHumanFood(this ThingDef thingDef)
	{
		if (thingDef.ingestible == null)
		{
			return false;
		}
		if (!thingDef.ingestible.HumanEdible)
		{
			return false;
		}
		if (thingDef.ingestible.preferability != FoodPreferability.Undefined)
		{
			return thingDef.ingestible.preferability != FoodPreferability.NeverForNutrition;
		}
		return false;
	}
}
