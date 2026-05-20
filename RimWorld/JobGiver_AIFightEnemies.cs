using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_AIFightEnemies : JobGiver_AIFightEnemy
{
	protected override bool TryFindShootingPosition(Pawn pawn, out IntVec3 dest, Verb verbToUse = null)
	{
		Thing enemyTarget = pawn.mindState.enemyTarget;
		bool allowManualCastWeapons = !pawn.IsColonist && !pawn.IsColonySubhuman;
		Verb verb = verbToUse ?? pawn.TryGetAttackVerb(enemyTarget, allowManualCastWeapons, allowTurrets);
		if (verb == null)
		{
			dest = IntVec3.Invalid;
			return false;
		}
		return CastPositionFinder.TryFindCastPosition(new CastPositionRequest
		{
			caster = pawn,
			target = enemyTarget,
			verb = verb,
			maxRangeFromTarget = verb.EffectiveRange,
			wantCoverFromTarget = (verb.EffectiveRange > 5f)
		}, out dest);
	}
}
