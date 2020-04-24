using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_Warden_TakeToBed : WorkGiver_Warden
	{
		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (!ShouldTakeCareOfPrisoner(pawn, t))
			{
				return null;
			}
			Pawn prisoner = (Pawn)t;
			Job job = TakeDownedToBedJob(prisoner, pawn);
			if (job != null)
			{
				return job;
			}
			Job job2 = TakeToPreferredBedJob(prisoner, pawn);
			if (job2 != null)
			{
				return job2;
			}
			return null;
		}

		private Job TakeToPreferredBedJob(Pawn prisoner, Pawn warden)
		{
			if (prisoner.Downed || !warden.CanReserve(prisoner))
			{
				return null;
			}
			if (RestUtility.FindBedFor(prisoner, prisoner, sleeperWillBePrisoner: true, checkSocialProperness: true) != null)
			{
				return null;
			}
			Room room = prisoner.GetRoom();
			Building_Bed building_Bed = RestUtility.FindBedFor(prisoner, warden, sleeperWillBePrisoner: true, checkSocialProperness: false);
			if (building_Bed != null && building_Bed.GetRoom() != room)
			{
				Job job = JobMaker.MakeJob(JobDefOf.EscortPrisonerToBed, prisoner, building_Bed);
				job.count = 1;
				return job;
			}
			return null;
		}

		private Job TakeDownedToBedJob(Pawn prisoner, Pawn warden)
		{
			if (!prisoner.Downed || !HealthAIUtility.ShouldSeekMedicalRestUrgent(prisoner) || prisoner.InBed() || !warden.CanReserve(prisoner))
			{
				return null;
			}
			Building_Bed building_Bed = RestUtility.FindBedFor(prisoner, warden, sleeperWillBePrisoner: true, checkSocialProperness: true);
			if (building_Bed != null)
			{
				Job job = JobMaker.MakeJob(JobDefOf.TakeWoundedPrisonerToBed, prisoner, building_Bed);
				job.count = 1;
				return job;
			}
			return null;
		}
	}
}
