using System.Collections.Generic;

namespace Verse;

public sealed class ThingGrid
{
	private Map map;

	private List<Thing>[] thingGrid;

	private static readonly List<Thing> EmptyThingList = new List<Thing>();

	public ThingGrid(Map map)
	{
		this.map = map;
		CellIndices cellIndices = map.cellIndices;
		thingGrid = new List<Thing>[cellIndices.NumGridCells];
		for (int i = 0; i < cellIndices.NumGridCells; i++)
		{
			thingGrid[i] = new List<Thing>(4);
		}
	}

	public void Register(Thing t)
	{
		if (t.def.size.x == 1 && t.def.size.z == 1)
		{
			RegisterInCell(t, t.Position);
			return;
		}
		CellRect cellRect = t.OccupiedRect();
		for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
		{
			for (int j = cellRect.minX; j <= cellRect.maxX; j++)
			{
				RegisterInCell(t, new IntVec3(j, 0, i));
			}
		}
	}

	private void RegisterInCell(Thing t, IntVec3 c)
	{
		if (!c.InBounds(map))
		{
			string obj = t?.ToString();
			IntVec3 intVec = c;
			Log.Warning(obj + " tried to register out of bounds at " + intVec.ToString() + ". Destroying.");
			t.Destroy();
		}
		else
		{
			thingGrid[map.cellIndices.CellToIndex(c)].Add(t);
		}
	}

	public void Deregister(Thing t, bool doEvenIfDespawned = false)
	{
		if (!t.Spawned && !doEvenIfDespawned)
		{
			return;
		}
		if (t.def.size.x == 1 && t.def.size.z == 1)
		{
			DeregisterInCell(t, t.Position);
			return;
		}
		CellRect cellRect = t.OccupiedRect();
		for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
		{
			for (int j = cellRect.minX; j <= cellRect.maxX; j++)
			{
				DeregisterInCell(t, new IntVec3(j, 0, i));
			}
		}
	}

	private void DeregisterInCell(Thing t, IntVec3 c)
	{
		if (!c.InBounds(map))
		{
			string obj = t?.ToString();
			IntVec3 intVec = c;
			Log.Error(obj + " tried to de-register out of bounds at " + intVec.ToString());
			return;
		}
		int num = map.cellIndices.CellToIndex(c);
		if (thingGrid[num].Contains(t))
		{
			thingGrid[num].Remove(t);
		}
	}

	public IEnumerable<Thing> ThingsAt(IntVec3 c)
	{
		if (c.InBounds(map))
		{
			List<Thing> list = thingGrid[map.cellIndices.CellToIndex(c)];
			for (int i = 0; i < list.Count; i++)
			{
				yield return list[i];
			}
		}
	}

	public List<Thing> ThingsListAt(IntVec3 c)
	{
		if (!c.InBounds(map))
		{
			IntVec3 intVec = c;
			Log.ErrorOnce("Got ThingsListAt out of bounds: " + intVec.ToString(), 495287);
			return EmptyThingList;
		}
		return thingGrid[map.cellIndices.CellToIndex(c)];
	}

	public List<Thing> ThingsListAtFast(IntVec3 c)
	{
		return thingGrid[map.cellIndices.CellToIndex(c)];
	}

	public List<Thing> ThingsListAtFast(int index)
	{
		return thingGrid[index];
	}

	public bool CellContains(IntVec3 c, ThingCategory cat)
	{
		return ThingAt(c, cat) != null;
	}

	public Thing ThingAt(IntVec3 c, ThingCategory cat)
	{
		if (!c.InBounds(map))
		{
			return null;
		}
		List<Thing> list = thingGrid[map.cellIndices.CellToIndex(c)];
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].def.category == cat)
			{
				return list[i];
			}
		}
		return null;
	}

	public bool CellContains(IntVec3 c, ThingDef def)
	{
		return ThingAt(c, def) != null;
	}

	public Thing ThingAt(IntVec3 c, ThingDef def)
	{
		if (!c.InBounds(map))
		{
			return null;
		}
		List<Thing> list = thingGrid[map.cellIndices.CellToIndex(c)];
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].def == def)
			{
				return list[i];
			}
		}
		return null;
	}

	public T ThingAt<T>(IntVec3 c) where T : Thing
	{
		if (!c.InBounds(map))
		{
			return null;
		}
		List<Thing> list = thingGrid[map.cellIndices.CellToIndex(c)];
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] is T result)
			{
				return result;
			}
		}
		return null;
	}
}
