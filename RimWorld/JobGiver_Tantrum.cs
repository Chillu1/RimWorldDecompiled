using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_Tantrum : ThinkNode_JobGiver
{
	protected override Job TryGiveJob(Pawn pawn)
	{
		if (!(pawn.MentalState is MentalState_Tantrum { target: not null } mentalState_Tantrum) || !mentalState_Tantrum.target.Spawned || !pawn.CanReach(mentalState_Tantrum.target, PathEndMode.Touch, Danger.Deadly))
		{
			return null;
		}
		Verb verb = null;
		if (mentalState_Tantrum.target is Pawn pawn2)
		{
			if (pawn2.Downed)
			{
				return null;
			}
			if (!SocialInteractionUtility.TryGetRandomVerbForSocialFight(pawn, out verb))
			{
				return null;
			}
		}
		Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, mentalState_Tantrum.target);
		job.maxNumMeleeAttacks = 1;
		job.verbToUse = verb;
		return job;
	}
}
