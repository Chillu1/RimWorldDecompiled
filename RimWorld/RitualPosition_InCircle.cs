using Verse;
using Verse.AI;

namespace RimWorld
{
	public class RitualPosition_InCircle : RitualPosition
	{
		public Rot4? preferredRotation;

		public IntRange distRange;

		public override PawnStagePosition GetCell(IntVec3 spot, Pawn p, LordJob_Ritual ritual)
		{
			if (preferredRotation.HasValue)
			{
				for (int num = distRange.max; num >= distRange.min; num--)
				{
					IntVec3 intVec = spot + preferredRotation.Value.FacingCell * num;
					if (intVec.InBounds(p.Map) && intVec.Standable(p.Map) && GenSight.LineOfSight(spot, intVec, ritual.Map, skipFirstCell: true) && WanderUtility.InSameRoom(intVec, spot, ritual.Map))
					{
						return new PawnStagePosition(intVec, null, preferredRotation.Value.Opposite, highlight);
					}
				}
			}
			CellRect spectateRect = spot.GetThingList(p.Map).FirstOrDefault((Thing t) => t == ritual.selectedTarget.Thing)?.OccupiedRect() ?? CellRect.SingleCell(spot);
			if (SpectatorCellFinder.TryFindCircleSpectatorCellFor(p, spectateRect, distRange.min, distRange.max, p.Map, out var cell))
			{
				return new PawnStagePosition(cell, null, Rot4.Invalid, highlight);
			}
			return new PawnStagePosition(IntVec3.Invalid, null, Rot4.Invalid, highlight);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref distRange, "distRange");
			Scribe_Values.Look(ref preferredRotation, "preferredRotation");
		}
	}
}
