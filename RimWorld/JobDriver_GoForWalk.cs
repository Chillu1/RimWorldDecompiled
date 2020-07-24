using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_GoForWalk : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			JobDriver_GoForWalk jobDriver_GoForWalk = this;
			this.FailOn(() => !JoyUtility.EnjoyableOutsideNow(jobDriver_GoForWalk.pawn));
			Toil goToil = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
			goToil.tickAction = delegate
			{
				if (Find.TickManager.TicksGame > jobDriver_GoForWalk.startTick + jobDriver_GoForWalk.job.def.joyDuration)
				{
					jobDriver_GoForWalk.EndJobWith(JobCondition.Succeeded);
				}
				else
				{
					JoyUtility.JoyTickCheckEnd(jobDriver_GoForWalk.pawn);
				}
			};
			yield return goToil;
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				if (jobDriver_GoForWalk.job.targetQueueA.Count > 0)
				{
					LocalTargetInfo targetA = jobDriver_GoForWalk.job.targetQueueA[0];
					jobDriver_GoForWalk.job.targetQueueA.RemoveAt(0);
					jobDriver_GoForWalk.job.targetA = targetA;
					jobDriver_GoForWalk.JumpToToil(goToil);
				}
			};
			yield return toil;
		}
	}
}
