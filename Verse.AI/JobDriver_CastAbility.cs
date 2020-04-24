using System.Collections.Generic;

namespace Verse.AI
{
	public class JobDriver_CastAbility : JobDriver_CastVerbOnce
	{
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				pawn.pather.StopDead();
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return toil;
			yield return Toils_Combat.CastVerb(TargetIndex.A, TargetIndex.B, canHitNonTargetPawns: false);
		}
	}
}
