using Verse;
using Verse.AI;

namespace RimWorld
{
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
				Log.Error(GetType() + " has null defendee. pawn=" + pawn.ToStringSafe());
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
				if (defendee.Spawned && !defendee.InMentalState && defendee.mindState.meleeThreat != null && defendee.mindState.meleeThreat != pawn && pawn.CanReach(defendee.mindState.meleeThreat, PathEndMode.Touch, Danger.Deadly))
				{
					return defendee.mindState.meleeThreat;
				}
			}
			return base.FindAttackTarget(pawn);
		}

		protected override bool TryFindShootingPosition(Pawn pawn, out IntVec3 dest)
		{
			Verb verb = pawn.TryGetAttackVerb(null, !pawn.IsColonist);
			if (verb == null)
			{
				dest = IntVec3.Invalid;
				return false;
			}
			CastPositionRequest newReq = default(CastPositionRequest);
			newReq.caster = pawn;
			newReq.target = pawn.mindState.enemyTarget;
			newReq.verb = verb;
			newReq.maxRangeFromTarget = 9999f;
			newReq.locus = GetDefendee(pawn).PositionHeld;
			newReq.maxRangeFromLocus = GetFlagRadius(pawn);
			newReq.wantCoverFromTarget = (verb.verbProps.range > 7f);
			newReq.maxRegions = 50;
			return CastPositionFinder.TryFindCastPosition(newReq, out dest);
		}
	}
}
