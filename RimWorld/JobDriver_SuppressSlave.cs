using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_SuppressSlave : JobDriver
	{
		private int SuppressDuration = 180;

		protected Pawn Slave => (Pawn)job.targetA.Thing;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			if (ModLister.CheckIdeology("Suppress slave"))
			{
				this.FailOnDespawnedOrNull(TargetIndex.A);
				this.FailOnMentalState(TargetIndex.A);
				this.FailOnNotAwake(TargetIndex.A);
				this.FailOnForbidden(TargetIndex.A);
				this.FailOn(() => !Slave.IsSlaveOfColony);
				yield return Toils_Interpersonal.GotoSlave(pawn, Slave);
				yield return Toils_Interpersonal.WaitToBeAbleToInteract(pawn);
				yield return Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A);
				yield return SetLastSuppressionTime(TargetIndex.A);
				yield return TrySuppress(TargetIndex.A);
			}
		}

		private Toil TrySuppress(TargetIndex slaveInd)
		{
			Toil toil = ToilMaker.MakeToil("TrySuppress");
			toil.initAction = delegate
			{
				pawn.interactions.TryInteractWith(Slave, InteractionDefOf.Suppress);
				PawnUtility.ForceWait(Slave, SuppressDuration, pawn);
			};
			toil.socialMode = RandomSocialMode.Off;
			toil.defaultCompleteMode = ToilCompleteMode.Delay;
			toil.defaultDuration = SuppressDuration;
			return toil;
		}

		private Toil SetLastSuppressionTime(TargetIndex targetInd)
		{
			Toil toil = ToilMaker.MakeToil("SetLastSuppressionTime");
			toil.initAction = delegate
			{
				((Pawn)toil.actor.jobs.curJob.GetTarget(targetInd).Thing).mindState.lastSlaveSuppressedTick = Find.TickManager.TicksGame;
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			return toil;
		}
	}
}
