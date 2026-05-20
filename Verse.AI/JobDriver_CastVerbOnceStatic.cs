using System.Collections.Generic;

namespace Verse.AI;

public class JobDriver_CastVerbOnceStatic : JobDriver_CastVerbOnce
{
	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedOrNull(TargetIndex.A);
		yield return Toils_General.StopDead();
		yield return Toils_Combat.CastVerb(TargetIndex.A);
	}
}
