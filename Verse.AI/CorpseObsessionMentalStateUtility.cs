using RimWorld;

namespace Verse.AI;

public static class CorpseObsessionMentalStateUtility
{
	public static Corpse GetClosestCorpseToDigUp(Pawn pawn)
	{
		if (!pawn.Spawned)
		{
			return null;
		}
		if (ModsConfig.AnomalyActive && Find.Anomaly.TryGetUnnaturalCorpseTrackerForHaunted(pawn, out var tracker) && IsCorpseValid(tracker.Corpse, pawn))
		{
			return tracker.Corpse;
		}
		return ((Building_Grave)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Grave), PathEndMode.InteractionCell, TraverseParms.For(pawn), 9999f, delegate(Thing x)
		{
			Building_Grave building_Grave = (Building_Grave)x;
			return building_Grave.HasCorpse && IsCorpseValid(building_Grave.Corpse, pawn, ignoreReachability: true);
		}))?.Corpse;
	}

	public static bool IsCorpseValid(Corpse corpse, Pawn pawn, bool ignoreReachability = false)
	{
		if (corpse == null || corpse.Destroyed || !corpse.InnerPawn.RaceProps.Humanlike)
		{
			return false;
		}
		if (pawn.carryTracker.CarriedThing == corpse)
		{
			return true;
		}
		if (corpse.Spawned)
		{
			if (!pawn.CanReserve(corpse))
			{
				return false;
			}
			if (!ignoreReachability)
			{
				return pawn.CanReach(corpse, PathEndMode.Touch, Danger.Deadly);
			}
			return true;
		}
		if (corpse.ParentHolder is Building_Grave { Spawned: not false } building_Grave)
		{
			if (!pawn.CanReserve(building_Grave))
			{
				return false;
			}
			if (!ignoreReachability)
			{
				return pawn.CanReach(building_Grave, PathEndMode.InteractionCell, Danger.Deadly);
			}
			return true;
		}
		return false;
	}
}
