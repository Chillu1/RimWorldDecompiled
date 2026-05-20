using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class StageEndTrigger_PawnDeliveredOrNotValid : StageEndTrigger
	{
		public override Trigger MakeTrigger(LordJob_Ritual ritual, TargetInfo spot, IEnumerable<TargetInfo> foci, RitualStage stage)
		{
			return new Trigger_TickCondition(delegate
			{
				foreach (TargetInfo focus in foci)
				{
					Pawn pawn = focus.Thing as Pawn;
					IntVec3 intVec = ((spot.Thing != null) ? spot.Thing.OccupiedRect().CenterCell : spot.Cell);
					if (!pawn.CanReachImmediate(intVec, PathEndMode.Touch) && !pawn.Dead)
					{
						return false;
					}
				}
				return true;
			});
		}

		public override void ExposeData()
		{
		}
	}
}
