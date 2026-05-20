using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class LordToil_WanderNearby : LordToil
{
	private IntVec3? cell;

	public override void UpdateAllDuties()
	{
		if (!cell.HasValue && lord.ownedPawns.Any())
		{
			cell = lord.ownedPawns[0].Position;
		}
		if (cell.HasValue)
		{
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				PawnDuty duty = new PawnDuty(DutyDefOf.WanderClose, cell.Value);
				lord.ownedPawns[i].mindState.duty = duty;
			}
		}
	}
}
