using Verse;

namespace RimWorld;

public class JobGiver_MetalhorrorFight : JobGiver_AIFightEnemy
{
	public JobGiver_MetalhorrorFight()
	{
		targetAcquireRadius = 29.9f;
		targetKeepRadius = targetAcquireRadius * 2f;
	}

	protected override Thing FindAttackTarget(Pawn pawn)
	{
		return MetalhorrorUtility.FindTarget(pawn);
	}

	protected override bool TryFindShootingPosition(Pawn pawn, out IntVec3 dest, Verb verbToUse = null)
	{
		dest = IntVec3.Invalid;
		return false;
	}
}
