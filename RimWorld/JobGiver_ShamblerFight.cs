using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_ShamblerFight : JobGiver_AIFightEnemy
{
	private static readonly IntRange CheckForBetterTargetInterval = new IntRange(240, 540);

	private static readonly PathFinderCostTuning tuning = new PathFinderCostTuning
	{
		costBlockedDoor = 75,
		costBlockedWallBase = 75,
		costBlockedWallExtraForNaturalWalls = 0,
		costBlockedDoorPerHitPoint = 0f,
		costBlockedWallExtraPerHitPoint = 0f
	};

	protected override IntRange ExpiryInterval_Melee => CheckForBetterTargetInterval;

	protected override int TicksSinceEngageToLoseTarget => 4200;

	protected override bool DisableAbilityVerbs => true;

	public JobGiver_ShamblerFight()
	{
		targetAcquireRadius = 20f;
		targetKeepRadius = 40f;
	}

	protected override Job TryGiveJob(Pawn pawn)
	{
		if (pawn.mindState.enemyTarget == null)
		{
			return null;
		}
		return base.TryGiveJob(pawn);
	}

	protected override void UpdateEnemyTarget(Pawn pawn)
	{
		Thing enemyTarget = pawn.mindState.enemyTarget;
		if (enemyTarget != null)
		{
			Thing thing = FindAttackTargetIfPossible(pawn);
			if (thing != null && pawn.Position.DistanceTo(thing.Position) < pawn.Position.DistanceTo(enemyTarget.Position) / 2f)
			{
				pawn.mindState.enemyTarget = thing;
				pawn.mindState.Notify_EngagedTarget();
			}
			if (ShouldLoseTarget(pawn))
			{
				pawn.mindState.enemyTarget = thing;
			}
			if (thing is Pawn pawn2 && thing.Faction == Faction.OfPlayer && pawn.Position.InHorDistOf(thing.Position, 40f) && !pawn2.IsShambler && !pawn.IsPsychologicallyInvisible())
			{
				Find.TickManager.slower.SignalForceNormalSpeed();
			}
		}
	}

	protected override Thing FindAttackTarget(Pawn pawn)
	{
		return MutantUtility.FindShamblerTarget(pawn);
	}

	protected override Job MeleeAttackJob(Pawn pawn, Thing enemyTarget)
	{
		if (pawn.Faction == Faction.OfPlayer)
		{
			return base.MeleeAttackJob(pawn, enemyTarget);
		}
		using PawnPath pawnPath = pawn.Map.pathFinder.FindPathNow(pawn.Position, enemyTarget.Position, TraverseParms.For(TraverseMode.PassAllDestroyablePlayerOwnedThings), tuning);
		if (!pawnPath.Found)
		{
			Job job = JobMaker.MakeJob(JobDefOf.Wait);
			job.expiryInterval = CheckForBetterTargetInterval.RandomInRange;
			return job;
		}
		IntVec3 cellBefore;
		Thing thing = pawnPath.FirstBlockingBuilding(out cellBefore, pawn);
		return base.MeleeAttackJob(pawn, thing ?? enemyTarget);
	}

	protected override bool ShouldLoseTarget(Pawn pawn)
	{
		Thing enemyTarget = pawn.mindState.enemyTarget;
		if (enemyTarget != null && !enemyTarget.Destroyed && enemyTarget.Spawned && enemyTarget.Map == pawn.Map && !((IAttackTarget)enemyTarget).ThreatDisabled(pawn))
		{
			if (Find.TickManager.TicksGame - pawn.mindState.lastEngageTargetTick > TicksSinceEngageToLoseTarget)
			{
				return !pawn.CanReach(enemyTarget, PathEndMode.Touch, Danger.Deadly, canBashDoors: false, canBashFences: false, TraverseMode.PassAllDestroyablePlayerOwnedThings);
			}
			return false;
		}
		return true;
	}

	protected override bool TryFindShootingPosition(Pawn pawn, out IntVec3 dest, Verb verbToUse = null)
	{
		dest = IntVec3.Invalid;
		return false;
	}
}
