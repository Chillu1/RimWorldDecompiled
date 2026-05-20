using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_InspectGravEngine : JobDriver
{
	private const TargetIndex GravEngineIndex = TargetIndex.A;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(base.TargetA, job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedOrNull(TargetIndex.A);
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
		yield return Toils_General.WaitWith(TargetIndex.A, 240, useProgressBar: true, maintainPosture: false, maintainSleep: false, TargetIndex.A);
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		toil.initAction = delegate
		{
			if (base.TargetA.Thing is Building_GravEngine building_GravEngine)
			{
				building_GravEngine.Inspect();
			}
		};
		yield return toil;
	}
}
