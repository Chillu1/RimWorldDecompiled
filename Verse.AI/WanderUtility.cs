using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;

namespace Verse.AI;

public static class WanderUtility
{
	private static List<IntVec3> gatherSpots = new List<IntVec3>();

	private static List<IntVec3> candidateCells = new List<IntVec3>();

	private static List<Building> candidateBuildingsInRandomOrder = new List<Building>();

	public static IntVec3 BestCloseWanderRoot(IntVec3 trueWanderRoot, Pawn pawn)
	{
		for (int i = 0; i < 50; i++)
		{
			IntVec3 intVec = ((i >= 8) ? (trueWanderRoot + GenRadial.RadialPattern[i - 8 + 1] * 7) : (trueWanderRoot + GenRadial.RadialPattern[i]));
			if (intVec.InBounds(pawn.Map) && intVec.WalkableBy(pawn.Map, pawn) && pawn.CanReach(intVec, PathEndMode.OnCell, Danger.Some))
			{
				return intVec;
			}
		}
		return IntVec3.Invalid;
	}

	public static bool InSameRoom(IntVec3 locA, IntVec3 locB, Map map)
	{
		Room room = locA.GetRoom(map);
		if (room == null)
		{
			return true;
		}
		return room == locB.GetRoom(map);
	}

	public static IntVec3 GetColonyWanderRoot(Pawn pawn)
	{
		if (pawn.RaceProps.Humanlike && !pawn.IsSubhuman)
		{
			gatherSpots.Clear();
			for (int i = 0; i < pawn.Map.gatherSpotLister.activeSpots.Count; i++)
			{
				IntVec3 position = pawn.Map.gatherSpotLister.activeSpots[i].parent.Position;
				if (!position.IsForbidden(pawn) && pawn.CanReach(position, PathEndMode.Touch, Danger.None))
				{
					gatherSpots.Add(position);
				}
			}
			if (gatherSpots.Count > 0)
			{
				return gatherSpots.RandomElement();
			}
		}
		candidateCells.Clear();
		candidateBuildingsInRandomOrder.Clear();
		candidateBuildingsInRandomOrder.AddRange(pawn.Map.listerBuildings.allBuildingsColonist);
		candidateBuildingsInRandomOrder.Shuffle();
		int num = 0;
		int num2 = 0;
		while (num2 < candidateBuildingsInRandomOrder.Count)
		{
			if (num > 80 && candidateCells.Count > 0)
			{
				return candidateCells.RandomElement();
			}
			Building building = candidateBuildingsInRandomOrder[num2];
			if ((building.def == ThingDefOf.Wall || building.def.building.ai_chillDestination) && !building.Position.IsForbidden(pawn) && pawn.Map.areaManager.Home[building.Position])
			{
				IntVec3 intVec = GenAdjFast.AdjacentCells8Way(building).RandomElement();
				if (intVec.Standable(building.Map) && !intVec.IsForbidden(pawn) && pawn.CanReach(intVec, PathEndMode.OnCell, Danger.None) && !intVec.IsInPrisonCell(pawn.Map))
				{
					candidateCells.Add(intVec);
					if ((pawn.Position - building.Position).LengthHorizontalSquared <= 1225)
					{
						return intVec;
					}
				}
			}
			num2++;
			num++;
		}
		if (pawn.Map.mapPawns.FreeColonistsSpawned.Where((Pawn c) => !c.Position.IsForbidden(pawn) && pawn.CanReach(c.Position, PathEndMode.Touch, Danger.None)).TryRandomElement(out var result))
		{
			return result.Position;
		}
		return pawn.Position;
	}

	public static IntVec3 GetHerdWanderRoot(Pawn pawn, Predicate<Thing> isHerdValidator)
	{
		Predicate<Thing> validator = delegate(Thing t)
		{
			if (t.Position.IsForbidden(pawn))
			{
				return false;
			}
			if (!pawn.CanReach(t, PathEndMode.OnCell, Danger.Deadly))
			{
				return false;
			}
			return !(Rand.Value < 0.5f) && isHerdValidator(t);
		};
		return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.OnCell, TraverseParms.For(pawn), 35f, validator, null, 13)?.Position ?? pawn.Position;
	}
}
