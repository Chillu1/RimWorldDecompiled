using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class LordToil_EscortPawn : LordToil
{
	public Pawn escortee;

	public float followRadius = 7f;

	public LordToil_EscortPawn(Pawn escortee, float followRadius = 7f)
	{
		this.escortee = escortee;
		this.followRadius = followRadius;
	}

	public override void UpdateAllDuties()
	{
		for (int i = 0; i < lord.ownedPawns.Count; i++)
		{
			PawnDuty duty = new PawnDuty(DutyDefOf.Escort, escortee, followRadius);
			lord.ownedPawns[i].mindState.duty = duty;
		}
	}
}
