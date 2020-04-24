using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class JobInBedUtility
	{
		public static void KeepLyingDown(this JobDriver driver, TargetIndex bedIndex)
		{
			driver.AddFinishAction(delegate
			{
				Pawn pawn = driver.pawn;
				if (!pawn.Drafted)
				{
					pawn.jobs.jobQueue.EnqueueFirst(JobMaker.MakeJob(JobDefOf.LayDown, pawn.CurJob.GetTarget(bedIndex)));
				}
			});
		}

		public static bool InBedOrRestSpotNow(Pawn pawn, LocalTargetInfo bedOrRestSpot)
		{
			if (!bedOrRestSpot.IsValid || !pawn.Spawned)
			{
				return false;
			}
			if (bedOrRestSpot.HasThing)
			{
				if (bedOrRestSpot.Thing.Map != pawn.Map)
				{
					return false;
				}
				return RestUtility.GetBedSleepingSlotPosFor(pawn, (Building_Bed)bedOrRestSpot.Thing) == pawn.Position;
			}
			return bedOrRestSpot.Cell == pawn.Position;
		}
	}
}
