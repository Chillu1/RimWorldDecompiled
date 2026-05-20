using System.Collections.Generic;

namespace Verse.AI
{
	public class JobDriver_CastAbilityGoTo : JobDriver_CastVerbOnce
	{
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			yield return Toils_Goto.Goto(TargetIndex.B, PathEndMode.OnCell);
			yield return Toils_Combat.CastVerb(TargetIndex.A, TargetIndex.B, canHitNonTargetPawns: false);
		}
	}
}
