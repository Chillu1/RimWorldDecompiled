using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_CarryToEntityHolderAlreadyHolding : JobDriver
{
	private const TargetIndex DestIndex = TargetIndex.A;

	private const TargetIndex TakeeIndex = TargetIndex.B;

	private Thing Takee => job.GetTarget(TargetIndex.B).Thing;

	private CompEntityHolder DestHolder => job.GetTarget(TargetIndex.A).Thing.TryGetComp<CompEntityHolder>();

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(DestHolder.parent, job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDestroyedOrNull(TargetIndex.B);
		this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
		this.FailOn(() => !DestHolder.Available);
		yield return Toils_Haul.StartCarryThing(TargetIndex.B);
		foreach (Toil item in JobDriver_CarryToEntityHolder.ChainTakeeToPlatformToils(pawn, Takee, DestHolder, TargetIndex.A))
		{
			yield return item;
		}
	}
}
