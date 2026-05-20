using System.Collections.Generic;

namespace Verse.AI
{
	public class JobDriver_CastAbilityTouch : JobDriver_CastVerbOnce
	{
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			this.FailOnCannotTouch(TargetIndex.B, PathEndMode.OnCell);
			yield return Toils_Goto.Goto(TargetIndex.A, PathEndMode.Touch);
			yield return Toils_Combat.CastVerb(TargetIndex.A, TargetIndex.B, canHitNonTargetPawns: false);
		}
	}
}
