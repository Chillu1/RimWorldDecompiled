using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_UseCommsConsole : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.InteractionCell).FailOn((Toil to) => !((Building_CommsConsole)to.actor.jobs.curJob.GetTarget(TargetIndex.A).Thing).CanUseCommsNow);
			Toil openComms = new Toil();
			openComms.initAction = delegate
			{
				Pawn actor = openComms.actor;
				if (((Building_CommsConsole)actor.jobs.curJob.GetTarget(TargetIndex.A).Thing).CanUseCommsNow)
				{
					actor.jobs.curJob.commTarget.TryOpenComms(actor);
				}
			};
			yield return openComms;
		}
	}
}
