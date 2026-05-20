using Verse;

namespace RimWorld
{
	public class RitualPosition_ClaimedBed : RitualPosition
	{
		[NoTranslate]
		public string ofPawn;

		public bool tryClaimIfNoneClaimed;

		public override PawnStagePosition GetCell(IntVec3 spot, Pawn p, LordJob_Ritual ritual)
		{
			Pawn pawn = ((ofPawn == null) ? p : ritual.assignments.FirstAssignedPawn(ofPawn));
			if (pawn.ownership != null && pawn.ownership.OwnedBed != null)
			{
				return new PawnStagePosition(pawn.ownership.OwnedBed.Position, null, Rot4.Invalid, highlight);
			}
			if (tryClaimIfNoneClaimed)
			{
				Building_Bed building_Bed = RestUtility.FindBedFor(pawn);
				if (building_Bed != null)
				{
					pawn.ownership.ClaimBedIfNonMedical(building_Bed);
					return new PawnStagePosition(building_Bed.Position, null, Rot4.Invalid, highlight);
				}
			}
			return new PawnStagePosition(IntVec3.Invalid, null, Rot4.Invalid, highlight);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref ofPawn, "ofPawn");
			Scribe_Values.Look(ref tryClaimIfNoneClaimed, "tryClaimIfNoneClaimed", defaultValue: false);
		}
	}
}
