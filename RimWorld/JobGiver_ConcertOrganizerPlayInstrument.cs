using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class JobGiver_ConcertOrganizerPlayInstrument : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (pawn.mindState.duty == null)
			{
				return null;
			}
			LordJob_Joinable_Concert lordJob_Joinable_Concert = pawn.GetLord().LordJob as LordJob_Joinable_Concert;
			if (lordJob_Joinable_Concert == null || lordJob_Joinable_Concert.Organizer != pawn)
			{
				return null;
			}
			IntVec3 gatherSpot = pawn.mindState.duty.focus.Cell;
			Building_MusicalInstrument building_MusicalInstrument = pawn.Map.listerBuildings.AllBuildingsColonistOfClass<Building_MusicalInstrument>().Where(delegate(Building_MusicalInstrument i)
			{
				if (!GatheringsUtility.InGatheringArea(i.InteractionCell, gatherSpot, pawn.Map))
				{
					return false;
				}
				return GatheringWorker_Concert.InstrumentAccessible(i, pawn) ? true : false;
			}).RandomElementWithFallback();
			if (building_MusicalInstrument != null)
			{
				Job job = JobMaker.MakeJob(JobDefOf.Play_MusicalInstrument, building_MusicalInstrument, building_MusicalInstrument.InteractionCell);
				job.doUntilGatheringEnded = true;
				job.expiryInterval = lordJob_Joinable_Concert.DurationTicks;
				return job;
			}
			return null;
		}
	}
}
