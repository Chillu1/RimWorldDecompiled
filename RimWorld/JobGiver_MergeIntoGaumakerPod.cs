using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_MergeIntoGaumakerPod : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (!ModsConfig.IdeologyActive)
			{
				return null;
			}
			if (pawn.connections == null || pawn.connections.ConnectedThings.NullOrEmpty())
			{
				return null;
			}
			foreach (Thing connectedThing in pawn.connections.ConnectedThings)
			{
				CompTreeConnection compTreeConnection = connectedThing.TryGetComp<CompTreeConnection>();
				if (compTreeConnection != null && compTreeConnection.ShouldEnterGaumakerPod(pawn) && pawn.CanReach(compTreeConnection.gaumakerPod, PathEndMode.Touch, Danger.Deadly))
				{
					return JobMaker.MakeJob(JobDefOf.MergeIntoGaumakerPod, connectedThing, compTreeConnection.gaumakerPod);
				}
			}
			return null;
		}
	}
}
