using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public abstract class LordToil_DoOpportunisticTaskOrCover : LordToil
	{
		public bool cover = true;

		public override bool AllowSatisfyLongNeeds => false;

		protected abstract DutyDef DutyDef
		{
			get;
		}

		protected abstract bool TryFindGoodOpportunisticTaskTarget(Pawn pawn, out Thing target, List<Thing> alreadyTakenTargets);

		public override void UpdateAllDuties()
		{
			List<Thing> list = null;
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				Pawn pawn = lord.ownedPawns[i];
				Thing target = null;
				if (!cover || (TryFindGoodOpportunisticTaskTarget(pawn, out target, list) && !GenAI.InDangerousCombat(pawn)))
				{
					if (pawn.mindState.duty == null || pawn.mindState.duty.def != DutyDef)
					{
						pawn.mindState.duty = new PawnDuty(DutyDef);
						pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
					}
					if (list == null)
					{
						list = new List<Thing>();
					}
					list.Add(target);
				}
				else
				{
					pawn.mindState.duty = new PawnDuty(DutyDefOf.AssaultColony);
				}
			}
		}

		public override void LordToilTick()
		{
			if (!cover || Find.TickManager.TicksGame % 181 != 0)
			{
				return;
			}
			List<Thing> list = null;
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				Pawn pawn = lord.ownedPawns[i];
				if (pawn.Downed || pawn.mindState.duty.def != DutyDefOf.AssaultColony)
				{
					continue;
				}
				Thing target = null;
				if (TryFindGoodOpportunisticTaskTarget(pawn, out target, list) && !base.Map.reservationManager.IsReservedByAnyoneOf(target, lord.faction) && !GenAI.InDangerousCombat(pawn))
				{
					pawn.mindState.duty = new PawnDuty(DutyDef);
					pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
					if (list == null)
					{
						list = new List<Thing>();
					}
					list.Add(target);
				}
			}
		}
	}
}
