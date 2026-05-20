using Verse;
using Verse.AI;

namespace RimWorld
{
	public class BabyPlayGiver_PlayStatic : BabyPlayGiver
	{
		public override bool CanDo(Pawn pawn, Pawn other)
		{
			if (!pawn.IsCarryingPawn(other) && !pawn.CanReserveAndReach(other, PathEndMode.Touch, Danger.Some))
			{
				return false;
			}
			return true;
		}

		public override Job TryGiveJob(Pawn pawn, Pawn other)
		{
			Job job = JobMaker.MakeJob(def.jobDef, other);
			job.count = 1;
			return job;
		}
	}
}
