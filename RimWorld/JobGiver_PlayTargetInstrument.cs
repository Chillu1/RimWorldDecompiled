using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class JobGiver_PlayTargetInstrument : ThinkNode_JobGiver
{
	protected override Job TryGiveJob(Pawn pawn)
	{
		if (pawn.mindState.duty?.focusSecond.Thing is Building_MusicalInstrument { Spawned: not false } building_MusicalInstrument)
		{
			if (!GatheringWorker_Concert.InstrumentAccessible(building_MusicalInstrument, pawn))
			{
				return null;
			}
			LordJob_Ritual lordJob_Ritual = pawn.GetLord().LordJob as LordJob_Ritual;
			Job job = JobMaker.MakeJob(JobDefOf.Play_MusicalInstrument, building_MusicalInstrument, building_MusicalInstrument.InteractionCell);
			job.doUntilGatheringEnded = true;
			if (lordJob_Ritual != null)
			{
				job.expiryInterval = lordJob_Ritual.DurationTicks;
			}
			else
			{
				job.expiryInterval = 2000;
			}
			return job;
		}
		return null;
	}
}
