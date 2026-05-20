using System.Collections.Generic;
using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_EnsureCanReachMapEdge : SymbolResolver
{
	private static HashSet<District> visited = new HashSet<District>();

	private static List<IntVec3> path = new List<IntVec3>();

	private static List<IntVec3> cellsInRandomOrder = new List<IntVec3>();

	public override void Resolve(ResolveParams rp)
	{
		cellsInRandomOrder.Clear();
		foreach (IntVec3 item in rp.rect)
		{
			cellsInRandomOrder.Add(item);
		}
		cellsInRandomOrder.Shuffle();
		TryMakeAllCellsReachable(canPathThroughNonStandable: false, rp);
		TryMakeAllCellsReachable(canPathThroughNonStandable: true, rp);
	}

	private void TryMakeAllCellsReachable(bool canPathThroughNonStandable, ResolveParams rp)
	{
		Map map = BaseGen.globalSettings.map;
		visited.Clear();
		for (int i = 0; i < cellsInRandomOrder.Count; i++)
		{
			IntVec3 intVec = cellsInRandomOrder[i];
			if (!CanTraverse(intVec, canPathThroughNonStandable))
			{
				continue;
			}
			District district = intVec.GetDistrict(map);
			if (district == null || visited.Contains(district))
			{
				continue;
			}
			visited.Add(district);
			TraverseParms traverseParms = TraverseParms.For(TraverseMode.PassDoors);
			if (map.reachability.CanReachMapEdge(intVec, traverseParms))
			{
				continue;
			}
			bool found = false;
			IntVec3 foundDest = IntVec3.Invalid;
			map.floodFiller.FloodFill(intVec, (IntVec3 x) => !found && CanTraverse(x, canPathThroughNonStandable), delegate(IntVec3 x)
			{
				if (!found && map.reachability.CanReachMapEdge(x, traverseParms))
				{
					found = true;
					foundDest = x;
				}
			}, int.MaxValue, rememberParents: true);
			if (found)
			{
				ReconstructPathAndDestroyWalls(foundDest, district, rp);
			}
		}
		visited.Clear();
	}

	private void ReconstructPathAndDestroyWalls(IntVec3 foundDest, District room, ResolveParams rp)
	{
		Map map = BaseGen.globalSettings.map;
		map.floodFiller.ReconstructLastFloodFillPath(foundDest, path);
		while (path.Count >= 2 && path[0].AdjacentToCardinal(room) && path[1].AdjacentToCardinal(room))
		{
			path.RemoveAt(0);
		}
		IntVec3 intVec = IntVec3.Invalid;
		ThingDef thingDef = null;
		IntVec3 intVec2 = IntVec3.Invalid;
		ThingDef thingDef2 = null;
		for (int i = 0; i < path.Count; i++)
		{
			Building edifice = path[i].GetEdifice(map);
			if (IsWallOrRock(edifice))
			{
				if (!intVec.IsValid)
				{
					intVec = path[i];
					thingDef = edifice.Stuff;
				}
				intVec2 = path[i];
				thingDef2 = edifice.Stuff;
				edifice.Destroy();
			}
		}
		if (intVec.IsValid)
		{
			ThingDef stuff = thingDef ?? rp.wallStuff ?? BaseGenUtility.RandomCheapWallStuff(rp.faction);
			Thing thing = ThingMaker.MakeThing(ThingDefOf.Door, stuff);
			thing.SetFaction(rp.faction);
			GenSpawn.Spawn(thing, intVec, map);
		}
		if (intVec2.IsValid && intVec2 != intVec && !intVec2.AdjacentToCardinal(intVec))
		{
			ThingDef stuff2 = thingDef2 ?? rp.wallStuff ?? BaseGenUtility.RandomCheapWallStuff(rp.faction);
			Thing thing2 = ThingMaker.MakeThing(ThingDefOf.Door, stuff2);
			thing2.SetFaction(rp.faction);
			GenSpawn.Spawn(thing2, intVec2, map);
		}
	}

	private bool CanTraverse(IntVec3 c, bool canPathThroughNonStandable)
	{
		Map map = BaseGen.globalSettings.map;
		Building edifice = c.GetEdifice(map);
		if (IsWallOrRock(edifice))
		{
			return true;
		}
		if (!canPathThroughNonStandable && (!c.Standable(map) || c.GetEdifice(map) != null))
		{
			return false;
		}
		if (!c.Impassable(map))
		{
			return true;
		}
		return false;
	}

	private bool IsWallOrRock(Building b)
	{
		if (b != null)
		{
			if (b.def != ThingDefOf.Wall)
			{
				return b.def.building.isNaturalRock;
			}
			return true;
		}
		return false;
	}
}
