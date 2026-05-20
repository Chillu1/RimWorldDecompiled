using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class LordToil_BestowingCeremony_MoveInPlace : LordToil
{
	public bool? pass;

	public IntVec3 spot;

	public Pawn target;

	public LordToil_BestowingCeremony_MoveInPlace(IntVec3 spot, Pawn target)
	{
		this.spot = spot;
		this.target = target;
	}

	public override void UpdateAllDuties()
	{
		for (int i = 0; i < lord.ownedPawns.Count; i++)
		{
			PawnDuty pawnDuty = new PawnDuty(DutyDefOf.BestowingCeremony_MoveInPlace, spot);
			pawnDuty.locomotion = LocomotionUrgency.Walk;
			lord.ownedPawns[i].mindState.duty = pawnDuty;
		}
	}
}
