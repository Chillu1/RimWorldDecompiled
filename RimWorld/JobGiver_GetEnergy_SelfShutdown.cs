using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_GetEnergy_SelfShutdown : JobGiver_GetEnergy
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (!ShouldAutoRecharge(pawn))
			{
				return null;
			}
			if (RCellFinder.TryFindRandomMechSelfShutdownSpot(pawn.Position, pawn, pawn.Map, out var result))
			{
				Job job = JobMaker.MakeJob(JobDefOf.SelfShutdown, result);
				job.checkOverrideOnExpire = true;
				job.expiryInterval = 500;
				return job;
			}
			return null;
		}
	}
}
