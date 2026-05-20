using Verse;
using Verse.AI;

namespace RimWorld;

public abstract class JobGiver_AIDefendPawn : JobGiver_AIFightEnemy
{
	private bool attackMeleeThreatEvenIfNotHostile;

	public override ThinkNode DeepCopy(bool resolve = true)
	{
		JobGiver_AIDefendPawn obj = (JobGiver_AIDefendPawn)base.DeepCopy(resolve);
		obj.attackMeleeThreatEvenIfNotHostile = attackMeleeThreatEvenIfNotHostile;
		return obj;
	}

	protected abstract Pawn GetDefendee(Pawn pawn);

	protected override IntVec3 GetFlagPosition(Pawn pawn)
	{
		Pawn defendee = GetDefendee(pawn);
		if (defendee.Spawned || defendee.CarriedBy != null)
		{
			return defendee.PositionHeld;
		}
		return IntVec3.Invalid;
	}

	protected override Job TryGiveJob(Pawn pawn)
	{
		Pawn defendee = GetDefendee(pawn);
		if (defendee == null)
		{
			Log.Error(GetType()?.ToString() + " has null defendee. pawn=" + pawn.ToStringSafe());
			return null;
		}
		Pawn carriedBy = defendee.CarriedBy;
		if (carriedBy != null)
		{
			if (!pawn.CanReach(carriedBy, PathEndMode.OnCell, Danger.Deadly))
			{
				return null;
			}
		}
		else if (!defendee.Spawned || !pawn.CanReach(defendee, PathEndMode.OnCell, Danger.Deadly))
		{
			return null;
		}
		return base.TryGiveJob(pawn);
	}

	protected override Thing FindAttackTarget(Pawn pawn)
	{
		if (attackMeleeThreatEvenIfNotHostile)
		{
			Pawn defendee = GetDefendee(pawn);
			if (defendee.Spawned && !defendee.InMentalState && defendee.mindState.meleeThreat != null && defendee.mindState.meleeThreat != pawn && defendee.mindState.MeleeThreatStillThreat && pawn.CanReach(defendee.mindState.meleeThreat, PathEndMode.Touch, Danger.Deadly))
			{
				return defendee.mindState.meleeThreat;
			}
		}
		return base.FindAttackTarget(pawn);
	}

	protected override bool TryFindShootingPosition(Pawn pawn, out IntVec3 dest, Verb verbToUse = null)
	{
		Verb verb = verbToUse ?? pawn.TryGetAttackVerb(null, !pawn.IsColonist);
		if (verb == null)
		{
			dest = IntVec3.Invalid;
			return false;
		}
		return CastPositionFinder.TryFindCastPosition(new CastPositionRequest
		{
			caster = pawn,
			target = pawn.mindState.enemyTarget,
			verb = verb,
			maxRangeFromTarget = 9999f,
			locus = GetDefendee(pawn).PositionHeld,
			maxRangeFromLocus = GetFlagRadius(pawn),
			wantCoverFromTarget = (verb.EffectiveRange > 7f),
			maxRegions = 50
		}, out dest);
	}
}
