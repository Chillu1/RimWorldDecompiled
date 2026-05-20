using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_MurderousRage : ThinkNode_JobGiver
{
	protected override Job TryGiveJob(Pawn pawn)
	{
		if (!(pawn.MentalState is MentalState_MurderousRage mentalState_MurderousRage) || !mentalState_MurderousRage.IsTargetStillValidAndReachable())
		{
			return null;
		}
		Thing spawnedParentOrMe = mentalState_MurderousRage.target.SpawnedParentOrMe;
		Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, spawnedParentOrMe);
		job.canBashDoors = true;
		job.killIncappedTarget = true;
		if (spawnedParentOrMe != mentalState_MurderousRage.target)
		{
			job.maxNumMeleeAttacks = 2;
		}
		return job;
	}
}
