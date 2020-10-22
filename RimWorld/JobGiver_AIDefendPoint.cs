using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_AIDefendPoint : JobGiver_AIFightEnemy
	{
		protected override bool TryFindShootingPosition(Pawn pawn, out IntVec3 dest)
		{
			Thing enemyTarget = pawn.mindState.enemyTarget;
			Verb verb = pawn.TryGetAttackVerb(enemyTarget, !pawn.IsColonist);
			if (verb == null)
			{
				dest = IntVec3.Invalid;
				return false;
			}
			CastPositionRequest newReq = default(CastPositionRequest);
			newReq.caster = pawn;
			newReq.target = enemyTarget;
			newReq.verb = verb;
			newReq.maxRangeFromTarget = 9999f;
			newReq.locus = (IntVec3)pawn.mindState.duty.focus;
			newReq.maxRangeFromLocus = pawn.mindState.duty.radius;
			newReq.wantCoverFromTarget = verb.verbProps.range > 7f;
			return CastPositionFinder.TryFindCastPosition(newReq, out dest);
		}
	}
}
