using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld
{
	public class JobDriver_PlayBilliards : JobDriver
	{
		private const int ShotDuration = 600;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.targetA, job, job.def.joyMaxParticipants, 0, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.EndOnDespawnedOrNull(TargetIndex.A);
			Toil chooseCell = Toils_Misc.FindRandomAdjacentReachableCell(TargetIndex.A, TargetIndex.B);
			yield return chooseCell;
			yield return Toils_Reserve.Reserve(TargetIndex.B);
			yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				job.locomotionUrgency = LocomotionUrgency.Walk;
			};
			toil.tickAction = delegate
			{
				pawn.rotationTracker.FaceCell(base.TargetA.Thing.OccupiedRect().ClosestCellTo(pawn.Position));
				if (ticksLeftThisToil == 300)
				{
					SoundDefOf.PlayBilliards.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
				}
				if (Find.TickManager.TicksGame > startTick + job.def.joyDuration)
				{
					EndJobWith(JobCondition.Succeeded);
				}
				else
				{
					JoyUtility.JoyTickCheckEnd(pawn, JoyTickFullJoyAction.EndJob, 1f, (Building)base.TargetThingA);
				}
			};
			toil.handlingFacing = true;
			toil.socialMode = RandomSocialMode.SuperActive;
			toil.defaultCompleteMode = ToilCompleteMode.Delay;
			toil.defaultDuration = 600;
			toil.AddFinishAction(delegate
			{
				JoyUtility.TryGainRecRoomThought(pawn);
			});
			yield return toil;
			yield return Toils_Reserve.Release(TargetIndex.B);
			yield return Toils_Jump.Jump(chooseCell);
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
