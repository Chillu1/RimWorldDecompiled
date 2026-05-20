using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse.AI;
using Verse.AI.Group;

namespace Verse;

public static class AnimalPenUtility
{
	private static ThingFilter fixedFilter;

	private static ThingFilter defaultFilter;

	private static readonly AnimalPenConnectedDistrictsCalculator tmpConnectedDistrictsCalc = new AnimalPenConnectedDistrictsCalculator();

	public static ThingFilter GetFixedAnimalFilter()
	{
		if (fixedFilter == null)
		{
			fixedFilter = new ThingFilter();
			foreach (ThingDef item in DefDatabase<ThingDef>.AllDefs.Where(IsRopeManagedAnimalDef))
			{
				fixedFilter.SetAllow(item, allow: true);
			}
		}
		return fixedFilter;
	}

	public static bool IsRopeManagedAnimalDef(ThingDef td)
	{
		if (td.race != null && td.race.Roamer)
		{
			return td.IsWithinCategory(ThingCategoryDefOf.Animals);
		}
		return false;
	}

	private static bool ShouldBePennedByDefault(ThingDef td)
	{
		if (IsRopeManagedAnimalDef(td))
		{
			return td.race.FenceBlocked;
		}
		return false;
	}

	public static bool NeedsToBeManagedByRope(Pawn pawn)
	{
		if (IsRopeManagedAnimalDef(pawn.def) && pawn.Spawned && pawn.Map.IsPlayerHome)
		{
			return pawn.Roamer;
		}
		return false;
	}

	public static ThingFilter GetDefaultAnimalFilter()
	{
		if (defaultFilter == null)
		{
			defaultFilter = new ThingFilter();
			foreach (ThingDef item in DefDatabase<ThingDef>.AllDefs.Where(ShouldBePennedByDefault))
			{
				defaultFilter.SetAllow(item, allow: true);
			}
		}
		return defaultFilter;
	}

	public static void ResetStaticData()
	{
		defaultFilter = null;
		fixedFilter = null;
	}

	public static CompAnimalPenMarker GetCurrentPenOf(Pawn animal, bool allowUnenclosedPens)
	{
		if (!animal.Roamer)
		{
			return null;
		}
		Map map = animal.Map;
		if (!map.listerBuildings.allBuildingsAnimalPenMarkers.Any())
		{
			return null;
		}
		List<District> list = tmpConnectedDistrictsCalc.CalculateConnectedDistricts(animal.Position, map);
		if (!list.Any())
		{
			return null;
		}
		CompAnimalPenMarker compAnimalPenMarker = null;
		foreach (Building allBuildingsAnimalPenMarker in map.listerBuildings.allBuildingsAnimalPenMarkers)
		{
			if (list.Contains(allBuildingsAnimalPenMarker.Position.GetDistrict(map)))
			{
				CompAnimalPenMarker compAnimalPenMarker2 = allBuildingsAnimalPenMarker.TryGetComp<CompAnimalPenMarker>();
				if (CanUseAndReach(animal, compAnimalPenMarker2, allowUnenclosedPens) && (compAnimalPenMarker == null || (compAnimalPenMarker2.PenState.Enclosed && compAnimalPenMarker.PenState.Unenclosed) || map.cellIndices.CellToIndex(compAnimalPenMarker2.parent.Position) < map.cellIndices.CellToIndex(compAnimalPenMarker.parent.Position)))
				{
					compAnimalPenMarker = compAnimalPenMarker2;
				}
			}
		}
		tmpConnectedDistrictsCalc.Reset();
		return compAnimalPenMarker;
	}

	public static bool AnySuitablePens(Pawn animal, bool allowUnenclosedPens)
	{
		foreach (Building allBuildingsAnimalPenMarker in animal.Map.listerBuildings.allBuildingsAnimalPenMarkers)
		{
			CompAnimalPenMarker penMarker = allBuildingsAnimalPenMarker.TryGetComp<CompAnimalPenMarker>();
			if (CanUseAndReach(animal, penMarker, allowUnenclosedPens))
			{
				return true;
			}
		}
		return false;
	}

	public static bool AnySuitableHitch(Pawn animal)
	{
		foreach (Building allBuildingsHitchingPost in animal.Map.listerBuildings.allBuildingsHitchingPosts)
		{
			if (animal.CanReach(allBuildingsHitchingPost, PathEndMode.Touch, Danger.Deadly))
			{
				return true;
			}
		}
		return false;
	}

	public static CompAnimalPenMarker ClosestSuitablePen(Pawn animal, bool allowUnenclosedPens)
	{
		Map map = animal.Map;
		CompAnimalPenMarker compAnimalPenMarker = null;
		float num = 0f;
		foreach (Building allBuildingsAnimalPenMarker in map.listerBuildings.allBuildingsAnimalPenMarkers)
		{
			CompAnimalPenMarker compAnimalPenMarker2 = allBuildingsAnimalPenMarker.TryGetComp<CompAnimalPenMarker>();
			if (CanUseAndReach(animal, compAnimalPenMarker2, allowUnenclosedPens))
			{
				int num2 = animal.Position.DistanceToSquared(compAnimalPenMarker2.parent.Position);
				if (compAnimalPenMarker == null || (float)num2 < num)
				{
					compAnimalPenMarker = compAnimalPenMarker2;
					num = num2;
				}
			}
		}
		return compAnimalPenMarker;
	}

	private static bool CanUseAndReach(Pawn animal, CompAnimalPenMarker penMarker, bool allowUnenclosedPens, Pawn roper = null)
	{
		bool foundEnclosed = false;
		return CheckUseAndReach(animal, penMarker, allowUnenclosedPens, roper, ref foundEnclosed, ref foundEnclosed, ref foundEnclosed);
	}

	private static bool CheckUseAndReach(Pawn animal, CompAnimalPenMarker penMarker, bool allowUnenclosedPens, Pawn roper, ref bool foundEnclosed, ref bool foundUsable, ref bool foundReachable)
	{
		if (!allowUnenclosedPens && penMarker.PenState.Unenclosed)
		{
			return false;
		}
		foundEnclosed = true;
		if (!penMarker.AcceptsToPen(animal))
		{
			return false;
		}
		if (roper == null && penMarker.parent.IsForbidden(Faction.OfPlayer))
		{
			return false;
		}
		if (roper != null && penMarker.parent.IsForbidden(roper))
		{
			return false;
		}
		foundUsable = true;
		bool flag;
		if (roper == null)
		{
			TraverseParms traverseParams = TraverseParms.For(TraverseMode.PassDoors).WithFenceblockedOf(animal);
			flag = animal.Map.reachability.CanReach(animal.Position, penMarker.parent, PathEndMode.Touch, traverseParams);
		}
		else
		{
			TraverseParms traverseParams2 = TraverseParms.For(roper).WithFenceblockedOf(animal);
			flag = animal.Map.reachability.CanReach(animal.Position, penMarker.parent, PathEndMode.Touch, traverseParams2);
		}
		if (!flag)
		{
			return false;
		}
		foundReachable = true;
		return true;
	}

	public static CompAnimalPenMarker GetPenAnimalShouldBeTakenTo(Pawn roper, Pawn animal, out string jobFailReason, bool forced = false, bool canInteractWhileSleeping = true, bool allowUnenclosedPens = false, bool ignoreSkillRequirements = true, RopingPriority mode = RopingPriority.Closest, AnimalPenBalanceCalculator balanceCalculator = null)
	{
		jobFailReason = null;
		if (allowUnenclosedPens && mode == RopingPriority.Balanced)
		{
			Log.Warning("Cannot allow unenclosed pens in balanced mode");
			return null;
		}
		if (animal == roper)
		{
			return null;
		}
		if (animal == null || !NeedsToBeManagedByRope(animal))
		{
			return null;
		}
		if (animal.Faction != roper.Faction)
		{
			return null;
		}
		if (!forced && animal.roping.IsRopedByPawn && animal.roping.RopedByPawn != roper)
		{
			return null;
		}
		if (RopeAttachmentInteractionCell(roper, animal) == IntVec3.Invalid)
		{
			jobFailReason = "CantRopeAnimalCantTouch".Translate();
			return null;
		}
		if (!forced && !roper.CanReserve(animal))
		{
			return null;
		}
		CompAnimalPenMarker currentPenOf = GetCurrentPenOf(animal, allowUnenclosedPens);
		if (mode == RopingPriority.Closest && currentPenOf != null && currentPenOf.PenState.Enclosed)
		{
			return null;
		}
		if (!WorkGiver_InteractAnimal.CanInteractWithAnimal(roper, animal, out jobFailReason, forced, canInteractWhileSleeping, ignoreSkillRequirements, canInteractWhileRoaming: true))
		{
			return null;
		}
		Map map = animal.Map;
		AnimalPenBalanceCalculator animalPenBalanceCalculator = balanceCalculator ?? new AnimalPenBalanceCalculator(map, considerInProgressMovement: true);
		CompAnimalPenMarker compAnimalPenMarker = null;
		bool flag = false;
		bool flag2 = false;
		bool foundEnclosed = false;
		bool foundUsable = false;
		bool foundReachable = false;
		foreach (Building allBuildingsAnimalPenMarker in map.listerBuildings.allBuildingsAnimalPenMarkers)
		{
			CompAnimalPenMarker compAnimalPenMarker2 = allBuildingsAnimalPenMarker.TryGetComp<CompAnimalPenMarker>();
			flag2 = true;
			if (!CheckUseAndReach(animal, compAnimalPenMarker2, allowUnenclosedPens, roper, ref foundEnclosed, ref foundUsable, ref foundReachable))
			{
				continue;
			}
			switch (mode)
			{
			case RopingPriority.Closest:
				if (compAnimalPenMarker == null || (compAnimalPenMarker2.PenState.Enclosed && compAnimalPenMarker.PenState.Unenclosed) || PenIsCloser(compAnimalPenMarker2, compAnimalPenMarker, animal))
				{
					compAnimalPenMarker = compAnimalPenMarker2;
				}
				break;
			case RopingPriority.Balanced:
				if (currentPenOf != null && !animalPenBalanceCalculator.IsBetterPen(compAnimalPenMarker2, currentPenOf, leavingMarkerB: true, animal))
				{
					flag = true;
				}
				else if (compAnimalPenMarker == null || animalPenBalanceCalculator.IsBetterPen(compAnimalPenMarker2, compAnimalPenMarker, leavingMarkerB: false, animal))
				{
					compAnimalPenMarker = compAnimalPenMarker2;
					flag = false;
				}
				break;
			}
		}
		if (currentPenOf != null && compAnimalPenMarker == currentPenOf)
		{
			return null;
		}
		if (compAnimalPenMarker == null)
		{
			if (flag)
			{
				jobFailReason = "CantRopeAnimalAlreadyInBestPen".Translate();
			}
			else if (!flag2)
			{
				jobFailReason = "CantRopeAnimalNoUsableReachablePens".Translate();
			}
			else if (!foundEnclosed)
			{
				jobFailReason = "CantRopeAnimalNoEnclosedPens".Translate();
			}
			else if (!foundUsable)
			{
				jobFailReason = "CantRopeAnimalNoAllowedPens".Translate();
			}
			else if (!foundReachable)
			{
				jobFailReason = "CantRopeAnimalNoReachablePens".Translate();
			}
			else
			{
				jobFailReason = "CantRopeAnimalNoUsableReachablePens".Translate();
			}
			return null;
		}
		return compAnimalPenMarker;
	}

	public static Building GetHitchingPostAnimalShouldBeTakenTo(Pawn roper, Pawn animal, out string jobFailReason, bool forced = false)
	{
		jobFailReason = null;
		if (animal == roper)
		{
			return null;
		}
		if (animal == null || !animal.Roamer || !IsRopeManagedAnimalDef(animal.def))
		{
			return null;
		}
		if (animal.Faction != roper.Faction)
		{
			return null;
		}
		if (!forced && animal.roping.IsRopedByPawn && animal.roping.RopedByPawn != roper)
		{
			return null;
		}
		if (RopeAttachmentInteractionCell(roper, animal) == IntVec3.Invalid)
		{
			jobFailReason = "CantRopeAnimalCantTouch".Translate();
			return null;
		}
		if (!forced && !roper.CanReserve(animal))
		{
			return null;
		}
		if (animal.roping.IsRopedToHitchingPost || GetCurrentPenOf(animal, allowUnenclosedPens: false) != null)
		{
			return null;
		}
		if (!WorkGiver_InteractAnimal.CanInteractWithAnimal(roper, animal, out jobFailReason, forced, canInteractWhileSleeping: true, ignoreSkillRequirements: true, canInteractWhileRoaming: true))
		{
			return null;
		}
		Building building = null;
		foreach (Building allBuildingsHitchingPost in animal.Map.listerBuildings.allBuildingsHitchingPosts)
		{
			if (!allBuildingsHitchingPost.IsForbidden(roper) && roper.CanReach(allBuildingsHitchingPost, PathEndMode.Touch, Danger.Deadly) && animal.Map.reachability.CanReach(animal.Position, allBuildingsHitchingPost, PathEndMode.Touch, TraverseParms.For(roper).WithFenceblockedOf(animal)) && (building == null || (animal.Position - allBuildingsHitchingPost.Position).LengthManhattan < (animal.Position - building.Position).LengthManhattan))
			{
				building = allBuildingsHitchingPost;
			}
		}
		return building;
	}

	private static bool PenIsCloser(CompAnimalPenMarker markerA, CompAnimalPenMarker markerB, Pawn animal)
	{
		return animal.Position.DistanceToSquared(markerA.parent.Position) < animal.Position.DistanceToSquared(markerB.parent.Position);
	}

	public static IntVec3 RopeAttachmentInteractionCell(Pawn roper, Pawn ropee)
	{
		if (!roper.Spawned || !ropee.Spawned)
		{
			return IntVec3.Invalid;
		}
		if (IsGoodRopeAttachmentInteractionCell(roper, ropee, roper.Position))
		{
			return roper.Position;
		}
		Map map = ropee.Map;
		for (int i = 0; i < 4; i++)
		{
			IntVec3 intVec = ropee.Position + GenAdj.CardinalDirections[i];
			if (intVec.InBounds(map) && MutuallyWalkable(roper, ropee, intVec))
			{
				return intVec;
			}
		}
		return IntVec3.Invalid;
	}

	public static bool IsGoodRopeAttachmentInteractionCell(Pawn roper, Pawn ropee, IntVec3 cell)
	{
		if (ropee.Position.AdjacentToCardinal(cell))
		{
			return MutuallyWalkable(roper, ropee, cell);
		}
		return false;
	}

	private static bool MutuallyWalkable(Pawn roper, Pawn ropee, IntVec3 c)
	{
		Map map = ropee.Map;
		if (c.WalkableBy(map, ropee))
		{
			return c.WalkableBy(map, roper);
		}
		return false;
	}

	public static IntVec3 FindPlaceInPenToStand(CompAnimalPenMarker marker, Pawn pawn)
	{
		Map map = pawn.Map;
		IntVec3 result = IntVec3.Invalid;
		RegionTraverser.BreadthFirstTraverse(marker.parent.Position, map, (Region from, Region to) => marker.PenState.ContainsConnectedRegion(to), RegionProcessor);
		return result;
		bool GoodCell(IntVec3 cell)
		{
			if (cell.Standable(map))
			{
				return pawn.Map.pawnDestinationReservationManager.CanReserve(cell, pawn);
			}
			return false;
		}
		bool RegionProcessor(Region reg)
		{
			if (reg.TryFindRandomCellInRegion(GoodCell, out var result2))
			{
				result = result2;
				return true;
			}
			return false;
		}
	}

	public static bool IsUnnecessarilyRoped(Pawn animal)
	{
		if (!NeedsToBeManagedByRope(animal))
		{
			return false;
		}
		if (animal.roping.IsRopedByPawn || animal.roping.IsRopedToHitchingPost || !animal.roping.IsRopedToSpot)
		{
			return false;
		}
		return animal.GetLord()?.LordJob?.ManagesRopableAnimals != true;
	}
}
