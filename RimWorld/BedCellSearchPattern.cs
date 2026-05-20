using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public abstract class BedCellSearchPattern : CellSearchPattern
	{
		public abstract void BedCellOffsets(List<IntVec3> offsets, IntVec2 size, int slot);

		public override void AddCellsToList(List<IntVec3> orderedCells, Thing thing, CellRect rect, IntVec3 focus, Rot4 focusRotation)
		{
			if (!rect.Contains(focus))
			{
				throw new ArgumentException();
			}
			int slotFromPosition = BedUtility.GetSlotFromPosition(focus, thing.Position, focusRotation, thing.def.size);
			BedCellOffsets(orderedCells, thing.def.size, slotFromPosition);
			RotationDirection relativeRotation = Rot4.GetRelativeRotation(Rot4.South, focusRotation);
			for (int i = 0; i < orderedCells.Count; i++)
			{
				orderedCells[i] = focus + orderedCells[i].RotatedBy(relativeRotation);
			}
		}
	}
}
