using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class LordToil_WanderNest : LordToil
{
	public override void UpdateAllDuties()
	{
		for (int i = 0; i < lord.ownedPawns.Count; i++)
		{
			PawnDuty duty = new PawnDuty(DutyDefOf.WanderNest);
			lord.ownedPawns[i].mindState.duty = duty;
		}
	}
}
