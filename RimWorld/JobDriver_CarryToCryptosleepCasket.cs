using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_CarryToCryptosleepCasket : JobDriver
	{
		private const TargetIndex TakeeInd = TargetIndex.A;

		private const TargetIndex DropPodInd = TargetIndex.B;

		protected Pawn Takee => (Pawn)job.GetTarget(TargetIndex.A).Thing;

		protected Building_CryptosleepCasket DropPod => (Building_CryptosleepCasket)job.GetTarget(TargetIndex.B).Thing;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			if (pawn.Reserve(Takee, job, 1, -1, null, errorOnFailed))
			{
				return pawn.Reserve(DropPod, job, 1, -1, null, errorOnFailed);
			}
			return false;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDestroyedOrNull(TargetIndex.A);
			this.FailOnDestroyedOrNull(TargetIndex.B);
			this.FailOnAggroMentalState(TargetIndex.A);
			this.FailOn(() => !DropPod.Accepts(Takee));
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell).FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnDespawnedNullOrForbidden(TargetIndex.B)
				.FailOn(() => DropPod.GetDirectlyHeldThings().Count > 0)
				.FailOn(() => !Takee.Downed)
				.FailOn(() => !pawn.CanReach(Takee, PathEndMode.OnCell, Danger.Deadly))
				.FailOnSomeonePhysicallyInteracting(TargetIndex.A);
			yield return Toils_Haul.StartCarryThing(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.InteractionCell);
			Toil toil = Toils_General.Wait(500, TargetIndex.B);
			toil.FailOnCannotTouch(TargetIndex.B, PathEndMode.InteractionCell);
			toil.WithProgressBarToilDelay(TargetIndex.B);
			yield return toil;
			Toil toil2 = new Toil();
			toil2.initAction = delegate
			{
				DropPod.TryAcceptThing(Takee);
			};
			toil2.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return toil2;
		}

		public override object[] TaleParameters()
		{
			return new object[2]
			{
				pawn,
				Takee
			};
		}
	}
}
