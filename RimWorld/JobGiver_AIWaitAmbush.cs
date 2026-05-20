using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_AIWaitAmbush : ThinkNode_JobGiver
{
	private const int WanderOutsideDoorRegions = 4;

	private const int WaitTicks = 300;

	private const int EnemyScanCheckTicks = 30;

	private bool ignoreNonCombatants;

	private bool humanlikesOnly;

	protected bool expireOnNearbyEnemy;

	public override ThinkNode DeepCopy(bool resolve = true)
	{
		JobGiver_AIWaitAmbush obj = (JobGiver_AIWaitAmbush)base.DeepCopy(resolve);
		obj.ignoreNonCombatants = ignoreNonCombatants;
		obj.humanlikesOnly = humanlikesOnly;
		return obj;
	}

	protected override Job TryGiveJob(Pawn pawn)
	{
		Thing thing = FindBestTarget(pawn);
		if (thing == null)
		{
			return null;
		}
		using PawnPath pawnPath = pawn.Map.pathFinder.FindPathNow(pawn.Position, thing.Position, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassDoors));
		if (!pawnPath.Found)
		{
			return null;
		}
		if (!pawnPath.TryFindLastCellBeforeBlockingDoor(pawn, out var result))
		{
			if (thing.Position.GetDoor(thing.Map) == null)
			{
				Log.Error(pawn?.ToString() + " did TryFindLastCellBeforeDoor but found none when it should have been one. Target: " + thing.LabelCap);
				return null;
			}
			result = pawnPath.NodesReversed[1];
		}
		IntVec3 randomCell = CellFinder.RandomRegionNear(result.GetRegion(pawn.Map), 4, TraverseParms.For(pawn)).RandomCell;
		Job job;
		if (randomCell == pawn.Position)
		{
			job = JobMaker.MakeJob(JobDefOf.Wait, 30);
		}
		pawn.mindState.nextMoveOrderIsWait = !pawn.mindState.nextMoveOrderIsWait;
		job = ((!pawn.mindState.nextMoveOrderIsWait) ? JobMaker.MakeJob(JobDefOf.Goto, randomCell) : JobMaker.MakeJob(JobDefOf.Wait, 300));
		if (expireOnNearbyEnemy)
		{
			job.expiryInterval = 30;
			job.checkOverrideOnExpire = true;
		}
		return job;
	}

	private Thing FindBestTarget(Pawn pawn)
	{
		float num = float.MaxValue;
		Thing result = null;
		List<IAttackTarget> potentialTargetsFor = pawn.Map.attackTargetsCache.GetPotentialTargetsFor(pawn);
		for (int i = 0; i < potentialTargetsFor.Count; i++)
		{
			IAttackTarget attackTarget = potentialTargetsFor[i];
			if (!attackTarget.ThreatDisabled(pawn) && AttackTargetFinder.IsAutoTargetable(attackTarget) && (!humanlikesOnly || !(attackTarget is Pawn pawn2) || pawn2.RaceProps.Humanlike) && (!(attackTarget.Thing is Pawn pawn3) || pawn3.IsCombatant() || (!ignoreNonCombatants && GenSight.LineOfSightToThing(pawn.Position, pawn3, pawn.Map))))
			{
				Thing thing = (Thing)attackTarget;
				int num2 = thing.Position.DistanceToSquared(pawn.Position);
				if ((float)num2 < num && pawn.CanReach(thing, PathEndMode.OnCell, Danger.Deadly, canBashDoors: false, canBashFences: false, TraverseMode.PassDoors))
				{
					num = num2;
					result = thing;
				}
			}
		}
		return result;
	}
}
