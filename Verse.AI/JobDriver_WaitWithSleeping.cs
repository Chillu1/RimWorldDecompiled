using System.Collections.Generic;
using RimWorld;

namespace Verse.AI;

public class JobDriver_WaitWithSleeping : JobDriver_Wait
{
	public override void DecorateWaitToil(Toil wait)
	{
		wait.FailOn(() => !(base.TargetThingB is Pawn { Dead: false } pawn) || pawn.Awake());
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
		foreach (Toil item in base.MakeNewToils())
		{
			yield return item;
		}
	}
}
