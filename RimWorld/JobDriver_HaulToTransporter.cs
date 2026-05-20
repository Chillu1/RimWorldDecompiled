using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_HaulToTransporter : JobDriver_HaulToContainer
{
	public int initialCount;

	public CompTransporter Transporter => base.Container?.TryGetComp<CompTransporter>();

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
		ThingCount thingCount = ((!job.targetA.IsValid) ? LoadTransportersJobUtility.FindThingToLoad(pawn, base.Container.TryGetComp<CompTransporter>()) : new ThingCount(job.targetA.Thing, job.targetA.Thing.stackCount));
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
