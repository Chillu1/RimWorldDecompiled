using Verse;

namespace RimWorld
{
	public class RitualPosition_AnimaLinkingSpot : RitualPosition
	{
		public override PawnStagePosition GetCell(IntVec3 spot, Pawn p, LordJob_Ritual ritual)
		{
			if (SpectatorCellFinder.TryFindCircleSpectatorCellFor(p, CellRect.CenteredOn(spot, 0), 2f, 3f, p.Map, out var cell))
			{
				return new PawnStagePosition(cell, null, Rot4.FromAngleFlat((spot - cell).AngleFlat), highlight);
			}
			CompPsylinkable compPsylinkable = ritual.selectedTarget.Thing?.TryGetComp<CompPsylinkable>();
			if (compPsylinkable != null && compPsylinkable.TryFindLinkSpot(p, out var spot2))
			{
				Rot4 orientation = Rot4.FromAngleFlat((spot - spot2.Cell).AngleFlat);
				return new PawnStagePosition(spot2.Cell, null, orientation, highlight);
			}
			return new PawnStagePosition(IntVec3.Invalid, null, Rot4.Invalid, highlight);
		}
	}
}
