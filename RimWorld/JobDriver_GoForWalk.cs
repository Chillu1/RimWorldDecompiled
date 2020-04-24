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
			this.FailOn(() => !JoyUtility.EnjoyableOutsideNow(pawn));
			Toil goToil = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
			goToil.tickAction = delegate
			{
				if (Find.TickManager.TicksGame > startTick + job.def.joyDuration)
				{
					EndJobWith(JobCondition.Succeeded);
				}
				else
				{
					JoyUtility.JoyTickCheckEnd(pawn);
				}
			};
			yield return goToil;
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				if (job.targetQueueA.Count > 0)
				{
					LocalTargetInfo targetA = job.targetQueueA[0];
					job.targetQueueA.RemoveAt(0);
					job.targetA = targetA;
					JumpToToil(goToil);
				}
			};
			yield return toil;
		}
	}
}
