using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_GetEnergy_Charger : JobGiver_GetEnergy
{
	public static Building_MechCharger GetClosestCharger(Pawn mech, Pawn carrier, bool forced)
	{
		if (!mech.Spawned || !carrier.Spawned)
		{
			return null;
		}
		Danger danger = (forced ? Danger.Deadly : Danger.Some);
		return (Building_MechCharger)GenClosest.ClosestThingReachable(mech.Position, mech.Map, ThingRequest.ForGroup(ThingRequestGroup.MechCharger), PathEndMode.InteractionCell, TraverseParms.For(carrier, danger), 9999f, delegate(Thing t)
		{
			Building_MechCharger building_MechCharger = (Building_MechCharger)t;
			if (!carrier.CanReach(t, PathEndMode.InteractionCell, danger))
			{
				return false;
			}
			if (carrier != mech)
			{
				if (!forced && building_MechCharger.Map.reservationManager.ReservedBy(building_MechCharger, carrier))
				{
					return false;
				}
				if (forced && KeyBindingDefOf.QueueOrder.IsDownEvent && building_MechCharger.Map.reservationManager.ReservedBy(building_MechCharger, carrier))
				{
					return false;
				}
			}
			return !t.IsForbidden(carrier) && carrier.CanReserve(t, 1, -1, null, forced) && building_MechCharger.CanPawnChargeCurrently(mech);
		});
	}

	protected override Job TryGiveJob(Pawn pawn)
	{
		if (!ShouldAutoRecharge(pawn))
		{
			return null;
		}
		Building_MechCharger closestCharger = GetClosestCharger(pawn, pawn, forced: false);
		if (closestCharger != null)
		{
			Job job = JobMaker.MakeJob(JobDefOf.MechCharge, closestCharger);
			job.overrideFacing = Rot4.South;
			return job;
		}
		return null;
	}
}
