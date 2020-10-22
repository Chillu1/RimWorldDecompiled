using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToil_Wait : LordToil
	{
		public override void UpdateAllDuties()
		{
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				PawnDuty duty = new PawnDuty(DutyDefOf.Idle);
				DecoratePawnDuty(duty);
				lord.ownedPawns[i].mindState.duty = duty;
			}
		}

		protected virtual void DecoratePawnDuty(PawnDuty duty)
		{
		}
	}
}
