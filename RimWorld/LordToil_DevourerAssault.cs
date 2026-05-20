using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class LordToil_DevourerAssault : LordToil
{
	public override void UpdateAllDuties()
	{
		foreach (Pawn ownedPawn in lord.ownedPawns)
		{
			ownedPawn.mindState.duty = new PawnDuty(DutyDefOf.DevourerAssault);
		}
	}
}
