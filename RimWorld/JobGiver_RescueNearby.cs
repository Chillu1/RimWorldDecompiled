using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_RescueNearby : ThinkNode_JobGiver
{
	private float radius = 30f;

	private const float MinDistFromEnemy = 25f;

	public override ThinkNode DeepCopy(bool resolve = true)
	{
		JobGiver_RescueNearby obj = (JobGiver_RescueNearby)base.DeepCopy(resolve);
		obj.radius = radius;
		return obj;
	}

	protected override Job TryGiveJob(Pawn pawn)
	{
		Pawn pawn2 = (Pawn)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.OnCell, TraverseParms.For(pawn), radius, Validator);
		if (pawn2 == null)
		{
			return null;
		}
		Building_Bed building_Bed = RestUtility.FindBedFor(pawn2, pawn, checkSocialProperness: false, ignoreOtherReservations: false, pawn2.GuestStatus);
		if (building_Bed == null || !pawn2.CanReserve(building_Bed))
		{
			return null;
		}
		Job job = JobMaker.MakeJob(JobDefOf.Rescue, pawn2, building_Bed);
		job.count = 1;
		return job;
		bool Validator(Thing t)
		{
			Pawn patient = (Pawn)t;
			if (!HealthAIUtility.CanRescueNow(pawn, patient))
			{
				return false;
			}
			return true;
		}
	}
}
