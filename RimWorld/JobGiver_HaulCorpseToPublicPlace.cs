using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_HaulCorpseToPublicPlace : ThinkNode_JobGiver
{
	protected override Job TryGiveJob(Pawn pawn)
	{
		if (pawn.MentalState is MentalState_CorpseObsession { corpse: not null, alreadyHauledCorpse: false, corpse: var corpse } mentalState_CorpseObsession)
		{
			Building_Grave building_Grave = mentalState_CorpseObsession.corpse.ParentHolder as Building_Grave;
			if (building_Grave != null)
			{
				if (!pawn.CanReserveAndReach(building_Grave, PathEndMode.InteractionCell, Danger.Deadly))
				{
					return null;
				}
			}
			else if (!pawn.CanReserveAndReach(corpse, PathEndMode.Touch, Danger.Deadly))
			{
				return null;
			}
			Job job = JobMaker.MakeJob(JobDefOf.HaulCorpseToPublicPlace, corpse, building_Grave);
			job.count = 1;
			return job;
		}
		return null;
	}
}
