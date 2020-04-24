using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_SitFacingBuilding : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			if (pawn.Reserve(job.targetA, job, job.def.joyMaxParticipants, 0, null, errorOnFailed))
			{
				return pawn.Reserve(job.targetB, job, 1, -1, null, errorOnFailed);
			}
			return false;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.EndOnDespawnedOrNull(TargetIndex.A);
			yield return Toils_Goto.Goto(TargetIndex.B, PathEndMode.OnCell);
			Toil toil = new Toil();
			toil.tickAction = delegate
			{
				base.pawn.rotationTracker.FaceTarget(base.TargetA);
				base.pawn.GainComfortFromCellIfPossible();
				JoyUtility.JoyTickCheckEnd(base.pawn, joySource: (Building)base.TargetThingA, fullJoyAction: job.doUntilGatheringEnded ? JoyTickFullJoyAction.None : JoyTickFullJoyAction.EndJob);
			};
			toil.handlingFacing = true;
			toil.defaultCompleteMode = ToilCompleteMode.Delay;
			toil.defaultDuration = (job.doUntilGatheringEnded ? job.expiryInterval : job.def.joyDuration);
			toil.AddFinishAction(delegate
			{
				JoyUtility.TryGainRecRoomThought(pawn);
			});
			ModifyPlayToil(toil);
			yield return toil;
		}

		protected virtual void ModifyPlayToil(Toil toil)
		{
		}

		public override object[] TaleParameters()
		{
			return new object[2]
			{
				pawn,
				base.TargetA.Thing.def
			};
		}
	}
}
