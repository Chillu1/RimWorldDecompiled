using Verse;
using Verse.AI;

namespace RimWorld;

public class PsycastUtility
{
	public static float TotalEntropyFromQueuedPsycasts(Pawn pawn)
	{
		float num = ((pawn.jobs?.curJob?.verbToUse is Verb_CastPsycast verb_CastPsycast) ? verb_CastPsycast.Psycast.def.EntropyGain : 0f);
		if (pawn.jobs != null)
		{
			for (int i = 0; i < pawn.jobs.jobQueue.Count; i++)
			{
				if (pawn.jobs.jobQueue[i].job.verbToUse is Verb_CastPsycast verb_CastPsycast2)
				{
					num += verb_CastPsycast2.Psycast.def.EntropyGain;
				}
			}
		}
		return num;
	}

	public static float TotalPsyfocusCostOfQueuedPsycasts(Pawn pawn)
	{
		float num = ((pawn.jobs?.curJob?.verbToUse is Verb_CastPsycast verb_CastPsycast) ? verb_CastPsycast.Psycast.FinalPsyfocusCost(pawn.jobs.curJob.targetA) : 0f);
		if (pawn.jobs != null)
		{
			for (int i = 0; i < pawn.jobs.jobQueue.Count; i++)
			{
				QueuedJob queuedJob = pawn.jobs.jobQueue[i];
				if (queuedJob.job.verbToUse is Verb_CastPsycast verb_CastPsycast2)
				{
					num += verb_CastPsycast2.Psycast.FinalPsyfocusCost(queuedJob.job.targetA);
				}
			}
		}
		return num;
	}
}
