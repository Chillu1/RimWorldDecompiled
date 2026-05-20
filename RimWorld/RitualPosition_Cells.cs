using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld;

public abstract class RitualPosition_Cells : RitualPosition
{
	public Rot4 facing = Rot4.Invalid;

	public bool faceThing;

	private static List<IntVec3> tmpPotentialCells = new List<IntVec3>(8);

	public abstract void FindCells(List<IntVec3> cells, Thing thing, CellRect rect, IntVec3 spot, Rot4 rotation);

	public override PawnStagePosition GetCell(IntVec3 spot, Pawn p, LordJob_Ritual ritual)
	{
		tmpPotentialCells.Clear();
		Thing thing = spot.GetThingList(ritual.Map).FirstOrDefault((Thing t) => t == ritual.selectedTarget.Thing);
		Map mapHeld = p.MapHeld;
		CellRect rect = thing?.OccupiedRect() ?? CellRect.CenteredOn(spot, 0);
		FindCells(tmpPotentialCells, thing, rect, spot, thing?.Rotation ?? Rot4.South);
		CommonRitualCellPredicates.RemoveLeastDesirableRitualCells(tmpPotentialCells, spot, mapHeld, p, rect);
		Func<IntVec3, bool> validator = CommonRitualCellPredicates.DefaultValidator(spot, mapHeld, p, rect);
		IntVec3 intVec = ((tmpPotentialCells.Count == 0) ? GetFallbackSpot(rect, spot, p, ritual, validator) : tmpPotentialCells[0]);
		if (!intVec.IsValid)
		{
			return null;
		}
		Rot4 orientation;
		if (faceThing)
		{
			if (facing != Rot4.Invalid)
			{
				Log.Error("Only one of faceThing and facing should be specified.");
			}
			orientation = Rot4.FromAngleFlat((thing.Position - intVec).AngleFlat);
		}
		else
		{
			orientation = facing;
		}
		return new PawnStagePosition(intVec, thing, orientation, highlight);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref facing, "facing");
		Scribe_Values.Look(ref faceThing, "faceThing", defaultValue: false);
	}
}
