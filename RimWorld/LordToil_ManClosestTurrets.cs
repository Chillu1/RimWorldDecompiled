using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToil_ManClosestTurrets : LordToil
	{
		public override void UpdateAllDuties()
		{
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				lord.ownedPawns[i].mindState.duty = new PawnDuty(DutyDefOf.ManClosestTurret, lord.ownedPawns[i].Position);
			}
		}
	}
}
