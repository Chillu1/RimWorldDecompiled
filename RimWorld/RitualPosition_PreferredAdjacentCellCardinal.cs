using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RitualPosition_PreferredAdjacentCellCardinal : RitualPosition_Cells
	{
		public CellSearchPattern preferredCellSearchPattern;

		public override void FindCells(List<IntVec3> cells, Thing thing, CellRect rect, IntVec3 spot, Rot4 rotation)
		{
			if (preferredCellSearchPattern != null)
			{
				preferredCellSearchPattern.AddCellsToList(cells, thing, rect, spot, thing?.Rotation ?? Rot4.South);
			}
			else
			{
				cells.AddRange(rect.AdjacentCellsCardinal);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref preferredCellSearchPattern, "preferredCellSearchPattern");
		}
	}
}
