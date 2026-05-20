using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_Warden_ImprisonSlave : WorkGiver_Warden
	{
		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (!ModLister.CheckIdeology("Slave imprisonment"))
			{
				return null;
			}
			Pawn pawn2 = t as Pawn;
			if (!ShouldTakeCareOfSlave(pawn, pawn2))
			{
				return null;
			}
			if (pawn2.guest.slaveInteractionMode != SlaveInteractionModeDefOf.Imprison)
			{
				return null;
			}
			Building_Bed building_Bed = RestUtility.FindBedFor(pawn2, pawn, checkSocialProperness: false, ignoreOtherReservations: false, GuestStatus.Prisoner);
			if (building_Bed == null)
			{
				return null;
			}
			Job job = JobMaker.MakeJob(JobDefOf.Arrest, pawn2, building_Bed);
			job.count = 1;
			return job;
		}
	}
}
