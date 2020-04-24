using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToil_PrepareCaravan_GatherItems : LordToil
	{
		private IntVec3 meetingPoint;

		public override float? CustomWakeThreshold => 0.5f;

		public override bool AllowRestingInBed => false;

		public LordToil_PrepareCaravan_GatherItems(IntVec3 meetingPoint)
		{
			this.meetingPoint = meetingPoint;
		}

		public override void UpdateAllDuties()
		{
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				Pawn pawn = lord.ownedPawns[i];
				if (pawn.IsColonist)
				{
					pawn.mindState.duty = new PawnDuty(DutyDefOf.PrepareCaravan_GatherItems);
				}
				else if (pawn.RaceProps.Animal)
				{
					pawn.mindState.duty = new PawnDuty(DutyDefOf.PrepareCaravan_Wait, meetingPoint);
				}
				else
				{
					pawn.mindState.duty = new PawnDuty(DutyDefOf.PrepareCaravan_Wait);
				}
			}
		}

		public override void LordToilTick()
		{
			base.LordToilTick();
			if (Find.TickManager.TicksGame % 120 != 0)
			{
				return;
			}
			bool flag = true;
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				Pawn pawn = lord.ownedPawns[i];
				if (pawn.IsColonist && pawn.mindState.lastJobTag != JobTag.WaitingForOthersToFinishGatheringItems)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				List<Pawn> allPawnsSpawned = base.Map.mapPawns.AllPawnsSpawned;
				for (int j = 0; j < allPawnsSpawned.Count; j++)
				{
					if (allPawnsSpawned[j].CurJob != null && allPawnsSpawned[j].jobs.curDriver is JobDriver_PrepareCaravan_GatherItems && allPawnsSpawned[j].CurJob.lord == lord)
					{
						flag = false;
						break;
					}
				}
			}
			if (flag)
			{
				lord.ReceiveMemo("AllItemsGathered");
			}
		}
	}
}
