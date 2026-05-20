using RimWorld;

namespace Verse.AI.Group
{
	public class LordToil_ExitMapRandom : LordToil
	{
		public override void UpdateAllDuties()
		{
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				PawnDuty duty = new PawnDuty(DutyDefOf.ExitMapRandom);
				lord.ownedPawns[i].mindState.duty = duty;
			}
		}
	}
}
