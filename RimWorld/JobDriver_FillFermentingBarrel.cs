using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_FillFermentingBarrel : JobDriver
	{
		private const TargetIndex BarrelInd = TargetIndex.A;

		private const TargetIndex WortInd = TargetIndex.B;

		private const int Duration = 200;

		protected Building_FermentingBarrel Barrel => (Building_FermentingBarrel)job.GetTarget(TargetIndex.A).Thing;

		protected Thing Wort => job.GetTarget(TargetIndex.B).Thing;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			if (pawn.Reserve(Barrel, job, 1, -1, null, errorOnFailed))
			{
				return pawn.Reserve(Wort, job, 1, -1, null, errorOnFailed);
			}
			return false;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			this.FailOnBurningImmobile(TargetIndex.A);
			AddEndCondition(() => (Barrel.SpaceLeftForWort > 0) ? JobCondition.Ongoing : JobCondition.Succeeded);
			yield return Toils_General.DoAtomic(delegate
			{
				job.count = Barrel.SpaceLeftForWort;
			});
			Toil reserveWort = Toils_Reserve.Reserve(TargetIndex.B);
			yield return reserveWort;
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
			yield return Toils_Haul.StartCarryThing(TargetIndex.B, putRemainderInQueue: false, subtractNumTakenFromJobCount: true).FailOnDestroyedNullOrForbidden(TargetIndex.B);
			yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveWort, TargetIndex.B, TargetIndex.None, takeFromValidStorage: true);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			yield return Toils_General.Wait(200).FailOnDestroyedNullOrForbidden(TargetIndex.B).FailOnDestroyedNullOrForbidden(TargetIndex.A)
				.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch)
				.WithProgressBarToilDelay(TargetIndex.A);
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Barrel.AddWort(Wort);
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return toil;
		}
	}
}
