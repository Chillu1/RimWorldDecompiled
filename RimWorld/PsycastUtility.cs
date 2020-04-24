using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class PsycastUtility
	{
		public static float TotalEntropyFromQueuedPsycasts(Pawn pawn)
		{
			return ((pawn.jobs.curJob?.verbToUse as Verb_CastPsycast)?.Psycast.def.EntropyGain ?? 0f) + pawn.jobs.jobQueue.Select((QueuedJob qj) => qj.job.verbToUse).OfType<Verb_CastPsycast>().Sum((Verb_CastPsycast t) => t.Psycast.def.EntropyGain);
		}
	}
}
