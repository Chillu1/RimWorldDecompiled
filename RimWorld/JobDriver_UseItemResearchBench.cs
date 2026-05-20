using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_UseItemResearchBench : JobDriver_UseItem
{
	private const TargetIndex Item = TargetIndex.A;

	private const TargetIndex ResearchBenchInd = TargetIndex.B;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		if (base.TryMakePreToilReservations(errorOnFailed))
		{
			return pawn.Reserve(job.targetB, job, 1, -1, null, errorOnFailed);
		}
		return false;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnIncapable(PawnCapacityDefOf.Manipulation);
		this.FailOn(() => !base.TargetThingA.TryGetComp<CompUsable>().CanBeUsedBy(pawn));
		this.FailOnDespawnedNullOrForbidden(TargetIndex.B);
		this.FailOnBurningImmobile(TargetIndex.B);
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.A).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
		yield return Toils_Haul.StartCarryThing(TargetIndex.A).FailOnDestroyedNullOrForbidden(TargetIndex.A);
		yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.InteractionCell);
		yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.B, null, storageMode: false);
		yield return PrepareToUse();
		yield return Use();
	}
}
