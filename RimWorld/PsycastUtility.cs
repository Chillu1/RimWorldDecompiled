using Verse;
using Verse.AI;

namespace RimWorld
{
	public class PsycastUtility
	{
		public static float TotalEntropyFromQueuedPsycasts(Pawn pawn)
		{
			float num = (pawn.jobs?.curJob?.verbToUse as Verb_CastPsycast)?.Psycast.def.EntropyGain ?? 0f;
			if (pawn.jobs != null)
			{
				for (int i = 0; i < pawn.jobs.jobQueue.Count; i++)
				{
					Verb_CastPsycast verb_CastPsycast;
					if ((verb_CastPsycast = pawn.jobs.jobQueue[i].job.verbToUse as Verb_CastPsycast) != null)
					{
						num += verb_CastPsycast.Psycast.def.EntropyGain;
					}
				}
			}
			return num;
		}

		public static float TotalPsyfocusCostOfQueuedPsycasts(Pawn pawn)
		{
			float num = (pawn.jobs?.curJob?.verbToUse as Verb_CastPsycast)?.Psycast.FinalPsyfocusCost(pawn.jobs.curJob.targetA) ?? 0f;
			if (pawn.jobs != null)
			{
				for (int i = 0; i < pawn.jobs.jobQueue.Count; i++)
				{
					QueuedJob queuedJob = pawn.jobs.jobQueue[i];
					Verb_CastPsycast verb_CastPsycast;
					if ((verb_CastPsycast = queuedJob.job.verbToUse as Verb_CastPsycast) != null)
					{
						num += verb_CastPsycast.Psycast.FinalPsyfocusCost(queuedJob.job.targetA);
					}
				}
			}
			return num;
		}
	}
}
