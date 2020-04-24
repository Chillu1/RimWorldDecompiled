using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class DigUtility
	{
		private const int CheckOverrideInterval = 500;

		public static Job PassBlockerJob(Pawn pawn, Thing blocker, IntVec3 cellBeforeBlocker, bool canMineMineables, bool canMineNonMineables)
		{
			if (StatDefOf.MiningSpeed.Worker.IsDisabledFor(pawn))
			{
				canMineMineables = false;
				canMineNonMineables = false;
			}
			if (blocker.def.mineable)
			{
				if (canMineMineables)
				{
					return MineOrWaitJob(pawn, blocker, cellBeforeBlocker);
				}
				return MeleeOrWaitJob(pawn, blocker, cellBeforeBlocker);
			}
			if (pawn.equipment != null && pawn.equipment.Primary != null)
			{
				Verb primaryVerb = pawn.equipment.PrimaryEq.PrimaryVerb;
				if (primaryVerb.verbProps.ai_IsBuildingDestroyer && (!primaryVerb.IsIncendiary() || blocker.FlammableNow))
				{
					Job job = JobMaker.MakeJob(JobDefOf.UseVerbOnThing);
					job.targetA = blocker;
					job.verbToUse = primaryVerb;
					job.expiryInterval = JobGiver_AIFightEnemy.ExpiryInterval_ShooterSucceeded.RandomInRange;
					return job;
				}
			}
			if (canMineNonMineables)
			{
				return MineOrWaitJob(pawn, blocker, cellBeforeBlocker);
			}
			return MeleeOrWaitJob(pawn, blocker, cellBeforeBlocker);
		}

		private static Job MeleeOrWaitJob(Pawn pawn, Thing blocker, IntVec3 cellBeforeBlocker)
		{
			if (!pawn.CanReserve(blocker))
			{
				return WaitNearJob(pawn, cellBeforeBlocker);
			}
			Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, blocker);
			job.ignoreDesignations = true;
			job.expiryInterval = JobGiver_AIFightEnemy.ExpiryInterval_ShooterSucceeded.RandomInRange;
			job.checkOverrideOnExpire = true;
			return job;
		}

		private static Job MineOrWaitJob(Pawn pawn, Thing blocker, IntVec3 cellBeforeBlocker)
		{
			if (!pawn.CanReserve(blocker))
			{
				return WaitNearJob(pawn, cellBeforeBlocker);
			}
			Job job = JobMaker.MakeJob(JobDefOf.Mine, blocker);
			job.ignoreDesignations = true;
			job.expiryInterval = JobGiver_AIFightEnemy.ExpiryInterval_ShooterSucceeded.RandomInRange;
			job.checkOverrideOnExpire = true;
			return job;
		}

		private static Job WaitNearJob(Pawn pawn, IntVec3 cellBeforeBlocker)
		{
			IntVec3 intVec = CellFinder.RandomClosewalkCellNear(cellBeforeBlocker, pawn.Map, 10);
			if (intVec == pawn.Position)
			{
				return JobMaker.MakeJob(JobDefOf.Wait, 20, checkOverrideOnExpiry: true);
			}
			return JobMaker.MakeJob(JobDefOf.Goto, intVec, 500, checkOverrideOnExpiry: true);
		}
	}
}
