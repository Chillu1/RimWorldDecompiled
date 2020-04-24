using System.Collections.Generic;

namespace Verse
{
	public class RoofCollapseBuffer
	{
		private List<IntVec3> cellsToCollapse = new List<IntVec3>();

		public List<IntVec3> CellsMarkedToCollapse => cellsToCollapse;

		public bool IsMarkedToCollapse(IntVec3 c)
		{
			return cellsToCollapse.Contains(c);
		}

		public void MarkToCollapse(IntVec3 c)
		{
			if (!cellsToCollapse.Contains(c))
			{
				cellsToCollapse.Add(c);
			}
		}

		public void Clear()
		{
			cellsToCollapse.Clear();
		}
	}
}
