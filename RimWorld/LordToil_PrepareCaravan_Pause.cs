using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToil_PrepareCaravan_Pause : LordToil
	{
		public override void UpdateAllDuties()
		{
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				lord.ownedPawns[i].mindState.duty = new PawnDuty(DutyDefOf.PrepareCaravan_Pause);
			}
		}
	}
}
