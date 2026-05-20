using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class JobGiver_AIBreaching : ThinkNode_JobGiver
{
	private const float ReachDestDist = 5f;

	private const int CheckOverrideInterval = 500;

	private const float WanderDuringBusyJobChance = 0.3f;

	private static IntRange WanderTicks = new IntRange(30, 80);

	protected override Job TryGiveJob(Pawn pawn)
	{
		IntVec3 cell = pawn.mindState.duty.focus.Cell;
		if (cell.IsValid && (float)cell.DistanceToSquared(pawn.Position) < 25f && cell.GetRoom(pawn.Map) == pawn.GetRoom() && cell.WithinRegions(pawn.Position, pawn.Map, 9, TraverseMode.NoPassClosedDoors))
		{
			pawn.GetLord().Notify_ReachedDutyLocation(pawn);
			return null;
		}
		Verb verb = BreachingUtility.FindVerbToUseForBreaching(pawn);
		if (verb == null)
		{
			return null;
		}
		UpdateBreachingTarget(pawn, verb);
		BreachingTargetData breachingTarget = pawn.mindState.breachingTarget;
		if (breachingTarget == null)
		{
			if (cell.IsValid && pawn.CanReach(cell, PathEndMode.OnCell, Danger.Deadly))
			{
				Job job = JobMaker.MakeJob(JobDefOf.Goto, cell, 500, checkOverrideOnExpiry: true);
				BreachingUtility.FinalizeTrashJob(job);
				return job;
			}
			return null;
		}
		if (!breachingTarget.firingPosition.IsValid)
		{
			return null;
		}
		Thing target = breachingTarget.target;
		IntVec3 firingPosition = breachingTarget.firingPosition;
		if (verb.IsMeleeAttack)
		{
			Job job2 = JobMaker.MakeJob(JobDefOf.AttackMelee, target, firingPosition);
			job2.verbToUse = verb;
			BreachingUtility.FinalizeTrashJob(job2);
			return job2;
		}
		bool flag = firingPosition.Standable(pawn.Map) && pawn.Map.pawnDestinationReservationManager.CanReserve(firingPosition, pawn);
		Job job3 = JobMaker.MakeJob(JobDefOf.UseVerbOnThing, target, flag ? firingPosition : IntVec3.Invalid);
		job3.verbToUse = verb;
		job3.preventFriendlyFire = true;
		BreachingUtility.FinalizeTrashJob(job3);
		return job3;
	}

	private void UpdateBreachingTarget(Pawn pawn, Verb verb)
	{
		try
		{
			LordToil_AssaultColonyBreaching lordToil_AssaultColonyBreaching = BreachingUtility.LordToilOf(pawn);
			if (lordToil_AssaultColonyBreaching != null)
			{
				lordToil_AssaultColonyBreaching.UpdateCurrentBreachTarget();
				LordToilData_AssaultColonyBreaching data = lordToil_AssaultColonyBreaching.Data;
				BreachingGrid breachingGrid = lordToil_AssaultColonyBreaching.Data.breachingGrid;
				BreachingTargetData breachingTargetData = pawn.mindState.breachingTarget;
				bool flag = false;
				if (breachingTargetData?.target != null && (breachingTargetData.target.Destroyed || data.currentTarget != breachingTargetData.target || breachingGrid.MarkerGrid[pawn.Position] == 10 || (breachingTargetData.firingPosition.IsValid && !verb.IsMeleeAttack && !verb.CanHitTargetFrom(breachingTargetData.firingPosition, breachingTargetData.target)) || (breachingTargetData.firingPosition.IsValid && !pawn.CanReach(breachingTargetData.firingPosition, PathEndMode.OnCell, Danger.Deadly)) || (data.soloAttacker != null && pawn != data.soloAttacker)))
				{
					breachingTargetData = null;
					flag = true;
				}
				if (breachingTargetData == null && data.currentTarget != null)
				{
					flag = true;
					breachingTargetData = new BreachingTargetData(data.currentTarget, IntVec3.Invalid);
				}
				bool flag2 = (BreachingUtility.IsSoloAttackVerb(verb) ? (data.soloAttacker == pawn) : (data.soloAttacker == null));
				if (breachingTargetData?.target != null && !breachingTargetData.firingPosition.IsValid && BreachingUtility.CanDamageTarget(verb, breachingTargetData.target) && flag2 && BreachingUtility.TryFindCastPosition(pawn, verb, breachingTargetData.target, out breachingTargetData.firingPosition))
				{
					flag = true;
				}
				if (flag)
				{
					pawn.mindState.breachingTarget = breachingTargetData;
					breachingGrid.Notify_PawnStateChanged(pawn);
				}
			}
		}
		finally
		{
		}
	}
}
