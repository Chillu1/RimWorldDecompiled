using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_LayDown : JobDriver
	{
		public const TargetIndex BedOrRestSpotIndex = TargetIndex.A;

		public Building_Bed Bed => (Building_Bed)job.GetTarget(TargetIndex.A).Thing;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			if (job.GetTarget(TargetIndex.A).HasThing && !pawn.Reserve(Bed, job, Bed.SleepingSlotsCount, 0, null, errorOnFailed))
			{
				return false;
			}
			return true;
		}

		public override bool CanBeginNowWhileLyingDown()
		{
			return JobInBedUtility.InBedOrRestSpotNow(pawn, job.GetTarget(TargetIndex.A));
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			bool hasBed = job.GetTarget(TargetIndex.A).HasThing;
			if (hasBed)
			{
				yield return Toils_Bed.ClaimBedIfNonMedical(TargetIndex.A);
				yield return Toils_Bed.GotoBed(TargetIndex.A);
			}
			else
			{
				yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
			}
			yield return Toils_LayDown.LayDown(TargetIndex.A, hasBed, lookForOtherJobs: true);
		}

		public override string GetReport()
		{
			if (asleep)
			{
				return "ReportSleeping".Translate();
			}
			return "ReportResting".Translate();
		}
	}
}
