using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class LordToil_WanderClose : LordToil
{
	private IntVec3 location;

	public LordToil_WanderClose(IntVec3 location)
	{
		this.location = location;
	}

	public override void UpdateAllDuties()
	{
		for (int i = 0; i < lord.ownedPawns.Count; i++)
		{
			PawnDuty duty = new PawnDuty(DutyDefOf.WanderClose, location);
			lord.ownedPawns[i].mindState.duty = duty;
		}
	}
}
