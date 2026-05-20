using System.Collections.Generic;
using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_OutdoorsPath : SymbolResolver
{
	private static readonly List<IntVec3> cellsInRandomOrder = new List<IntVec3>();

	private static readonly List<IntVec3> path = new List<IntVec3>();

	private static readonly SimpleCurve ChanceToSkipPathOverDistanceCurve = new SimpleCurve
	{
		new CurvePoint(10f, 0f),
		new CurvePoint(20f, 0f),
		new CurvePoint(50f, 1f)
	};

	public override bool CanResolve(ResolveParams rp)
	{
		return base.CanResolve(rp);
	}

	public override void Resolve(ResolveParams rp)
	{
		cellsInRandomOrder.Clear();
		cellsInRandomOrder.AddRange(rp.rect.Cells);
		cellsInRandomOrder.Shuffle();
		Map map = BaseGen.globalSettings.map;
		for (int i = 0; i < cellsInRandomOrder.Count; i++)
		{
			IntVec3 intVec = cellsInRandomOrder[i];
			if (intVec.GetDoor(BaseGen.globalSettings.map) == null || !map.reachability.CanReachMapEdge(intVec, TraverseParms.For(TraverseMode.NoPassClosedDoorsOrWater)))
			{
				continue;
			}
			bool found = false;
			IntVec3 foundDest = IntVec3.Invalid;
			map.floodFiller.FloodFill(intVec, (IntVec3 x) => !found && CanTraverse(x), delegate(IntVec3 x)
			{
				if (x.OnEdge(map))
				{
					found = true;
					foundDest = x;
				}
			}, int.MaxValue, rememberParents: true);
			if (!found)
			{
				continue;
			}
			path.Clear();
			map.floodFiller.ReconstructLastFloodFillPath(foundDest, path);
			for (int num = 0; num < path.Count; num++)
			{
				IntVec3 intVec2 = path[num];
				if (Rand.Chance(ChanceToSkipPathOverDistanceCurve.Evaluate(intVec.DistanceTo(intVec2))) || !CanPlacePath(intVec2))
				{
					continue;
				}
				List<Thing> thingList = intVec2.GetThingList(map);
				for (int num2 = thingList.Count - 1; num2 >= 0; num2--)
				{
					if (thingList[num2].def.destroyable)
					{
						thingList[num2].Destroy();
					}
				}
				map.terrainGrid.SetTerrain(intVec2, rp.floorDef ?? TerrainDefOf.Gravel);
			}
			break;
		}
		path.Clear();
		cellsInRandomOrder.Clear();
	}

	private bool CanTraverse(IntVec3 c)
	{
		Map map = BaseGen.globalSettings.map;
		if (c.GetDoor(map) == null)
		{
			Room room = c.GetRoom(map);
			if (room != null && !room.PsychologicallyOutdoors)
			{
				return false;
			}
		}
		Building edifice = c.GetEdifice(map);
		if (IsWallOrRock(edifice))
		{
			return false;
		}
		return true;
	}

	private bool CanPlacePath(IntVec3 c)
	{
		Map map = BaseGen.globalSettings.map;
		if (c.GetDoor(map) != null || c.GetTerrain(map).IsWater)
		{
			return false;
		}
		return true;
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
