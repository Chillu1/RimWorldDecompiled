using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_UnnaturalCorpseSkip : JobDriver
{
	private const TargetIndex DestIndex = TargetIndex.A;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return true;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		toil.initAction = delegate
		{
			SkipUtility.SkipTo(pawn, base.TargetA.Cell, pawn.MapHeld);
			pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
		};
		yield return toil;
	}
}
