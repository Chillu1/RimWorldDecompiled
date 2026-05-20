using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_HaulToPortal : JobDriver_HaulToContainer
{
	private const int DepositDuration = 90;

	public int initialCount;

	public MapPortal MapPortal => base.Container as MapPortal;

	protected override int Duration => 90;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref initialCount, "initialCount", 0);
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		pawn.ReserveAsManyAsPossible(job.GetTargetQueue(TargetIndex.A), job);
		pawn.ReserveAsManyAsPossible(job.GetTargetQueue(TargetIndex.B), job);
		return true;
	}

	public override void Notify_Starting()
	{
		base.Notify_Starting();
		ThingCount thingCount = ((!job.targetA.IsValid) ? EnterPortalUtility.FindThingToLoad(pawn, MapPortal) : new ThingCount(job.targetA.Thing, job.targetA.Thing.stackCount));
		if (job.playerForced && pawn.carryTracker.CarriedThing != null && pawn.carryTracker.CarriedThing != thingCount.Thing)
		{
			pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out var _);
		}
		job.targetA = thingCount.Thing;
		job.count = thingCount.Count;
		initialCount = thingCount.Count;
		pawn.Reserve(thingCount.Thing, job);
	}
}
