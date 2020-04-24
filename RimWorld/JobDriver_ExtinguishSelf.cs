using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_ExtinguishSelf : JobDriver
	{
		protected Fire TargetFire => (Fire)job.targetA.Thing;

		public override string GetReport()
		{
			if (TargetFire != null && TargetFire.parent != null)
			{
				return "ReportExtinguishingFireOn".Translate(TargetFire.parent.LabelCap, TargetFire.parent.Named("TARGET"));
			}
			return "ReportExtinguishingFire".Translate();
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			Toil toil = new Toil();
			toil.defaultCompleteMode = ToilCompleteMode.Delay;
			toil.defaultDuration = 150;
			yield return toil;
			Toil toil2 = new Toil();
			toil2.initAction = delegate
			{
				TargetFire.Destroy();
				pawn.records.Increment(RecordDefOf.FiresExtinguished);
			};
			toil2.FailOnDestroyedOrNull(TargetIndex.A);
			toil2.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return toil2;
		}
	}
}
