using System.Collections.Generic;

namespace Verse
{
	public class LinkGrid
	{
		private Map map;

		private LinkFlags[] linkGrid;

		public LinkGrid(Map map)
		{
			this.map = map;
			linkGrid = new LinkFlags[map.cellIndices.NumGridCells];
		}

		public LinkFlags LinkFlagsAt(IntVec3 c)
		{
			return linkGrid[map.cellIndices.CellToIndex(c)];
		}

		public void Notify_LinkerCreatedOrDestroyed(Thing linker)
		{
			CellIndices cellIndices = map.cellIndices;
			foreach (IntVec3 item in linker.OccupiedRect())
			{
				LinkFlags linkFlags = LinkFlags.None;
				List<Thing> list = map.thingGrid.ThingsListAt(item);
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].def.graphicData != null)
					{
						linkFlags |= list[i].def.graphicData.linkFlags;
					}
				}
				linkGrid[cellIndices.CellToIndex(item)] = linkFlags;
			}
		}
	}
}
