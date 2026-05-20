using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_AIReleaseMechs : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			CompMechCarrier compMechCarrier = pawn.TryGetComp<CompMechCarrier>();
			if (compMechCarrier != null && (bool)compMechCarrier.CanSpawn)
			{
				return JobMaker.MakeJob(JobDefOf.ReleaseMechs);
			}
			return null;
		}
	}
}
