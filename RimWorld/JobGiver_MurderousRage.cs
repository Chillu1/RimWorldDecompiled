using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_MurderousRage : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			MentalState_MurderousRage mentalState_MurderousRage = pawn.MentalState as MentalState_MurderousRage;
			if (mentalState_MurderousRage == null || !mentalState_MurderousRage.IsTargetStillValidAndReachable())
			{
				return null;
			}
			Thing spawnedParentOrMe = mentalState_MurderousRage.target.SpawnedParentOrMe;
			Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, spawnedParentOrMe);
			job.canBash = true;
			job.killIncappedTarget = true;
			if (spawnedParentOrMe != mentalState_MurderousRage.target)
			{
				job.maxNumMeleeAttacks = 2;
			}
			return job;
		}
	}
}
