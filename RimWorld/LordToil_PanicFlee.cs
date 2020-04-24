using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToil_PanicFlee : LordToil
	{
		public override bool AllowSatisfyLongNeeds => false;

		public override bool AllowSelfTend => false;

		public override void Init()
		{
			base.Init();
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				Pawn pawn = lord.ownedPawns[i];
				if (!HasFleeingDuty(pawn) || pawn.mindState.duty.def == DutyDefOf.ExitMapRandom)
				{
					pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.PanicFlee);
				}
			}
		}

		public override void UpdateAllDuties()
		{
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				Pawn pawn = lord.ownedPawns[i];
				if (!HasFleeingDuty(pawn))
				{
					pawn.mindState.duty = new PawnDuty(DutyDefOf.ExitMapRandom);
				}
			}
		}

		private bool HasFleeingDuty(Pawn pawn)
		{
			if (pawn.mindState.duty == null)
			{
				return false;
			}
			if (pawn.mindState.duty.def == DutyDefOf.ExitMapRandom || pawn.mindState.duty.def == DutyDefOf.Steal || pawn.mindState.duty.def == DutyDefOf.Kidnap)
			{
				return true;
			}
			return false;
		}
	}
}
