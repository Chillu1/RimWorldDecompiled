using RimWorld;

namespace Verse.AI.Group
{
	public class LordToil_DefendSelf : LordToil
	{
		public override void UpdateAllDuties()
		{
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				lord.ownedPawns[i].mindState.duty = new PawnDuty(DutyDefOf.Defend, lord.ownedPawns[i].Position);
				lord.ownedPawns[i].mindState.duty.radius = 28f;
			}
		}
	}
}
