using System.Collections.Generic;
using Verse;

namespace RimWorld;

public static class FilthMaker
{
	private static readonly List<Filth> toBeRemoved = new List<Filth>();

	public static bool CanMakeFilth(IntVec3 cell, Map map, ThingDef filthDef, FilthSourceFlags additionalFlags = FilthSourceFlags.None)
	{
		TerrainGrid terrainGrid = map.terrainGrid;
		foreach (IntVec3 item in GenAdj.OccupiedRect(cell, Rot4.North, filthDef.size))
		{
			if (!item.InBounds(map))
			{
				return false;
			}
			TerrainDef terrainDef = terrainGrid.FoundationAt(item) ?? terrainGrid.TerrainAt(item);
			if (!filthDef.filth.ignoreFilthMultiplierStat && (filthDef.filth.placementMask & FilthSourceFlags.Natural) == 0 && Rand.Value > terrainDef.GetStatValueAbstract(StatDefOf.FilthMultiplier))
			{
				return false;
			}
			FilthSourceFlags filthSourceFlags = filthDef.filth.placementMask | additionalFlags;
			if (terrainDef.filthAcceptanceMask != FilthSourceFlags.None && filthSourceFlags.HasFlag(FilthSourceFlags.Pawn))
			{
				if (item.GetRoof(map) != null)
				{
					return true;
				}
				Room room = item.GetRoom(map);
				if (room != null && !room.TouchesMapEdge && !room.UsesOutdoorTemperature)
				{
					return true;
				}
			}
			if (!TerrainAcceptsFilth(terrainDef, filthDef, additionalFlags))
			{
				return false;
			}
		}
		return true;
	}

	public static bool TerrainAcceptsFilth(TerrainDef terrainDef, ThingDef filthDef, FilthSourceFlags additionalFlags = FilthSourceFlags.None)
	{
		if (terrainDef.filthAcceptanceMask == FilthSourceFlags.None)
		{
			return false;
		}
		FilthSourceFlags filthSourceFlags = filthDef.filth.placementMask | additionalFlags;
		return (terrainDef.filthAcceptanceMask & filthSourceFlags) == filthSourceFlags;
	}

	public static bool TryMakeFilth(IntVec3 c, Map map, ThingDef filthDef, int count = 1, FilthSourceFlags additionalFlags = FilthSourceFlags.None, bool shouldPropagate = true)
	{
		bool flag = false;
		for (int i = 0; i < count; i++)
		{
			flag |= TryMakeFilth(c, map, filthDef, null, shouldPropagate, out var _, additionalFlags);
		}
		return flag;
	}

	public static bool TryMakeFilth(IntVec3 c, Map map, ThingDef filthDef, out Filth outFilth, int count = 1, FilthSourceFlags additionalFlags = FilthSourceFlags.None, bool shouldPropagate = true)
	{
		outFilth = null;
		bool flag = false;
		for (int i = 0; i < count; i++)
		{
			flag |= TryMakeFilth(c, map, filthDef, null, shouldPropagate, out outFilth, additionalFlags);
		}
		return flag;
	}

	public static bool TryMakeFilth(IntVec3 c, Map map, ThingDef filthDef, string source, int count = 1, FilthSourceFlags additionalFlags = FilthSourceFlags.None)
	{
		bool flag = false;
		for (int i = 0; i < count; i++)
		{
			flag |= TryMakeFilth(c, map, filthDef, Gen.YieldSingle(source), shouldPropagate: true, out var _, additionalFlags);
		}
		return flag;
	}

	public static bool TryMakeFilth(IntVec3 c, Map map, ThingDef filthDef, IEnumerable<string> sources, FilthSourceFlags additionalFlags = FilthSourceFlags.None)
	{
		Filth outFilth;
		return TryMakeFilth(c, map, filthDef, sources, shouldPropagate: true, out outFilth, additionalFlags);
	}

	public static bool TryMakeFilth(IntVec3 c, Map map, ThingDef filthDef, out Filth outFilth, string source, FilthSourceFlags additionalFlags = FilthSourceFlags.None, bool shouldPropagate = true)
	{
		outFilth = null;
		return TryMakeFilth(c, map, filthDef, Gen.YieldSingle(source), shouldPropagate, out outFilth, additionalFlags);
	}

	private static bool TryMakeFilth(IntVec3 c, Map map, ThingDef filthDef, IEnumerable<string> sources, bool shouldPropagate, out Filth outFilth, FilthSourceFlags additionalFlags = FilthSourceFlags.None)
	{
		outFilth = (Filth)c.GetThingList(map).FirstOrDefault((Thing t) => t.def == filthDef);
		if (c.GetTerrain(map).exposesToVacuum)
		{
			return false;
		}
		if (!c.WalkableByAny(map) || (outFilth != null && !outFilth.CanBeThickened))
		{
			if (shouldPropagate)
			{
				List<IntVec3> list = GenAdj.AdjacentCells8WayRandomized();
				for (int num = 0; num < 8; num++)
				{
					IntVec3 c2 = c + list[num];
					if (c2.InBounds(map) && TryMakeFilth(c2, map, filthDef, sources, shouldPropagate: false, out outFilth))
					{
						return true;
					}
				}
			}
			if (outFilth != null)
			{
				outFilth.AddSources(sources);
			}
			return false;
		}
		if (outFilth != null)
		{
			outFilth.ThickenFilth();
			outFilth.AddSources(sources);
		}
		else
		{
			if (!CanMakeFilth(c, map, filthDef, additionalFlags))
			{
				return false;
			}
			outFilth = (Filth)ThingMaker.MakeThing(filthDef);
			outFilth.AddSources(sources);
			GenSpawn.Spawn(outFilth, c, map);
		}
		FilthMonitor.Notify_FilthSpawned();
		return true;
	}

	public static void RemoveAllFilth(IntVec3 c, Map map)
	{
		toBeRemoved.Clear();
		List<Thing> thingList = c.GetThingList(map);
		for (int i = 0; i < thingList.Count; i++)
		{
			if (thingList[i] is Filth item)
			{
				toBeRemoved.Add(item);
			}
		}
		for (int j = 0; j < toBeRemoved.Count; j++)
		{
			toBeRemoved[j].Destroy();
		}
		toBeRemoved.Clear();
	}
}
