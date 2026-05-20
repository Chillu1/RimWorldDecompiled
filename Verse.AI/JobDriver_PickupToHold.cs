using System.Collections.Generic;
using RimWorld;

namespace Verse.AI
{
	public class JobDriver_PickupToHold : JobDriver
	{
		private const TargetIndex HeldItemInd = TargetIndex.A;

		public static bool TryMakePreToilReservations(JobDriver driver, bool errorOnFailed)
		{
			driver.pawn.Map.pawnDestinationReservationManager.Reserve(driver.pawn, driver.job, driver.job.GetTarget(TargetIndex.A).Cell);
			return driver.pawn.Reserve(driver.job.GetTarget(TargetIndex.A), driver.job, 1, -1, null, errorOnFailed);
		}

		public static IEnumerable<Toil> Toils(JobDriver driver, TargetIndex HeldItem = TargetIndex.A, bool subtractNumTakenFromJobCount = true)
		{
			driver.FailOn(() => !driver.GetActor().health.capacities.CapableOf(PawnCapacityDefOf.Manipulation));
			driver.FailOnDestroyedOrNull(HeldItem);
			driver.FailOnForbidden(HeldItem);
			Toil end = Toils_General.Label();
			yield return Toils_Jump.JumpIf(end, () => driver.GetActor().IsCarryingThing(driver.GetActor().CurJob.GetTarget(HeldItem).Thing));
			yield return Toils_Goto.GotoThing(HeldItem, PathEndMode.ClosestTouch).FailOnSomeonePhysicallyInteracting(HeldItem);
			yield return Toils_Haul.StartCarryThing(HeldItem, putRemainderInQueue: false, subtractNumTakenFromJobCount);
			yield return end;
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return TryMakePreToilReservations(this, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			return Toils(this);
		}
	}
}
