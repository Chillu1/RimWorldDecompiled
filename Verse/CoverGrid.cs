using System.Collections.Generic;

namespace Verse
{
	public sealed class CoverGrid
	{
		private Map map;

		private Thing[] innerArray;

		public Thing this[int index] => innerArray[index];

		public Thing this[IntVec3 c] => innerArray[map.cellIndices.CellToIndex(c)];

		public CoverGrid(Map map)
		{
			this.map = map;
			innerArray = new Thing[map.cellIndices.NumGridCells];
		}

		public void Register(Thing t)
		{
			if (t.def.Fillage == FillCategory.None)
			{
				return;
			}
			CellRect cellRect = t.OccupiedRect();
			for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
			{
				for (int j = cellRect.minX; j <= cellRect.maxX; j++)
				{
					IntVec3 c = new IntVec3(j, 0, i);
					RecalculateCell(c);
				}
			}
		}

		public void DeRegister(Thing t)
		{
			if (t.def.Fillage == FillCategory.None)
			{
				return;
			}
			CellRect cellRect = t.OccupiedRect();
			for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
			{
				for (int j = cellRect.minX; j <= cellRect.maxX; j++)
				{
					IntVec3 c = new IntVec3(j, 0, i);
					RecalculateCell(c, t);
				}
			}
		}

		private void RecalculateCell(IntVec3 c, Thing ignoreThing = null)
		{
			Thing thing = null;
			float num = 0.001f;
			List<Thing> list = map.thingGrid.ThingsListAtFast(c);
			for (int i = 0; i < list.Count; i++)
			{
				Thing thing2 = list[i];
				if (thing2 != ignoreThing && !thing2.Destroyed && thing2.Spawned && thing2.def.fillPercent > num)
				{
					thing = thing2;
					num = thing2.def.fillPercent;
				}
			}
			innerArray[map.cellIndices.CellToIndex(c)] = thing;
		}
	}
}
