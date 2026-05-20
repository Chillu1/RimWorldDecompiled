using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_Ignite : JobDriver
{
	public const TargetIndex TargetInd = TargetIndex.A;

	public Thing TargetThing => job.GetTarget(TargetIndex.A).Thing;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return true;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedOrNull(TargetIndex.A);
		Toil toil = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnBurningImmobile(TargetIndex.A);
		if (job.ensureReachable)
		{
			toil.FailOnCannotReach(TargetIndex.A, PathEndMode.Touch);
		}
		yield return toil;
		Toil toil2 = ToilMaker.MakeToil("MakeNewToils");
		toil2.initAction = delegate
		{
			pawn.natives.TryStartIgnite(TargetThing);
		};
		yield return toil2;
	}
}
