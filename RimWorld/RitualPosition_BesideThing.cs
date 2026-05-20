using System;
using Verse;

namespace RimWorld;

public class RitualPosition_BesideThing : RitualPosition
{
	public bool faceThing;

	public override PawnStagePosition GetCell(IntVec3 spot, Pawn p, LordJob_Ritual ritual)
	{
		Thing thing = spot.GetThingList(p.Map).FirstOrDefault((Thing t) => t == ritual.selectedTarget.Thing);
		CellRect rect = thing?.OccupiedRect() ?? CellRect.CenteredOn(spot, 0);
		Map mapHeld = p.MapHeld;
		IntVec3 orig = ((thing != null) ? IntVec3.West.RotatedBy(thing.Rotation) : IntVec3.West);
		CellRect cellRect = new CellRect(rect.minX + orig.x, rect.minZ + orig.z, 1, rect.Height);
		CellRect cellRect2 = new CellRect(rect.maxX - orig.z, rect.minZ - orig.z, 1, rect.Height);
		Func<IntVec3, bool> func = CommonRitualCellPredicates.DefaultValidator(spot, mapHeld, p, rect);
		IntVec3 cell = IntVec3.Invalid;
		for (int num = 0; num < 16; num++)
		{
			IntVec3 intVec = ((!Rand.Chance(0.5f)) ? cellRect2.RandomCell : cellRect.RandomCell);
			if (func(intVec))
			{
				cell = intVec;
				break;
			}
		}
		if (!cell.IsValid)
		{
			cell = GetFallbackSpot(rect, spot, p, ritual, func);
		}
		return new PawnStagePosition(cell, thing, faceThing ? Rot4.FromIntVec3(orig.Inverse()) : Rot4.Invalid, highlight);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref faceThing, "faceThing", defaultValue: false);
	}
}
