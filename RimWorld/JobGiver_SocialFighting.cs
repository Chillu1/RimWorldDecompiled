using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_SocialFighting : ThinkNode_JobGiver
{
	protected override Job TryGiveJob(Pawn pawn)
	{
		if (pawn.RaceProps.Humanlike && pawn.WorkTagIsDisabled(WorkTags.Violent))
		{
			return null;
		}
		Pawn otherPawn = ((MentalState_SocialFighting)pawn.MentalState).otherPawn;
		if (!otherPawn.Spawned || otherPawn.Map != pawn.Map)
		{
			return null;
		}
		if (!SocialInteractionUtility.TryGetRandomVerbForSocialFight(pawn, out var verb))
		{
			return null;
		}
		Job job = JobMaker.MakeJob(JobDefOf.SocialFight, otherPawn);
		job.maxNumMeleeAttacks = 1;
		job.verbToUse = verb;
		return job;
	}
}
