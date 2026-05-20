using System.Collections.Generic;

namespace Verse;

public class PersistentDangerSource : SimpleBoolPathFinderDataSource
{
	public PersistentDangerSource(Map map)
		: base(map)
	{
	}

	public override void ComputeAll(IEnumerable<PathRequest> _)
	{
		data.Clear();
		for (int i = 0; i < cellCount; i++)
		{
			IntVec3 c = map.cellIndices.IndexToCell(i);
			TerrainDef terrain = c.GetTerrain(map);
			List<Thing> thingList = c.GetThingList(map);
			bool flag = false;
			for (int j = 0; j < thingList.Count; j++)
			{
				if (thingList[j].def.pathfinderDangerous && !(thingList[j] is Pawn))
				{
					flag = true;
					break;
				}
			}
			data.Set(i, terrain.dangerous || flag);
		}
	}

	public override bool UpdateIncrementally(IEnumerable<PathRequest> requests, List<IntVec3> cellDeltas)
	{
		foreach (IntVec3 cellDelta in cellDeltas)
		{
			int pos = map.cellIndices.CellToIndex(cellDelta);
			List<Thing> thingList = cellDelta.GetThingList(map);
			bool flag = false;
			for (int i = 0; i < thingList.Count; i++)
			{
				if (thingList[i].def.pathfinderDangerous && !(thingList[i] is Pawn))
				{
					flag = true;
					break;
				}
			}
			data.Set(pos, flag || cellDelta.GetTerrain(map).dangerous);
		}
		return false;
	}
}
