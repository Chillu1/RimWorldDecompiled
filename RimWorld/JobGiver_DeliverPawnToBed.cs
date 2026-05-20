using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_DeliverPawnToBed : JobGiver_GotoTravelDestination
{
	public bool addArrivalTagIfTargetIsDead;

	public bool addArrivalTagIfNoBedAvailable;

	public bool ignoreOtherReservations;

	public override ThinkNode DeepCopy(bool resolve = true)
	{
		JobGiver_DeliverPawnToBed obj = (JobGiver_DeliverPawnToBed)base.DeepCopy(resolve);
		obj.addArrivalTagIfTargetIsDead = addArrivalTagIfTargetIsDead;
		obj.addArrivalTagIfNoBedAvailable = addArrivalTagIfNoBedAvailable;
		obj.ignoreOtherReservations = ignoreOtherReservations;
		return obj;
	}

	protected override Job TryGiveJob(Pawn pawn)
	{
		Pawn pawn2 = pawn.mindState.duty.focusSecond.Pawn;
		if (pawn2 == null)
		{
			return null;
		}
		if (addArrivalTagIfTargetIsDead && pawn2.Dead)
		{
			RitualUtility.AddArrivalTag(pawn);
			RitualUtility.AddArrivalTag(pawn2);
			return null;
		}
		if (!pawn.CanReserveAndReach(pawn2, PathEndMode.Touch, PawnUtility.ResolveMaxDanger(pawn, maxDanger), 1, -1, null, ignoreOtherReservations))
		{
			return null;
		}
		Building_Bed building_Bed = pawn.mindState.duty.focusThird.Thing as Building_Bed;
		if (building_Bed == null && pawn2.CurrentBed() != null)
		{
			return null;
		}
		building_Bed = building_Bed ?? RestUtility.FindBedFor(pawn2, pawn, checkSocialProperness: true, ignoreOtherReservations: false, pawn2.GuestStatus);
		if (building_Bed != null && !RestUtility.IsValidBedFor(building_Bed, pawn2, pawn, checkSocialProperness: true))
		{
			building_Bed = null;
		}
		if (building_Bed != null)
		{
			pawn2.ownership.ClaimBedIfNonMedical(building_Bed);
			Job job = JobMaker.MakeJob(JobDefOf.DeliverToBed, pawn2, building_Bed);
			job.locomotionUrgency = PawnUtility.ResolveLocomotion(pawn, locomotionUrgency);
			job.expiryInterval = jobMaxDuration;
			job.count = 1;
			job.ritualTag = "Arrived";
			return job;
		}
		if (addArrivalTagIfNoBedAvailable)
		{
			RitualUtility.AddArrivalTag(pawn);
			RitualUtility.AddArrivalTag(pawn2);
		}
		return null;
	}
}
