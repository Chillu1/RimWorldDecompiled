using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_DoBill : WorkGiver_Scanner
{
	private class DefCountList
	{
		private List<ThingDef> defs = new List<ThingDef>();

		private List<float> counts = new List<float>();

		public int Count => defs.Count;

		public float this[ThingDef def]
		{
			get
			{
				int num = defs.IndexOf(def);
				if (num < 0)
				{
					return 0f;
				}
				return counts[num];
			}
			set
			{
				int num = defs.IndexOf(def);
				if (num < 0)
				{
					defs.Add(def);
					counts.Add(value);
					num = defs.Count - 1;
				}
				else
				{
					counts[num] = value;
				}
				CheckRemove(num);
			}
		}

		public float GetCount(int index)
		{
			return counts[index];
		}

		public void SetCount(int index, float val)
		{
			counts[index] = val;
			CheckRemove(index);
		}

		public ThingDef GetDef(int index)
		{
			return defs[index];
		}

		private void CheckRemove(int index)
		{
			if (counts[index] == 0f)
			{
				counts.RemoveAt(index);
				defs.RemoveAt(index);
			}
		}

		public void Clear()
		{
			defs.Clear();
			counts.Clear();
		}

		public void GenerateFrom(List<Thing> things)
		{
			Clear();
			for (int i = 0; i < things.Count; i++)
			{
				this[things[i].def] += things[i].stackCount;
			}
		}
	}

	private List<ThingCount> chosenIngThings = new List<ThingCount>();

	private static List<IngredientCount> missingIngredients = new List<IngredientCount>();

	private static List<Thing> tmpMissingUniqueIngredients = new List<Thing>();

	private static readonly IntRange ReCheckFailedBillTicksRange = new IntRange(500, 600);

	private static List<Thing> relevantThings = new List<Thing>();

	private static HashSet<Thing> processedThings = new HashSet<Thing>();

	private static List<Thing> newRelevantThings = new List<Thing>();

	private static List<Thing> tmpMedicine = new List<Thing>();

	private static DefCountList availableCounts = new DefCountList();

	public override PathEndMode PathEndMode => PathEndMode.InteractionCell;

	public override ThingRequest PotentialWorkThingRequest
	{
		get
		{
			if (def.fixedBillGiverDefs != null && def.fixedBillGiverDefs.Count == 1)
			{
				return ThingRequest.ForDef(def.fixedBillGiverDefs[0]);
			}
			return ThingRequest.ForGroup(ThingRequestGroup.PotentialBillGiver);
		}
	}

	public override Danger MaxPathDanger(Pawn pawn)
	{
		return Danger.Some;
	}

	public override bool ShouldSkip(Pawn pawn, bool forced = false)
	{
		List<Thing> list = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.PotentialBillGiver);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] is IBillGiver billGiver && billGiver != pawn && ThingIsUsableBillGiver(list[i]) && billGiver.BillStack.AnyShouldDoNow)
			{
				return false;
			}
		}
		return true;
	}

	public override Job JobOnThing(Pawn pawn, Thing thing, bool forced = false)
	{
		if (!(thing is IBillGiver billGiver) || !ThingIsUsableBillGiver(thing) || !billGiver.BillStack.AnyShouldDoNow || !billGiver.UsableForBillsAfterFueling() || !pawn.CanReserve(thing, 1, -1, null, forced) || thing.IsBurning())
		{
			return null;
		}
		if (thing.def.hasInteractionCell && !pawn.CanReserveSittableOrSpot(thing.InteractionCell, thing, forced))
		{
			return null;
		}
		CompRefuelable compRefuelable = thing.TryGetComp<CompRefuelable>();
		if (compRefuelable != null && !compRefuelable.HasFuel)
		{
			if (!RefuelWorkGiverUtility.CanRefuel(pawn, thing, forced))
			{
				return null;
			}
			return RefuelWorkGiverUtility.RefuelJob(pawn, thing, forced);
		}
		billGiver.BillStack.RemoveIncompletableBills();
		return StartOrResumeBillJob(pawn, billGiver, forced);
	}

	private static UnfinishedThing ClosestUnfinishedThingForBill(Pawn pawn, Bill_ProductionWithUft bill)
	{
		Predicate<Thing> validator = (Thing t) => !t.IsForbidden(pawn) && ((UnfinishedThing)t).Recipe == bill.recipe && ((UnfinishedThing)t).Creator == pawn && ((UnfinishedThing)t).ingredients.TrueForAll((Thing x) => bill.IsFixedOrAllowedIngredient(x.def)) && pawn.CanReserve(t);
		return (UnfinishedThing)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(bill.recipe.unfinishedThingDef), PathEndMode.InteractionCell, TraverseParms.For(pawn, pawn.NormalMaxDanger()), 9999f, validator);
	}

	private static Job FinishUftJob(Pawn pawn, UnfinishedThing uft, Bill_ProductionWithUft bill)
	{
		if (uft.Creator != pawn)
		{
			Log.Error("Tried to get FinishUftJob for " + pawn?.ToString() + " finishing " + uft?.ToString() + " but its creator is " + uft.Creator);
			return null;
		}
		Job job = WorkGiverUtility.HaulStuffOffBillGiverJob(pawn, bill.billStack.billGiver, uft);
		if (job != null && job.targetA.Thing != uft)
		{
			return job;
		}
		Job job2 = JobMaker.MakeJob(JobDefOf.DoBill, (Thing)bill.billStack.billGiver);
		job2.bill = bill;
		job2.targetQueueB = new List<LocalTargetInfo> { uft };
		job2.countQueue = new List<int> { 1 };
		job2.haulMode = HaulMode.ToCellNonStorage;
		return job2;
	}

	private Job StartOrResumeBillJob(Pawn pawn, IBillGiver giver, bool forced = false)
	{
		bool flag = FloatMenuMakerMap.makingFor == pawn;
		for (int i = 0; i < giver.BillStack.Count; i++)
		{
			Bill bill = giver.BillStack[i];
			if ((bill.recipe.requiredGiverWorkType != null && bill.recipe.requiredGiverWorkType != def.workType) || (Find.TickManager.TicksGame <= bill.nextTickToSearchForIngredients && FloatMenuMakerMap.makingFor != pawn) || !bill.ShouldDoNow() || !bill.PawnAllowedToStartAnew(pawn))
			{
				continue;
			}
			SkillRequirement skillRequirement = bill.recipe.FirstSkillRequirementPawnDoesntSatisfy(pawn);
			if (skillRequirement != null)
			{
				JobFailReason.Is("UnderRequiredSkill".Translate(skillRequirement.minLevel), bill.Label);
				continue;
			}
			if (bill is Bill_Medical bill_Medical)
			{
				if (bill_Medical.IsSurgeryViolationOnExtraFactionMember(pawn))
				{
					JobFailReason.Is("SurgeryViolationFellowFactionMember".Translate());
					continue;
				}
				if (!pawn.CanReserve(bill_Medical.GiverPawn, 1, -1, null, forced))
				{
					Pawn pawn2 = pawn.MapHeld.reservationManager.FirstRespectedReserver(bill_Medical.GiverPawn, pawn);
					JobFailReason.Is("IsReservedBy".Translate(bill_Medical.GiverPawn.LabelShort, pawn2.LabelShort));
					continue;
				}
			}
			if (bill is Bill_Mech bill_Mech && bill_Mech.Gestator.WasteProducer.Waste != null && bill_Mech.Gestator.GestatingMech == null)
			{
				JobFailReason.Is("WasteContainerFull".Translate());
				continue;
			}
			if (bill is Bill_ProductionWithUft bill_ProductionWithUft)
			{
				if (bill_ProductionWithUft.BoundUft != null)
				{
					if (bill_ProductionWithUft.BoundWorker == pawn && pawn.CanReserveAndReach(bill_ProductionWithUft.BoundUft, PathEndMode.Touch, Danger.Deadly) && !bill_ProductionWithUft.BoundUft.IsForbidden(pawn))
					{
						return FinishUftJob(pawn, bill_ProductionWithUft.BoundUft, bill_ProductionWithUft);
					}
					continue;
				}
				UnfinishedThing unfinishedThing = ClosestUnfinishedThingForBill(pawn, bill_ProductionWithUft);
				if (unfinishedThing != null)
				{
					return FinishUftJob(pawn, unfinishedThing, bill_ProductionWithUft);
				}
			}
			if (bill is Bill_Autonomous { State: not FormingState.Gathering } bill_Autonomous)
			{
				return WorkOnFormedBill((Thing)giver, bill_Autonomous);
			}
			List<IngredientCount> list = null;
			if (flag)
			{
				list = missingIngredients;
				list.Clear();
				tmpMissingUniqueIngredients.Clear();
			}
			Bill_Medical bill_Medical2 = bill as Bill_Medical;
			if (bill_Medical2 != null && bill_Medical2.uniqueRequiredIngredients?.NullOrEmpty() == false)
			{
				foreach (Thing uniqueRequiredIngredient in bill_Medical2.uniqueRequiredIngredients)
				{
					if (uniqueRequiredIngredient.IsForbidden(pawn) || !pawn.CanReserveAndReach(uniqueRequiredIngredient, PathEndMode.OnCell, Danger.Deadly))
					{
						tmpMissingUniqueIngredients.Add(uniqueRequiredIngredient);
					}
				}
			}
			if (!TryFindBestBillIngredients(bill, pawn, (Thing)giver, chosenIngThings, list) || !tmpMissingUniqueIngredients.NullOrEmpty())
			{
				if (FloatMenuMakerMap.makingFor != pawn)
				{
					bill.nextTickToSearchForIngredients = Find.TickManager.TicksGame + ReCheckFailedBillTicksRange.RandomInRange;
				}
				else if (flag)
				{
					if (CannotDoBillDueToMedicineRestriction(giver, bill, list))
					{
						JobFailReason.Is("NoMedicineMatchingCategory".Translate(GetMedicalCareCategory((Thing)giver).GetLabel().Named("CATEGORY")), bill.Label);
					}
					else
					{
						string text = list.Select((IngredientCount missing) => missing.Summary).Concat(tmpMissingUniqueIngredients.Select((Thing t) => t.Label)).ToCommaList();
						JobFailReason.Is("MissingMaterials".Translate(text), bill.Label);
					}
					flag = false;
				}
				chosenIngThings.Clear();
				continue;
			}
			flag = false;
			if (bill_Medical2 != null && bill_Medical2.uniqueRequiredIngredients?.NullOrEmpty() == false)
			{
				foreach (Thing uniqueRequiredIngredient2 in bill_Medical2.uniqueRequiredIngredients)
				{
					chosenIngThings.Add(new ThingCount(uniqueRequiredIngredient2, 1));
				}
			}
			Job haulOffJob;
			Job result = TryStartNewDoBillJob(pawn, bill, giver, chosenIngThings, out haulOffJob);
			chosenIngThings.Clear();
			return result;
		}
		chosenIngThings.Clear();
		return null;
	}

	private static bool CannotDoBillDueToMedicineRestriction(IBillGiver giver, Bill bill, List<IngredientCount> missingIngredients)
	{
		if (!(giver is Pawn pawn))
		{
			return false;
		}
		bool flag = false;
		foreach (IngredientCount missingIngredient in missingIngredients)
		{
			if (missingIngredient.filter.Allows(ThingDefOf.MedicineIndustrial))
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			MedicalCareCategory medicalCareCategory = GetMedicalCareCategory(pawn);
			foreach (Thing item in pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Medicine))
			{
				if (IsUsableIngredient(item, bill) && medicalCareCategory.AllowsMedicine(item.def))
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	public static Job TryStartNewDoBillJob(Pawn pawn, Bill bill, IBillGiver giver, List<ThingCount> chosenIngThings, out Job haulOffJob, bool dontCreateJobIfHaulOffRequired = true)
	{
		haulOffJob = WorkGiverUtility.HaulStuffOffBillGiverJob(pawn, giver, null);
		if (haulOffJob != null && dontCreateJobIfHaulOffRequired)
		{
			return haulOffJob;
		}
		Job job = JobMaker.MakeJob(JobDefOf.DoBill, (Thing)giver);
		job.targetQueueB = new List<LocalTargetInfo>(chosenIngThings.Count);
		job.countQueue = new List<int>(chosenIngThings.Count);
		for (int i = 0; i < chosenIngThings.Count; i++)
		{
			job.targetQueueB.Add(chosenIngThings[i].Thing);
			job.countQueue.Add(chosenIngThings[i].Count);
		}
		if (bill.xenogerm != null)
		{
			job.targetQueueB.Add(bill.xenogerm);
			job.countQueue.Add(1);
		}
		job.haulMode = HaulMode.ToCellNonStorage;
		job.bill = bill;
		return job;
	}

	private static Job WorkOnFormedBill(Thing giver, Bill_Autonomous bill)
	{
		Job job = JobMaker.MakeJob(JobDefOf.DoBill, giver);
		job.bill = bill;
		return job;
	}

	public bool ThingIsUsableBillGiver(Thing thing)
	{
		Pawn pawn = thing as Pawn;
		Corpse corpse = thing as Corpse;
		Pawn pawn2 = null;
		if (corpse != null)
		{
			pawn2 = corpse.InnerPawn;
		}
		if (def.fixedBillGiverDefs != null && def.fixedBillGiverDefs.Contains(thing.def))
		{
			return true;
		}
		if (pawn != null)
		{
			if (def.billGiversAllHumanlikes && pawn.RaceProps.Humanlike)
			{
				return true;
			}
			if (def.billGiversAllMechanoids && pawn.RaceProps.IsMechanoid)
			{
				return true;
			}
			if (def.billGiversAllAnimals && pawn.IsAnimal)
			{
				return true;
			}
		}
		if (corpse != null && pawn2 != null)
		{
			if (def.billGiversAllHumanlikesCorpses && pawn2.RaceProps.Humanlike)
			{
				return true;
			}
			if (def.billGiversAllMechanoidsCorpses && pawn2.RaceProps.IsMechanoid)
			{
				return true;
			}
			if (def.billGiversAllAnimalsCorpses && pawn2.IsAnimal)
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsUsableIngredient(Thing t, Bill bill)
	{
		if (!bill.IsFixedOrAllowedIngredient(t))
		{
			return false;
		}
		foreach (IngredientCount ingredient in bill.recipe.ingredients)
		{
			if (ingredient.filter.Allows(t))
			{
				return true;
			}
		}
		return false;
	}

	public static bool TryFindBestFixedIngredients(List<IngredientCount> ingredients, Pawn pawn, Thing ingredientDestination, List<ThingCount> chosen, float searchRadius = 999f)
	{
		return TryFindBestIngredientsHelper(delegate(Thing t)
		{
			foreach (IngredientCount ingredient in ingredients)
			{
				if (ingredient.filter.Allows(t))
				{
					return true;
				}
			}
			return false;
		}, (List<Thing> foundThings) => TryFindBestIngredientsInSet_NoMixHelper(foundThings, ingredients, chosen, GetBillGiverRootCell(ingredientDestination, pawn), alreadySorted: false, null), ingredients, pawn, ingredientDestination, chosen, searchRadius);
	}

	private static bool TryFindBestBillIngredients(Bill bill, Pawn pawn, Thing billGiver, List<ThingCount> chosen, List<IngredientCount> missingIngredients)
	{
		return TryFindBestIngredientsHelper((Thing t) => IsUsableIngredient(t, bill), (List<Thing> foundThings) => TryFindBestBillIngredientsInSet(foundThings, bill, chosen, GetBillGiverRootCell(billGiver, pawn), billGiver is Pawn, missingIngredients), bill.recipe.ingredients, pawn, billGiver, chosen, bill.ingredientSearchRadius);
	}

	private static bool TryFindBestIngredientsHelper(Predicate<Thing> thingValidator, Predicate<List<Thing>> foundAllIngredientsAndChoose, List<IngredientCount> ingredients, Pawn pawn, Thing billGiver, List<ThingCount> chosen, float searchRadius)
	{
		chosen.Clear();
		newRelevantThings.Clear();
		if (ingredients.Count == 0)
		{
			return true;
		}
		IntVec3 billGiverRootCell = GetBillGiverRootCell(billGiver, pawn);
		Region rootReg = billGiverRootCell.GetRegion(pawn.Map);
		if (rootReg == null)
		{
			return false;
		}
		relevantThings.Clear();
		processedThings.Clear();
		bool foundAll = false;
		float radiusSq = searchRadius * searchRadius;
		Predicate<Thing> baseValidator = (Thing t) => t.Spawned && thingValidator(t) && (float)(t.Position - billGiver.Position).LengthHorizontalSquared < radiusSq && !t.IsForbidden(pawn) && pawn.CanReserve(t);
		bool billGiverIsPawn = billGiver is Pawn;
		if (billGiverIsPawn)
		{
			AddEveryMedicineToRelevantThings(pawn, billGiver, relevantThings, baseValidator, pawn.Map);
			if (foundAllIngredientsAndChoose(relevantThings))
			{
				relevantThings.Clear();
				return true;
			}
		}
		if (billGiver is Building_WorkTableAutonomous building_WorkTableAutonomous)
		{
			relevantThings.AddRange(building_WorkTableAutonomous.innerContainer);
			if (foundAllIngredientsAndChoose(relevantThings))
			{
				relevantThings.Clear();
				return true;
			}
		}
		foreach (IHaulSource item in pawn.Map.haulDestinationManager.AllHaulSourcesListForReading)
		{
			if (!item.HaulSourceEnabled || !(item is Thing { Spawned: not false, Position: var position } thing) || !position.InHorDistOf(billGiver.Position, searchRadius) || thing.IsForbidden(pawn))
			{
				continue;
			}
			ThingOwnerUtility.GetAllThingsRecursively(item, newRelevantThings);
			foreach (Thing newRelevantThing in newRelevantThings)
			{
				if (!processedThings.Contains(newRelevantThing) && !newRelevantThing.IsForbidden(pawn) && pawn.CanReserve(newRelevantThing) && thingValidator(newRelevantThing))
				{
					relevantThings.Add(newRelevantThing);
					processedThings.Add(newRelevantThing);
				}
			}
		}
		newRelevantThings.Clear();
		TraverseParms traverseParams = TraverseParms.For(pawn);
		RegionEntryPredicate entryCondition = null;
		if (Math.Abs(999f - searchRadius) >= 1f)
		{
			entryCondition = delegate(Region from, Region r)
			{
				if (!r.Allows(traverseParams, isDestination: false))
				{
					return false;
				}
				CellRect extentsClose = r.extentsClose;
				int num = Math.Abs(billGiver.Position.x - Math.Max(extentsClose.minX, Math.Min(billGiver.Position.x, extentsClose.maxX)));
				if ((float)num > searchRadius)
				{
					return false;
				}
				int num2 = Math.Abs(billGiver.Position.z - Math.Max(extentsClose.minZ, Math.Min(billGiver.Position.z, extentsClose.maxZ)));
				return !((float)num2 > searchRadius) && (float)(num * num + num2 * num2) <= radiusSq;
			};
		}
		else
		{
			entryCondition = (Region from, Region r) => r.Allows(traverseParams, isDestination: false);
		}
		int adjacentRegionsAvailable = rootReg.Neighbors.Count((Region region) => entryCondition(rootReg, region));
		int regionsProcessed = 0;
		processedThings.AddRange(relevantThings);
		if (foundAllIngredientsAndChoose(relevantThings))
		{
			return true;
		}
		RegionProcessor regionProcessor = delegate(Region r)
		{
			foreach (Thing item2 in r.ListerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.HaulableEver)))
			{
				if (!processedThings.Contains(item2) && ReachabilityWithinRegion.ThingFromRegionListerReachable(item2, r, PathEndMode.ClosestTouch, pawn) && baseValidator(item2) && !(item2.def.IsMedicine && billGiverIsPawn))
				{
					newRelevantThings.Add(item2);
					processedThings.Add(item2);
				}
			}
			int num = regionsProcessed + 1;
			regionsProcessed = num;
			if (newRelevantThings.Count > 0 && regionsProcessed > adjacentRegionsAvailable)
			{
				relevantThings.AddRange(newRelevantThings);
				newRelevantThings.Clear();
				if (foundAllIngredientsAndChoose(relevantThings))
				{
					foundAll = true;
					return true;
				}
			}
			return false;
		};
		RegionTraverser.BreadthFirstTraverse(rootReg, entryCondition, regionProcessor, 99999);
		relevantThings.Clear();
		newRelevantThings.Clear();
		processedThings.Clear();
		return foundAll;
	}

	private static IntVec3 GetBillGiverRootCell(Thing billGiver, Pawn forPawn)
	{
		if (billGiver is Building building)
		{
			if (building.def.hasInteractionCell)
			{
				return building.InteractionCell;
			}
			Log.Error("Tried to find bill ingredients for " + billGiver?.ToString() + " which has no interaction cell.");
			return forPawn.Position;
		}
		return billGiver.Position;
	}

	private static void AddEveryMedicineToRelevantThings(Pawn pawn, Thing billGiver, List<Thing> relevantThings, Predicate<Thing> baseValidator, Map map)
	{
		MedicalCareCategory medicalCareCategory = GetMedicalCareCategory(billGiver);
		List<Thing> list = map.listerThings.ThingsInGroup(ThingRequestGroup.Medicine);
		tmpMedicine.Clear();
		for (int i = 0; i < list.Count; i++)
		{
			Thing thing = list[i];
			if (medicalCareCategory.AllowsMedicine(thing.def) && baseValidator(thing) && pawn.CanReach(thing, PathEndMode.OnCell, Danger.Deadly))
			{
				tmpMedicine.Add(thing);
			}
		}
		tmpMedicine.SortBy((Thing x) => 0f - x.GetStatValue(StatDefOf.MedicalPotency), (Thing x) => x.Position.DistanceToSquared(billGiver.Position));
		relevantThings.AddRange(tmpMedicine);
		tmpMedicine.Clear();
	}

	public static MedicalCareCategory GetMedicalCareCategory(Thing billGiver)
	{
		if (billGiver is Pawn { playerSettings: not null } pawn)
		{
			return pawn.playerSettings.medCare;
		}
		return MedicalCareCategory.Best;
	}

	private static bool TryFindBestBillIngredientsInSet(List<Thing> availableThings, Bill bill, List<ThingCount> chosen, IntVec3 rootCell, bool alreadySorted, List<IngredientCount> missingIngredients)
	{
		if (bill.recipe.allowMixingIngredients)
		{
			return TryFindBestBillIngredientsInSet_AllowMix(availableThings, bill, chosen, rootCell, missingIngredients);
		}
		return TryFindBestBillIngredientsInSet_NoMix(availableThings, bill, chosen, rootCell, alreadySorted, missingIngredients);
	}

	private static bool TryFindBestBillIngredientsInSet_NoMix(List<Thing> availableThings, Bill bill, List<ThingCount> chosen, IntVec3 rootCell, bool alreadySorted, List<IngredientCount> missingIngredients)
	{
		return TryFindBestIngredientsInSet_NoMixHelper(availableThings, bill.recipe.ingredients, chosen, rootCell, alreadySorted, missingIngredients, bill);
	}

	private static bool TryFindBestIngredientsInSet_NoMixHelper(List<Thing> availableThings, List<IngredientCount> ingredients, List<ThingCount> chosen, IntVec3 rootCell, bool alreadySorted, List<IngredientCount> missingIngredients, Bill bill = null)
	{
		if (!alreadySorted)
		{
			Comparison<Thing> comparison = delegate(Thing t1, Thing t2)
			{
				float num7 = (t1.PositionHeld - rootCell).LengthHorizontalSquared;
				float value = (t2.PositionHeld - rootCell).LengthHorizontalSquared;
				return num7.CompareTo(value);
			};
			availableThings.Sort(comparison);
		}
		chosen.Clear();
		availableCounts.Clear();
		missingIngredients?.Clear();
		availableCounts.GenerateFrom(availableThings);
		for (int num = 0; num < ingredients.Count; num++)
		{
			IngredientCount ingredientCount = ingredients[num];
			bool flag = false;
			for (int num2 = 0; num2 < availableCounts.Count; num2++)
			{
				float num3 = ((bill != null) ? ((float)ingredientCount.CountRequiredOfFor(availableCounts.GetDef(num2), bill.recipe, bill)) : ingredientCount.GetBaseCount());
				if ((bill != null && !bill.recipe.ignoreIngredientCountTakeEntireStacks && num3 > availableCounts.GetCount(num2)) || !ingredientCount.filter.Allows(availableCounts.GetDef(num2)) || (bill != null && !ingredientCount.IsFixedIngredient && !bill.ingredientFilter.Allows(availableCounts.GetDef(num2))))
				{
					continue;
				}
				for (int num4 = 0; num4 < availableThings.Count; num4++)
				{
					if (availableThings[num4].def != availableCounts.GetDef(num2))
					{
						continue;
					}
					int num5 = availableThings[num4].stackCount - ThingCountUtility.CountOf(chosen, availableThings[num4]);
					if (num5 > 0)
					{
						if (bill != null && bill.recipe.ignoreIngredientCountTakeEntireStacks)
						{
							ThingCountUtility.AddToList(chosen, availableThings[num4], num5);
							return true;
						}
						int num6 = Mathf.Min(Mathf.FloorToInt(num3), num5);
						ThingCountUtility.AddToList(chosen, availableThings[num4], num6);
						num3 -= (float)num6;
						if (num3 < 0.001f)
						{
							flag = true;
							float count = availableCounts.GetCount(num2);
							count -= num3;
							availableCounts.SetCount(num2, count);
							break;
						}
					}
				}
				if (flag)
				{
					break;
				}
			}
			if (!flag)
			{
				if (missingIngredients == null)
				{
					return false;
				}
				missingIngredients.Add(ingredientCount);
			}
		}
		if (missingIngredients != null)
		{
			return missingIngredients.Count == 0;
		}
		return true;
	}

	private static bool TryFindBestBillIngredientsInSet_AllowMix(List<Thing> availableThings, Bill bill, List<ThingCount> chosen, IntVec3 rootCell, List<IngredientCount> missingIngredients)
	{
		chosen.Clear();
		missingIngredients?.Clear();
		availableThings.SortBy((Thing t) => bill.recipe.IngredientValueGetter.ValuePerUnitOf(t.def), (Thing t) => (t.Position - rootCell).LengthHorizontalSquared);
		for (int num = 0; num < bill.recipe.ingredients.Count; num++)
		{
			IngredientCount ingredientCount = bill.recipe.ingredients[num];
			float num2 = ingredientCount.GetBaseCount();
			for (int num3 = 0; num3 < availableThings.Count; num3++)
			{
				Thing thing = availableThings[num3];
				if (ingredientCount.filter.Allows(thing) && (ingredientCount.IsFixedIngredient || bill.ingredientFilter.Allows(thing)))
				{
					float num4 = bill.recipe.IngredientValueGetter.ValuePerUnitOf(thing.def);
					int num5 = Mathf.Min(Mathf.CeilToInt(num2 / num4), thing.stackCount);
					ThingCountUtility.AddToList(chosen, thing, num5);
					num2 -= (float)num5 * num4;
					if (num2 <= 0.0001f)
					{
						break;
					}
				}
			}
			if (num2 > 0.0001f)
			{
				if (missingIngredients == null)
				{
					return false;
				}
				missingIngredients.Add(ingredientCount);
			}
		}
		if (missingIngredients != null)
		{
			return missingIngredients.Count == 0;
		}
		return true;
	}
}
