using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class CellSearchPattern : IExposable
	{
		public virtual void AddCellsToList(List<IntVec3> orderedCells, Thing thing, CellRect rect, IntVec3 focus, Rot4 focusRotation)
		{
			orderedCells.AddRange(rect.AdjacentCells);
		}

		public virtual void ExposeData()
		{
		}
	}
}
