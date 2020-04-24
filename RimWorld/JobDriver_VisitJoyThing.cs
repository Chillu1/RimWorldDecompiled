using System.Collections.Generic;
using Verse.AI;

namespace RimWorld
{
	public abstract class JobDriver_VisitJoyThing : JobDriver
	{
		protected const TargetIndex TargetThingIndex = TargetIndex.A;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.GetTarget(TargetIndex.A), job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDestroyedNullOrForbidden(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			Toil toil = Toils_General.Wait(job.def.joyDuration);
			toil.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			toil.tickAction = delegate
			{
				WaitTickAction();
			};
			toil.AddFinishAction(delegate
			{
				JoyUtility.TryGainRecRoomThought(pawn);
			});
			yield return toil;
		}

		protected abstract void WaitTickAction();
	}
}
