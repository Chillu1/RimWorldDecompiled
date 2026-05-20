using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_TransferBetweenEntityHolders : JobDriver
{
	private const TargetIndex SourceHolderIndex = TargetIndex.A;

	private const TargetIndex DestHolderIndex = TargetIndex.B;

	private const TargetIndex TakeeIndex = TargetIndex.C;

	private Thing Takee => job.GetTarget(TargetIndex.C).Thing;

	private CompEntityHolder SourceHolder => job.GetTarget(TargetIndex.A).Thing.TryGetComp<CompEntityHolder>();

	private CompEntityHolder DestHolder => job.GetTarget(TargetIndex.B).Thing.TryGetComp<CompEntityHolder>();

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		if (pawn.Reserve(Takee, job, 1, -1, null, errorOnFailed) && pawn.Reserve(SourceHolder.parent, job, 1, -1, null, errorOnFailed))
		{
			return pawn.Reserve(DestHolder.parent, job, 1, -1, null, errorOnFailed);
		}
		return false;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDestroyedOrNull(TargetIndex.C);
		this.FailOnDespawnedNullOrForbidden(TargetIndex.B);
		this.FailOn(() => !DestHolder.Available);
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.A);
		yield return Toils_General.Do(delegate
		{
			Takee.TryGetComp<CompHoldingPlatformTarget>()?.Notify_ReleasedFromPlatform();
			SourceHolder.EjectContents();
			pawn.Map.reservationManager.Release(job.GetTarget(TargetIndex.A), pawn, job);
		}).FailOnDespawnedNullOrForbidden(TargetIndex.A);
		yield return Toils_Haul.StartCarryThing(TargetIndex.C);
		yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch);
		foreach (Toil item in JobDriver_CarryToEntityHolder.ChainTakeeToPlatformToils(pawn, Takee, DestHolder, TargetIndex.B))
		{
			yield return item;
		}
	}
}
