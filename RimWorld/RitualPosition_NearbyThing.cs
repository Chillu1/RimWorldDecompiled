using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public abstract class RitualPosition_NearbyThing : RitualPosition
	{
		public int maxDistanceToFocus = 1;

		public abstract IEnumerable<Thing> CandidateThings(LordJob_Ritual ritual);

		public abstract IntVec3 PositionForThing(Thing t);

		public override PawnStagePosition GetCell(IntVec3 spot, Pawn p, LordJob_Ritual ritual)
		{
			foreach (Thing item in CandidateThings(ritual))
			{
				if (IsUsableThing(item, spot, ritual))
				{
					return new PawnStagePosition(PositionForThing(item), item, FacingDir(item), highlight);
				}
			}
			return new PawnStagePosition(IntVec3.Invalid, null, Rot4.Invalid, highlight);
		}

		public virtual bool IsUsableThing(Thing thing, IntVec3 spot, TargetInfo ritualTarget)
		{
			bool flag = false;
			bool flag2 = false;
			foreach (IntVec3 cell in (ritualTarget.HasThing ? ritualTarget.Thing.OccupiedRect() : CellRect.CenteredOn(spot, 0)).Cells)
			{
				if (thing.Position.InHorDistOf(cell, maxDistanceToFocus))
				{
					flag = true;
				}
				if (GenSight.LineOfSight(thing.Position, cell, ritualTarget.Map, skipFirstCell: true))
				{
					flag2 = true;
				}
			}
			return flag && flag2;
		}

		public bool IsUsableThing(Thing thing, IntVec3 spot, LordJob_Ritual ritual)
		{
			return IsUsableThing(thing, spot, ritual.selectedTarget);
		}

		public override bool CanUse(IntVec3 spot, Pawn p, LordJob_Ritual ritual)
		{
			return GetCell(spot, p, ritual).cell.IsValid;
		}

		protected virtual Rot4 FacingDir(Thing t)
		{
			return Rot4.Invalid;
		}
	}
}
