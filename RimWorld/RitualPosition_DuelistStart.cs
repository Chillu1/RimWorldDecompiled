using Verse;

namespace RimWorld
{
	public class RitualPosition_DuelistStart : RitualPosition
	{
		public int distFromTarget;

		public int duelistIndex;

		private static readonly Rot4[] Rotations = new Rot4[4]
		{
			Rot4.West,
			Rot4.East,
			Rot4.North,
			Rot4.South
		};

		public override PawnStagePosition GetCell(IntVec3 spot, Pawn p, LordJob_Ritual ritual)
		{
			int num = 0;
			for (int i = 0; i < Rotations.Length; i++)
			{
				Rot4 rot = Rotations[i];
				IntVec3 intVec = rot.FacingCell * distFromTarget;
				IntVec3 intVec2 = spot + intVec;
				if (intVec2.InBounds(p.Map) && intVec2.Standable(p.Map))
				{
					if (num == duelistIndex)
					{
						return new PawnStagePosition(intVec2, null, Rot4.FromIntVec3(rot.FacingCell), highlight);
					}
					num++;
				}
			}
			CellRect spectateRect = spot.GetThingList(p.Map).FirstOrDefault((Thing t) => t == ritual.selectedTarget.Thing)?.OccupiedRect() ?? CellRect.SingleCell(spot);
			if (SpectatorCellFinder.TryFindCircleSpectatorCellFor(p, spectateRect, 1f, distFromTarget * 2, p.Map, out var cell))
			{
				return new PawnStagePosition(cell, null, Rot4.Invalid, highlight);
			}
			return new PawnStagePosition(IntVec3.Invalid, null, Rot4.Invalid, highlight);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref distFromTarget, "distFromTarget", 0);
			Scribe_Values.Look(ref duelistIndex, "duelistIndex", 0);
		}
	}
}
