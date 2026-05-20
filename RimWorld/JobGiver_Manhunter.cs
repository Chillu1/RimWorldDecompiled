using System;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_Manhunter : ThinkNode_JobGiver
{
	public bool canBashDoors = true;

	private const float WaitChance = 0.75f;

	private const int WaitTicks = 90;

	private const int MinMeleeChaseTicks = 420;

	private const int MaxMeleeChaseTicks = 900;

	private const int WanderOutsideDoorRegions = 9;

	protected override Job TryGiveJob(Pawn pawn)
	{
		if (pawn.TryGetAttackVerb(null) == null)
		{
			return null;
		}
		bool fenceBlocked = pawn.FenceBlocked;
		Pawn pawn2 = FindPawnTarget(pawn, fenceBlocked, canBashDoors);
		if (pawn2 != null && pawn.CanReach(pawn2, PathEndMode.Touch, Danger.Deadly, canBashDoors: false, fenceBlocked))
		{
			Job abilityJob = JobGiver_AIFightEnemy.GetAbilityJob(pawn, pawn2);
			if (abilityJob != null)
			{
				return abilityJob;
			}
			return MeleeAttackJob(pawn2, fenceBlocked, canBashDoors);
		}
		Building building = FindTurretTarget(pawn, fenceBlocked);
		if (building != null)
		{
			return MeleeAttackJob(building, fenceBlocked, canBashDoors);
		}
		if (pawn2 != null)
		{
			using (PawnPath pawnPath = pawn.Map.pathFinder.FindPathNow(pawn.Position, pawn2.Position, TraverseParms.For(pawn, Danger.Deadly, canBashDoors ? TraverseMode.PassDoors : TraverseMode.NoPassClosedDoors)))
			{
				if (!pawnPath.Found)
				{
					return null;
				}
				if (!pawnPath.TryFindLastCellBeforeBlockingDoor(pawn, out var result))
				{
					if (pawn2.Position.GetDoor(pawn2.Map) == null)
					{
						Log.Error(pawn?.ToString() + " did TryFindLastCellBeforeDoor but found none when it should have been one. Target: " + pawn2.LabelCap);
						return null;
					}
					result = pawnPath.NodesReversed[1];
				}
				IntVec3 randomCell = CellFinder.RandomRegionNear(result.GetRegion(pawn.Map), 9, TraverseParms.For(pawn)).RandomCell;
				if (randomCell == pawn.Position)
				{
					return JobMaker.MakeJob(JobDefOf.Wait, 30);
				}
				return JobMaker.MakeJob(JobDefOf.Goto, randomCell);
			}
		}
		return null;
	}

	private Job MeleeAttackJob(Thing target, bool canBashFences, bool canBashDoors)
	{
		Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, target);
		job.maxNumMeleeAttacks = 1;
		job.expiryInterval = Rand.Range(420, 900);
		job.attackDoorIfTargetLost = canBashDoors;
		job.canBashFences = canBashFences;
		return job;
	}

	private Pawn FindPawnTarget(Pawn pawn, bool canBashFences, bool canBashDoors)
	{
		Predicate<Thing> validator = (Thing x) => x is Pawn && (int)x.def.race.intelligence >= 1;
		bool flag = canBashDoors;
		bool canBashFences2 = canBashFences;
		return (Pawn)AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable, validator, 0f, 9999f, default(IntVec3), float.MaxValue, flag, canTakeTargetsCloserThanEffectiveMinRange: true, canBashFences2);
	}

	private Building FindTurretTarget(Pawn pawn, bool canBashFences)
	{
		Predicate<Thing> validator = (Thing t) => t is Building;
		bool canBashFences2 = canBashFences;
		return (Building)AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedLOSToAll | TargetScanFlags.NeedReachable | TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable, validator, 0f, 70f, default(IntVec3), float.MaxValue, canBashDoors: false, canTakeTargetsCloserThanEffectiveMinRange: true, canBashFences2);
	}
}
