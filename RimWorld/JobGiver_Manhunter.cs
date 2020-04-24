using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_Manhunter : ThinkNode_JobGiver
	{
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
			Pawn pawn2 = FindPawnTarget(pawn);
			if (pawn2 != null && pawn.CanReach(pawn2, PathEndMode.Touch, Danger.Deadly))
			{
				return MeleeAttackJob(pawn, pawn2);
			}
			Building building = FindTurretTarget(pawn);
			if (building != null)
			{
				return MeleeAttackJob(pawn, building);
			}
			if (pawn2 != null)
			{
				using (PawnPath pawnPath = pawn.Map.pathFinder.FindPath(pawn.Position, pawn2.Position, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassDoors)))
				{
					if (!pawnPath.Found)
					{
						return null;
					}
					if (!pawnPath.TryFindLastCellBeforeBlockingDoor(pawn, out IntVec3 result))
					{
						Log.Error(pawn + " did TryFindLastCellBeforeDoor but found none when it should have been one. Target: " + pawn2.LabelCap);
						return null;
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

		private Job MeleeAttackJob(Pawn pawn, Thing target)
		{
			Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, target);
			job.maxNumMeleeAttacks = 1;
			job.expiryInterval = Rand.Range(420, 900);
			job.attackDoorIfTargetLost = true;
			return job;
		}

		private Pawn FindPawnTarget(Pawn pawn)
		{
			return (Pawn)AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable, (Thing x) => x is Pawn && (int)x.def.race.intelligence >= 1, 0f, 9999f, default(IntVec3), float.MaxValue, canBash: true);
		}

		private Building FindTurretTarget(Pawn pawn)
		{
			return (Building)AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedLOSToNonPawns | TargetScanFlags.NeedReachable | TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable, (Thing t) => t is Building, 0f, 70f);
		}
	}
}
