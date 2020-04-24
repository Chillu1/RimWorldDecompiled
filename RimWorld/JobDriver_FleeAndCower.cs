using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_FleeAndCower : JobDriver_Flee
	{
		private const int CowerTicks = 1200;

		private const int CheckFleeAgainIntervalTicks = 35;

		public override string GetReport()
		{
			if (pawn.CurJob != job || pawn.Position != job.GetTarget(TargetIndex.A).Cell)
			{
				return base.GetReport();
			}
			return "ReportCowering".Translate();
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			foreach (Toil item in base.MakeNewToils())
			{
				yield return item;
			}
			Toil toil = new Toil();
			toil.defaultCompleteMode = ToilCompleteMode.Delay;
			toil.defaultDuration = 1200;
			toil.tickAction = delegate
			{
				if (pawn.IsHashIntervalTick(35) && SelfDefenseUtility.ShouldStartFleeing(pawn))
				{
					EndJobWith(JobCondition.InterruptForced);
				}
			};
			yield return toil;
		}
	}
}
