using RimWorld;

namespace Verse.AI.Group
{
	public class LordToil_ExitMapTraderFighting : LordToil
	{
		public override bool AllowSatisfyLongNeeds => false;

		public override bool AllowSelfTend => false;

		public override void UpdateAllDuties()
		{
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				Pawn pawn = lord.ownedPawns[i];
				TraderCaravanRole traderCaravanRole = pawn.GetTraderCaravanRole();
				if (traderCaravanRole == TraderCaravanRole.Carrier || traderCaravanRole == TraderCaravanRole.Chattel)
				{
					pawn.mindState.duty = new PawnDuty(DutyDefOf.ExitMapBest);
					pawn.mindState.duty.locomotion = LocomotionUrgency.Jog;
				}
				else
				{
					pawn.mindState.duty = new PawnDuty(DutyDefOf.ExitMapBestAndDefendSelf);
				}
			}
		}
	}
}
