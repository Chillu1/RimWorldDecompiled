using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class FoodUtility
	{
		public const int FoodPoisoningStageInitial = 2;

		public const int FoodPoisoningStageMajor = 1;

		public const int FoodPoisoningStageRecovering = 0;

		public static float? bestFoodSourceOnMap_minNutrition_NewTemp = null;

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

		private static List<ThoughtDef> ingestThoughts = new List<ThoughtDef>();

		public static bool WillEat(this Pawn p, Thing food, Pawn getter = null, bool careIfNotAcceptableForTitle = true)
		{
			if (!p.RaceProps.CanEverEat(food))
			{
				return false;
			}
			if (p.foodRestriction != null)
			{
				FoodRestriction currentRespectedRestriction = p.foodRestriction.GetCurrentRespectedRestriction(getter);
				if (currentRespectedRestriction != null && !currentRespectedRestriction.Allows(food) && (food.def.IsWithinCategory(ThingCategoryDefOf.Foods) || food.def.IsWithinCategory(ThingCategoryDefOf.Corpses)))
				{
					return false;
				}
			}
			if (careIfNotAcceptableForTitle && InappropriateForTitle(food.def, p, allowIfStarving: true))
			{
				return false;
			}
			return true;
		}

		public static bool WillEat(this Pawn p, ThingDef food, Pawn getter = null, bool careIfNotAcceptableForTitle = true)
		{
			if (!p.RaceProps.CanEverEat(food))
			{
				return false;
			}
			if (p.foodRestriction != null)
			{
				FoodRestriction currentRespectedRestriction = p.foodRestriction.GetCurrentRespectedRestriction(getter);
				if (currentRespectedRestriction != null && !currentRespectedRestriction.Allows(food) && food.IsWithinCategory(currentRespectedRestriction.filter.DisplayRootCategory.catDef))
				{
					return false;
				}
			}
			if (careIfNotAcceptableForTitle && InappropriateForTitle(food, p, allowIfStarving: true))
			{
				return false;
			}
			return true;
		}

		public static bool InappropriateForTitle(ThingDef food, Pawn p, bool allowIfStarving)
		{
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

		public static bool TryFindBestFoodSourceFor(Pawn getter, Pawn eater, bool desperate, out Thing foodSource, out ThingDef foodDef, bool canRefillDispenser = true, bool canUseInventory = true, bool allowForbidden = false, bool allowCorpse = true, bool allowSociallyImproper = false, bool allowHarvest = false, bool forceScanWholeMap = false, bool ignoreReservations = false, FoodPreferability minPrefOverride = FoodPreferability.Undefined)
		{
			bool flag = getter.RaceProps.ToolUser && getter.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation);
			bool allowDrug = !eater.IsTeetotaler();
			Thing thing = null;
			if (canUseInventory)
			{
				if (flag)
				{
					thing = BestFoodInInventory(getter, eater, (minPrefOverride == FoodPreferability.Undefined) ? FoodPreferability.MealAwful : minPrefOverride);
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
			bool allowPlant = getter == eater;
			bool allowForbidden2 = allowForbidden;
			ThingDef foodDef2;
			Thing thing2 = BestFoodSourceOnMap(getter, eater, desperate, out foodDef2, FoodPreferability.MealLavish, allowPlant, allowDrug, allowCorpse, allowDispenserFull: true, canRefillDispenser, allowForbidden2, allowSociallyImproper, allowHarvest, forceScanWholeMap, ignoreReservations, minPrefOverride);
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
				float num2 = FoodOptimality(eater, thing, finalIngestibleDef, 0f);
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
			if (canUseInventory && flag)
			{
				thing = BestFoodInInventory(getter, eater, FoodPreferability.DesperateOnly, FoodPreferability.MealLavish, 0f, allowDrug);
				if (thing != null)
				{
					foodSource = thing;
					foodDef = GetFinalIngestibleDef(foodSource);
					return true;
				}
			}
			if (thing2 == null && getter == eater && (getter.RaceProps.predator || (getter.IsWildMan() && !getter.IsPrisoner)))
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
		}

		public static ThingDef GetFinalIngestibleDef(Thing foodSource, bool harvest = false)
		{
			Building_NutrientPasteDispenser building_NutrientPasteDispenser = foodSource as Building_NutrientPasteDispenser;
			if (building_NutrientPasteDispenser != null)
			{
				return building_NutrientPasteDispenser.DispensableDef;
			}
			Pawn pawn = foodSource as Pawn;
			if (pawn != null)
			{
				return pawn.RaceProps.corpseDef;
			}
			if (harvest)
			{
				Plant plant = foodSource as Plant;
				if (plant != null && plant.HarvestableNow && plant.def.plant.harvestedThingDef.IsIngestible)
				{
					return plant.def.plant.harvestedThingDef;
				}
			}
			return foodSource.def;
		}

		public static Thing BestFoodInInventory(Pawn holder, Pawn eater = null, FoodPreferability minFoodPref = FoodPreferability.NeverForNutrition, FoodPreferability maxFoodPref = FoodPreferability.MealLavish, float minStackNutrition = 0f, bool allowDrug = false)
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
				if (thing.def.IsNutritionGivingIngestible && thing.IngestibleNow && eater.WillEat(thing, holder) && (int)thing.def.ingestible.preferability >= (int)minFoodPref && (int)thing.def.ingestible.preferability <= (int)maxFoodPref && (allowDrug || !thing.def.IsDrug) && thing.GetStatValue(StatDefOf.Nutrition) * (float)thing.stackCount >= minStackNutrition)
				{
					return thing;
				}
			}
			return null;
		}

		public static int GetMaxAmountToPickup(Thing food, Pawn pawn, int wantedCount)
		{
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
			int num = Math.Min(wantedCount, food.stackCount);
			if (food.Spawned && food.Map != null)
			{
				return Math.Min(num, food.Map.reservationManager.CanReserveStack(pawn, food, 10));
			}
			return num;
		}

		public static Thing BestFoodSourceOnMap(Pawn getter, Pawn eater, bool desperate, out ThingDef foodDef, FoodPreferability maxPref = FoodPreferability.MealLavish, bool allowPlant = true, bool allowDrug = true, bool allowCorpse = true, bool allowDispenserFull = true, bool allowDispenserEmpty = true, bool allowForbidden = false, bool allowSociallyImproper = false, bool allowHarvest = false, bool forceScanWholeMap = false, bool ignoreReservations = false, FoodPreferability minPrefOverride = FoodPreferability.Undefined)
		{
			foodDef = null;
			bool getterCanManipulate = getter.RaceProps.ToolUser && getter.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation);
			if (!getterCanManipulate && getter != eater)
			{
				Log.Error(getter + " tried to find food to bring to " + eater + " but " + getter + " is incapable of Manipulation.");
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
				}
			}
			else
			{
				minPref = minPrefOverride;
			}
			Predicate<Thing> foodValidator = delegate(Thing t)
			{
				Building_NutrientPasteDispenser building_NutrientPasteDispenser = t as Building_NutrientPasteDispenser;
				if (building_NutrientPasteDispenser != null)
				{
					if (!allowDispenserFull || !getterCanManipulate || (int)ThingDefOf.MealNutrientPaste.ingestible.preferability < (int)minPref || (int)ThingDefOf.MealNutrientPaste.ingestible.preferability > (int)maxPref || !eater.WillEat(ThingDefOf.MealNutrientPaste, getter) || (t.Faction != getter.Faction && t.Faction != getter.HostFaction) || (!allowForbidden && t.IsForbidden(getter)) || !building_NutrientPasteDispenser.powerComp.PowerOn || (!allowDispenserEmpty && !building_NutrientPasteDispenser.HasEnoughFeedstockInHoppers()) || !t.InteractionCell.Standable(t.Map) || !IsFoodSourceOnMapSociallyProper(t, getter, eater, allowSociallyImproper) || !getter.Map.reachability.CanReachNonLocal(getter.Position, new TargetInfo(t.InteractionCell, t.Map), PathEndMode.OnCell, TraverseParms.For(getter, Danger.Some)))
					{
						return false;
					}
				}
				else
				{
					int stackCount = 1;
					if (bestFoodSourceOnMap_minNutrition_NewTemp.HasValue)
					{
						float statValue = t.GetStatValue(StatDefOf.Nutrition);
						stackCount = StackCountForNutrition(bestFoodSourceOnMap_minNutrition_NewTemp.Value, statValue);
					}
					if ((int)t.def.ingestible.preferability < (int)minPref || (int)t.def.ingestible.preferability > (int)maxPref || !eater.WillEat(t, getter) || !t.def.IsNutritionGivingIngestible || !t.IngestibleNow || (!allowCorpse && t is Corpse) || (!allowDrug && t.def.IsDrug) || (!allowForbidden && t.IsForbidden(getter)) || (!desperate && t.IsNotFresh()) || t.IsDessicated() || !IsFoodSourceOnMapSociallyProper(t, getter, eater, allowSociallyImproper) || (!getter.AnimalAwareOf(t) && !forceScanWholeMap) || (!ignoreReservations && !getter.CanReserve(t, 10, stackCount)))
					{
						return false;
					}
				}
				return true;
			};
			ThingRequest thingRequest = ((eater.RaceProps.foodType & (FoodTypeFlags.Plant | FoodTypeFlags.Tree)) == 0 || !allowPlant) ? ThingRequest.ForGroup(ThingRequestGroup.FoodSourceNotPlantOrTree) : ThingRequest.ForGroup(ThingRequestGroup.FoodSource);
			Thing bestThing;
			if (getter.RaceProps.Humanlike)
			{
				bestThing = SpawnedFoodSearchInnerScan(eater, getter.Position, getter.Map.listerThings.ThingsMatching(thingRequest), PathEndMode.ClosestTouch, TraverseParms.For(getter), 9999f, foodValidator);
				if (allowHarvest & getterCanManipulate)
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
						if (!eater.WillEat(harvestedThingDef, getter))
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
					Pawn pawn = item as Pawn;
					if (pawn != null && pawn != getter && pawn.RaceProps.Animal && pawn.CurJob != null && pawn.CurJob.def == JobDefOf.Ingest && pawn.CurJob.GetTarget(TargetIndex.A).HasThing)
					{
						filtered.Add(pawn.CurJob.GetTarget(TargetIndex.A).Thing);
					}
				}
				bool ignoreEntirelyForbiddenRegions = !allowForbidden && ForbidUtility.CaresAboutForbidden(getter, cellTarget: true) && getter.playerSettings != null && getter.playerSettings.EffectiveAreaRestrictionInPawnCurrentMap != null;
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
					return (!t.IsNotFresh()) ? true : false;
				};
				bestThing = GenClosest.ClosestThingReachable(getter.Position, getter.Map, thingRequest, PathEndMode.ClosestTouch, TraverseParms.For(getter), 9999f, validator, null, 0, maxRegionsToScan, forceAllowGlobalSearch: false, RegionType.Set_Passable, ignoreEntirelyForbiddenRegions);
				filtered.Clear();
				if (bestThing == null)
				{
					desperate = true;
					bestThing = GenClosest.ClosestThingReachable(getter.Position, getter.Map, thingRequest, PathEndMode.ClosestTouch, TraverseParms.For(getter), 9999f, foodValidator, null, 0, maxRegionsToScan, forceAllowGlobalSearch: false, RegionType.Set_Passable, ignoreEntirelyForbiddenRegions);
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
				return 100;
			}
			return 30;
		}

		private static bool IsFoodSourceOnMapSociallyProper(Thing t, Pawn getter, Pawn eater, bool allowSociallyImproper)
		{
			if (!allowSociallyImproper)
			{
				bool animalsCare = !getter.RaceProps.Animal;
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
				List<ThoughtDef> list = ThoughtsFromIngesting(eater, foodSource, foodDef);
				for (int i = 0; i < list.Count; i++)
				{
					num += FoodOptimalityEffectFromMoodCurve.Evaluate(list[i].stages[0].baseMoodEffect);
				}
			}
			if (foodDef.ingestible != null)
			{
				if (eater.RaceProps.Humanlike)
				{
					num += foodDef.ingestible.optimalityOffsetHumanlikes;
				}
				else if (eater.RaceProps.Animal)
				{
					num += foodDef.ingestible.optimalityOffsetFeedingAnimals;
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
			Pawn pawn = Find.Selector.SingleSelectedThing as Pawn;
			if (pawn != null && pawn.Map == Find.CurrentMap)
			{
				Thing thing = SpawnedFoodSearchInnerScan(pawn, root, Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.FoodSourceNotPlantOrTree), PathEndMode.ClosestTouch, TraverseParms.For(TraverseMode.PassDoors));
				if (thing != null)
				{
					GenDraw.DrawLineBetween(root.ToVector3Shifted(), thing.Position.ToVector3Shifted());
				}
			}
		}

		public static void DebugFoodSearchFromMouse_OnGUI()
		{
			IntVec3 a = UI.MouseCell();
			Pawn pawn = Find.Selector.SingleSelectedThing as Pawn;
			if (pawn != null && pawn.Map == Find.CurrentMap)
			{
				Text.Anchor = TextAnchor.MiddleCenter;
				Text.Font = GameFont.Tiny;
				foreach (Thing item in Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.FoodSourceNotPlantOrTree))
				{
					ThingDef finalIngestibleDef = GetFinalIngestibleDef(item);
					float num = FoodOptimality(pawn, item, finalIngestibleDef, (a - item.Position).LengthHorizontal);
					Vector2 vector = item.DrawPos.MapToUIPosition();
					Rect rect = new Rect(vector.x - 100f, vector.y - 100f, 200f, 200f);
					string text = num.ToString("F0");
					List<ThoughtDef> list = ThoughtsFromIngesting(pawn, item, finalIngestibleDef);
					for (int i = 0; i < list.Count; i++)
					{
						text = text + "\n" + list[i].defName + "(" + FoodOptimalityEffectFromMoodCurve.Evaluate(list[i].stages[0].baseMoodEffect).ToString("F0") + ")";
					}
					Widgets.Label(rect, text);
				}
				Text.Anchor = TextAnchor.UpperLeft;
			}
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
			if (GetMaxRegionsToScan(predator, forceScanWholeMap) < 0)
			{
				tmpPredatorCandidates.AddRange(predator.Map.mapPawns.AllPawnsSpawned);
			}
			else
			{
				TraverseParms traverseParms = TraverseParms.For(predator);
				RegionTraverser.BreadthFirstTraverse(predator.Position, predator.Map, (Region from, Region to) => to.Allows(traverseParms, isDestination: true), delegate(Region x)
				{
					List<Thing> list = x.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn);
					for (int j = 0; j < list.Count; j++)
					{
						tmpPredatorCandidates.Add((Pawn)list[j]);
					}
					return false;
				});
			}
			Pawn pawn = null;
			float num = 0f;
			bool tutorialMode = TutorSystem.TutorialMode;
			for (int i = 0; i < tmpPredatorCandidates.Count; i++)
			{
				Pawn pawn2 = tmpPredatorCandidates[i];
				if (predator.GetRoom() == pawn2.GetRoom() && predator != pawn2 && (!flag || pawn2.Downed) && IsAcceptablePreyFor(predator, pawn2) && predator.CanReach(pawn2, PathEndMode.ClosestTouch, Danger.Deadly) && !pawn2.IsForbidden(predator) && (!tutorialMode || pawn2.Faction != Faction.OfPlayer))
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
				float num = prey.kindDef.combatPower * prey.health.summaryHealth.SummaryHealthPercent * prey.ageTracker.CurLifeStage.bodySizeFactor;
				float num2 = predator.kindDef.combatPower * predator.health.summaryHealth.SummaryHealthPercent * predator.ageTracker.CurLifeStage.bodySizeFactor;
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
			return num3;
		}

		public static void DebugDrawPredatorFoodSource()
		{
			Pawn pawn = Find.Selector.SingleSelectedThing as Pawn;
			if (pawn == null || !TryFindBestFoodSourceFor(pawn, pawn, desperate: true, out Thing foodSource, out ThingDef _, canRefillDispenser: false, canUseInventory: false))
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

		public static List<ThoughtDef> ThoughtsFromIngesting(Pawn ingester, Thing foodSource, ThingDef foodDef)
		{
			ingestThoughts.Clear();
			if (ingester.needs == null || ingester.needs.mood == null)
			{
				return ingestThoughts;
			}
			if (!ingester.story.traits.HasTrait(TraitDefOf.Ascetic) && foodDef.ingestible.tasteThought != null)
			{
				ingestThoughts.Add(foodDef.ingestible.tasteThought);
			}
			CompIngredients compIngredients = foodSource.TryGetComp<CompIngredients>();
			Building_NutrientPasteDispenser building_NutrientPasteDispenser = foodSource as Building_NutrientPasteDispenser;
			if (IsHumanlikeMeat(foodDef) && ingester.RaceProps.Humanlike)
			{
				ingestThoughts.Add(ingester.story.traits.HasTrait(TraitDefOf.Cannibal) ? ThoughtDefOf.AteHumanlikeMeatDirectCannibal : ThoughtDefOf.AteHumanlikeMeatDirect);
			}
			else if (compIngredients != null)
			{
				for (int i = 0; i < compIngredients.ingredients.Count; i++)
				{
					AddIngestThoughtsFromIngredient(compIngredients.ingredients[i], ingester, ingestThoughts);
				}
			}
			else if (building_NutrientPasteDispenser != null)
			{
				Thing thing = building_NutrientPasteDispenser.FindFeedInAnyHopper();
				if (thing != null)
				{
					AddIngestThoughtsFromIngredient(thing.def, ingester, ingestThoughts);
				}
			}
			if (foodDef.ingestible.specialThoughtDirect != null)
			{
				ingestThoughts.Add(foodDef.ingestible.specialThoughtDirect);
			}
			if (foodSource.IsNotFresh())
			{
				ingestThoughts.Add(ThoughtDefOf.AteRottenFood);
			}
			if (ModsConfig.RoyaltyActive && InappropriateForTitle(foodDef, ingester, allowIfStarving: false))
			{
				ingestThoughts.Add(ThoughtDefOf.AteFoodInappropriateForTitle);
			}
			return ingestThoughts;
		}

		private static void AddIngestThoughtsFromIngredient(ThingDef ingredient, Pawn ingester, List<ThoughtDef> ingestThoughts)
		{
			if (ingredient.ingestible != null)
			{
				if (ingester.RaceProps.Humanlike && IsHumanlikeMeat(ingredient))
				{
					ingestThoughts.Add(ingester.story.traits.HasTrait(TraitDefOf.Cannibal) ? ThoughtDefOf.AteHumanlikeMeatAsIngredientCannibal : ThoughtDefOf.AteHumanlikeMeatAsIngredient);
				}
				else if (ingredient.ingestible.specialThoughtAsIngredient != null)
				{
					ingestThoughts.Add(ingredient.ingestible.specialThoughtAsIngredient);
				}
			}
		}

		public static bool IsHumanlikeMeat(ThingDef def)
		{
			if (def.ingestible.sourceDef != null && def.ingestible.sourceDef.race != null && def.ingestible.sourceDef.race.Humanlike)
			{
				return true;
			}
			return false;
		}

		public static bool IsHumanlikeMeatOrHumanlikeCorpse(Thing thing)
		{
			if (IsHumanlikeMeat(thing.def))
			{
				return true;
			}
			Corpse corpse = thing as Corpse;
			if (corpse != null && corpse.InnerPawn.RaceProps.Humanlike)
			{
				return true;
			}
			return false;
		}

		public static int WillIngestStackCountOf(Pawn ingester, ThingDef def, float singleFoodNutrition)
		{
			int num = Mathf.Min(def.ingestible.maxNumToIngestAtOnce, StackCountForNutrition(ingester.needs.food.NutritionWanted, singleFoodNutrition));
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
				return num;
			}
			return num;
		}

		public static bool Starving(this Pawn p)
		{
			if (p.needs != null && p.needs.food != null)
			{
				return p.needs.food.Starving;
			}
			return false;
		}

		public static float GetNutrition(Thing foodSource, ThingDef foodDef)
		{
			if (foodSource == null || foodDef == null)
			{
				return 0f;
			}
			if (foodSource.def == foodDef)
			{
				return foodSource.GetStatValue(StatDefOf.Nutrition);
			}
			return foodDef.GetStatValueAbstract(StatDefOf.Nutrition);
		}
	}
}
