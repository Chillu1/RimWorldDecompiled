using System;
using Verse;

namespace RimWorld;

public abstract class RitualPosition_VerticalThingCenter : RitualPosition
{
	public IntVec3 offset;

	public override PawnStagePosition GetCell(IntVec3 spot, Pawn p, LordJob_Ritual ritual)
	{
		Thing thing = spot.GetThingList(p.Map).FirstOrDefault((Thing t) => t == ritual.selectedTarget.Thing);
		CellRect cellRect = thing?.OccupiedRect() ?? CellRect.SingleCell(spot);
		Map mapHeld = p.MapHeld;
		CellRect rect = GetRect(cellRect);
		Func<IntVec3, bool> func = CommonRitualCellPredicates.DefaultValidator(spot, mapHeld, p, cellRect);
		if (func(rect.CenterCell))
		{
			return new PawnStagePosition(rect.CenterCell, thing, Rot4.Invalid, highlight);
		}
		IntVec3 cell = IntVec3.Invalid;
		for (int num = 0; num < 16 && num < rect.Width; num++)
		{
			IntVec3 randomCell = rect.RandomCell;
			if (func(randomCell))
			{
				cell = randomCell;
				break;
			}
		}
		if (!cell.IsValid)
		{
			cell = GetFallbackSpot(cellRect, spot, p, ritual, func);
		}
		return new PawnStagePosition(cell, thing, Rot4.Invalid, highlight);
	}

	protected abstract CellRect GetRect(CellRect thingRect);
}
