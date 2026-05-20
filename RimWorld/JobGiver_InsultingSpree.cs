using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_InsultingSpree : ThinkNode_JobGiver
{
	protected override Job TryGiveJob(Pawn pawn)
	{
		if (!(pawn.MentalState is MentalState_InsultingSpree { target: not null } mentalState_InsultingSpree) || !pawn.CanReach(mentalState_InsultingSpree.target, PathEndMode.Touch, Danger.Deadly))
		{
			return null;
		}
		if (!SocialInteractionUtility.BestInteractableCell(pawn, mentalState_InsultingSpree.target).IsValid)
		{
			return null;
		}
		return JobMaker.MakeJob(JobDefOf.Insult, mentalState_InsultingSpree.target);
	}
}
