using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RitualPosition_OnInteractionCell : RitualPosition_Cells
	{
		public IntVec3 offset = IntVec3.Zero;

		public override void FindCells(List<IntVec3> cells, Thing thing, CellRect rect, IntVec3 spot, Rot4 rotation)
		{
			cells.Add(thing.InteractionCell + offset);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref offset, "offset");
		}
	}
}
