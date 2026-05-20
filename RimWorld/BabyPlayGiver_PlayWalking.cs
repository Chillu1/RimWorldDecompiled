using Verse;
using Verse.AI;

namespace RimWorld
{
	public class BabyPlayGiver_PlayWalking : BabyPlayGiver
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
			IntVec3 intVec = JobDriver_PlayWalking.TryFindWanderCell(pawn, other.PositionHeld);
			if (!intVec.IsValid)
			{
				return null;
			}
			if (!pawn.CanReserveAndReach(intVec, PathEndMode.Touch, Danger.Some))
			{
				return null;
			}
			Job job = JobMaker.MakeJob(def.jobDef, other, intVec);
			job.count = 1;
			return job;
		}
	}
}
