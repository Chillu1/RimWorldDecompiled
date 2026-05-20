using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_Seal : JobDriver
{
	private const TargetIndex HatchInd = TargetIndex.A;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(job.GetTarget(TargetIndex.A), job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
		yield return Toils_General.WaitWith(TargetIndex.A, 300, useProgressBar: true);
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		toil.initAction = delegate
		{
			if (job.GetTarget(TargetIndex.A).Thing.TryGetComp(out CompSealable comp))
			{
				comp.Seal();
			}
		};
		yield return toil;
	}
}
