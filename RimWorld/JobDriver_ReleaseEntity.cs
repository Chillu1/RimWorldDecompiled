using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_ReleaseEntity : JobDriver
{
	private const TargetIndex PlatformIndex = TargetIndex.A;

	private const TargetIndex EntityIndex = TargetIndex.B;

	private const int TransferTicks = 300;

	private Thing Platform => base.TargetThingA;

	private Pawn InnerPawn => (Platform as Building_HoldingPlatform)?.HeldPawn;

	private bool EntityShouldBeReleased
	{
		get
		{
			CompHoldingPlatformTarget compHoldingPlatformTarget = InnerPawn?.TryGetComp<CompHoldingPlatformTarget>();
			if (compHoldingPlatformTarget != null)
			{
				if (compHoldingPlatformTarget.containmentMode != EntityContainmentMode.Release)
				{
					return job.ignoreDesignations;
				}
				return true;
			}
			return false;
		}
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(Platform, job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnSomeonePhysicallyInteracting(TargetIndex.A).FailOn(() => !EntityShouldBeReleased);
		Toil toil = Toils_General.WaitWhileExtractingContents(TargetIndex.A, TargetIndex.B, 300);
		toil.PlaySustainerOrSound(SoundDefOf.ReleaseFromPlatform);
		yield return toil;
		yield return Toils_General.Do(delegate
		{
			if (base.TargetThingB is Pawn thing)
			{
				CompHoldingPlatformTarget compHoldingPlatformTarget = thing.TryGetComp<CompHoldingPlatformTarget>();
				compHoldingPlatformTarget.Escape(initiator: false);
				if (compHoldingPlatformTarget != null)
				{
					compHoldingPlatformTarget.containmentMode = EntityContainmentMode.MaintainOnly;
				}
			}
			pawn.MentalState?.Notify_ReleasedTarget();
		});
	}
}
