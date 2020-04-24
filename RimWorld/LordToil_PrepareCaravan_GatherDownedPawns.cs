using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToil_PrepareCaravan_GatherDownedPawns : LordToil
	{
		private IntVec3 meetingPoint;

		private IntVec3 exitSpot;

		public override float? CustomWakeThreshold => 0.5f;

		public override bool AllowRestingInBed => false;

		public LordToil_PrepareCaravan_GatherDownedPawns(IntVec3 meetingPoint, IntVec3 exitSpot)
		{
			this.meetingPoint = meetingPoint;
			this.exitSpot = exitSpot;
		}

		public override void UpdateAllDuties()
		{
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				Pawn pawn = lord.ownedPawns[i];
				if (pawn.IsColonist)
				{
					pawn.mindState.duty = new PawnDuty(DutyDefOf.PrepareCaravan_GatherDownedPawns, meetingPoint, exitSpot);
				}
				else
				{
					pawn.mindState.duty = new PawnDuty(DutyDefOf.PrepareCaravan_Wait, meetingPoint);
				}
			}
		}

		public override void LordToilTick()
		{
			if (Find.TickManager.TicksGame % 100 != 0)
			{
				return;
			}
			bool flag = true;
			List<Pawn> downedPawns = ((LordJob_FormAndSendCaravan)lord.LordJob).downedPawns;
			for (int i = 0; i < downedPawns.Count; i++)
			{
				if (!JobGiver_PrepareCaravan_GatherDownedPawns.IsDownedPawnNearExitPoint(downedPawns[i], exitSpot))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				lord.ReceiveMemo("AllDownedPawnsGathered");
			}
		}
	}
}
