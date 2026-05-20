using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class JobGiver_Duel : JobGiver_AIFightEnemies
{
	public const float MinDistOpponentWhenMoving = 1.9f;

	public const float MaxFightMoveDist = 3.1f;

	protected override bool DisableAbilityVerbs => true;

	protected override Job TryGiveJob(Pawn pawn)
	{
		if (pawn.GetLord()?.LordJob is LordJob_Ritual_Duel lordJob_Ritual_Duel)
		{
			lordJob_Ritual_Duel.StartDuelIfNotStartedYet();
			if (lordJob_Ritual_Duel.CurrentDuelStage == DuelBehaviorStage.Attack)
			{
				return base.TryGiveJob(pawn);
			}
			Job job = JobMaker.MakeJob(JobDefOf.Goto, GetMoveTarget(pawn, lordJob_Ritual_Duel), lordJob_Ritual_Duel.Opponent(pawn));
			job.checkOverrideOnExpire = true;
			job.expiryInterval = 40;
			job.collideWithPawns = true;
			job.locomotionUrgency = LocomotionUrgency.Sprint;
			return job;
		}
		return null;
	}

	private LocalTargetInfo GetMoveTarget(Pawn pawn, LordJob_Ritual_Duel duel)
	{
		Pawn opponent = duel.Opponent(pawn);
		return RCellFinder.RandomWanderDestFor(pawn, duel.selectedTarget.Cell, 3.1f, delegate(Pawn p, IntVec3 c, IntVec3 r)
		{
			if (c == pawn.Position || !c.Standable(p.Map) || !p.CanReserveAndReach(c, PathEndMode.OnCell, Danger.Deadly) || c.DistanceTo(duel.selectedTarget.Cell) > 3.1f)
			{
				return false;
			}
			IntVec3 intVec = ((opponent.CurJob?.def == JobDefOf.Goto) ? opponent.CurJob.targetA.Cell : IntVec3.Invalid);
			if (c.DistanceTo(opponent.Position) < 1.9f || (intVec != IntVec3.Invalid && intVec.DistanceTo(c) < 1.9f))
			{
				return false;
			}
			PawnPath pawnPath = pawn.Map.pathFinder.FindPathNow(pawn.Position, c, pawn);
			try
			{
				foreach (IntVec3 item in pawnPath.NodesReversed)
				{
					if (item.DistanceTo(opponent.Position) < 1.9f || item.DistanceTo(duel.selectedTarget.Cell) > 3.1f)
					{
						return false;
					}
				}
				if (intVec != IntVec3.Invalid && opponent.pather.curPath != null)
				{
					foreach (IntVec3 item2 in opponent.pather.curPath.NodesReversed)
					{
						if (item2.DistanceTo(pawn.Position) < 1.9f || item2.DistanceTo(duel.selectedTarget.Cell) > 3.1f)
						{
							return false;
						}
						foreach (IntVec3 item3 in pawnPath.NodesReversed)
						{
							if (item2.DistanceTo(item3) < 1.9f)
							{
								return false;
							}
						}
					}
				}
			}
			finally
			{
				pawnPath.ReleaseToPool();
			}
			return true;
		}, Danger.Deadly);
	}

	protected override void UpdateEnemyTarget(Pawn pawn)
	{
		Pawn pawn2 = ((LordJob_Ritual_Duel)pawn.GetLord().LordJob).Opponent(pawn);
		if (pawn2 == null || pawn2.Dead)
		{
			pawn.mindState.enemyTarget = null;
		}
		else
		{
			pawn.mindState.enemyTarget = pawn2;
		}
	}

	protected override Job MeleeAttackJob(Pawn pawn, Thing enemyTarget)
	{
		Job job = base.MeleeAttackJob(pawn, enemyTarget);
		job.killIncappedTarget = true;
		return job;
	}
}
