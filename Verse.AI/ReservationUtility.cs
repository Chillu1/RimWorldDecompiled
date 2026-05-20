using System;
using System.Collections.Generic;
using RimWorld;

namespace Verse.AI;

public static class ReservationUtility
{
	public static bool CanReserveSittableOrSpot(this Pawn pawn, IntVec3 exactSittingPos, bool ignoreOtherReservations = false)
	{
		return pawn.CanReserveSittableOrSpot(exactSittingPos, null, ignoreOtherReservations);
	}

	public static bool CanReserveSittableOrSpot(this Pawn pawn, IntVec3 exactSittingPos, Thing ignoreThing, bool ignoreOtherReservations = false)
	{
		Building edifice = exactSittingPos.GetEdifice(pawn.Map);
		if (exactSittingPos.Impassable(pawn.Map) || exactSittingPos.IsForbidden(pawn))
		{
			return false;
		}
		for (int i = 0; i < 4; i++)
		{
			IntVec3 c = exactSittingPos + GenAdj.CardinalDirections[i];
			if (c.InBounds(pawn.Map))
			{
				Building edifice2 = c.GetEdifice(pawn.Map);
				if (edifice2 != null && edifice2 != ignoreThing && edifice2.def.hasInteractionCell && edifice2.InteractionCell == exactSittingPos && pawn.Map.reservationManager.TryGetReserver(edifice2, pawn.Faction, out var reserver) && reserver.Spawned && reserver != pawn)
				{
					return false;
				}
			}
		}
		if (edifice == null || edifice.def.building.multiSittable)
		{
			return pawn.CanReserve(exactSittingPos, 1, -1, null, ignoreOtherReservations);
		}
		if (edifice.def.building.isSittable && edifice.def.hasInteractionCell && exactSittingPos != edifice.InteractionCell)
		{
			return false;
		}
		return pawn.CanReserve(edifice, 1, -1, null, ignoreOtherReservations);
	}

	public static bool ReserveSittableOrSpot(this Pawn pawn, IntVec3 exactSittingPos, Job job, bool errorOnFailed = true)
	{
		Building edifice = exactSittingPos.GetEdifice(pawn.Map);
		if (exactSittingPos.Impassable(pawn.Map))
		{
			Log.Error("Tried reserving impassable sittable or spot.");
			return false;
		}
		if (edifice == null || edifice.def.building.multiSittable)
		{
			return pawn.Reserve(exactSittingPos, job, 1, -1, null, errorOnFailed);
		}
		if (edifice != null && edifice.def.building.isSittable && edifice.def.hasInteractionCell && exactSittingPos != edifice.InteractionCell)
		{
			return false;
		}
		return pawn.Reserve(edifice, job, 1, -1, null, errorOnFailed);
	}

	public static bool CanReserve(this Pawn p, LocalTargetInfo target, int maxPawns = 1, int stackCount = -1, ReservationLayerDef layer = null, bool ignoreOtherReservations = false)
	{
		if (!p.Spawned)
		{
			return false;
		}
		return p.Map.reservationManager.CanReserve(p, target, maxPawns, stackCount, layer, ignoreOtherReservations);
	}

	public static bool CanReserveAndReach(this Pawn p, LocalTargetInfo target, PathEndMode peMode, Danger maxDanger, int maxPawns = 1, int stackCount = -1, ReservationLayerDef layer = null, bool ignoreOtherReservations = false)
	{
		if (!p.Spawned)
		{
			return false;
		}
		if (p.CanReach(target, peMode, maxDanger))
		{
			return p.Map.reservationManager.CanReserve(p, target, maxPawns, stackCount, layer, ignoreOtherReservations);
		}
		return false;
	}

	public static bool TryFindReserveAndReachableOfDef(this Pawn p, ThingDef thingDef, out Thing thing, PathEndMode peMode = PathEndMode.Touch, Danger maxDanger = Danger.None, int maxPawns = 1, int stackCount = -1, ReservationLayerDef layer = null, bool ignoreOtherReservations = false)
	{
		thing = null;
		if (!p.Spawned)
		{
			return false;
		}
		thing = GenClosest.ClosestThingReachable(p.PositionHeld, p.MapHeld, ThingRequest.ForDef(thingDef), PathEndMode.OnCell, TraverseParms.For(p), 9999f, (Thing t) => !t.IsForbidden(p) && p.CanReserveAndReach(t, peMode, maxDanger, maxPawns, stackCount, layer, ignoreOtherReservations));
		return thing != null;
	}

	public static bool HasReserved(this Pawn pawn, ThingDef thingDef)
	{
		if (!pawn.Spawned)
		{
			return false;
		}
		return pawn.MapHeld.reservationManager.HasReservedOfDef(pawn, thingDef);
	}

	public static bool ExistsUnreservedAmountOfDef(Map map, ThingDef thingDef, Faction faction, int amount, Predicate<Thing> validator = null)
	{
		int num = 0;
		foreach (Thing item in map.listerThings.ThingsOfDef(thingDef))
		{
			if (!item.IsForbidden(faction) && !map.reservationManager.IsReservedByAnyoneOf(item, faction) && (validator == null || validator(item)))
			{
				num += item.stackCount;
				if (num >= amount)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool CanReserveAndReachableOfDef(this Pawn p, ThingDef thingDef, PathEndMode peMode = PathEndMode.Touch, Danger maxDanger = Danger.None, int maxPawns = 1, int stackCount = -1, ReservationLayerDef layer = null, bool ignoreOtherReservations = false)
	{
		Thing thing;
		return p.TryFindReserveAndReachableOfDef(thingDef, out thing, peMode, maxDanger, maxPawns, stackCount, layer, ignoreOtherReservations);
	}

	public static bool Reserve(this Pawn p, LocalTargetInfo target, Job job, int maxPawns = 1, int stackCount = -1, ReservationLayerDef layer = null, bool errorOnFailed = true, bool ignoreOtherReservations = false)
	{
		if (!p.Spawned)
		{
			return false;
		}
		return p.Map.reservationManager.Reserve(p, job, target, maxPawns, stackCount, layer, errorOnFailed, ignoreOtherReservations);
	}

	public static void ReserveAsManyAsPossible(this Pawn p, List<LocalTargetInfo> target, Job job, int maxPawns = 1, int stackCount = -1, ReservationLayerDef layer = null)
	{
		if (p.Spawned)
		{
			for (int i = 0; i < target.Count; i++)
			{
				p.Map.reservationManager.Reserve(p, job, target[i], maxPawns, stackCount, layer, errorOnFailed: false, ignoreOtherReservations: false, canReserversStartJobs: false);
			}
		}
	}

	public static bool HasReserved(this Pawn p, LocalTargetInfo target, Job job = null)
	{
		if (!p.Spawned)
		{
			return false;
		}
		return p.Map.reservationManager.ReservedBy(target, p, job);
	}

	public static bool HasReserved<TDriver>(this Pawn p, LocalTargetInfo target, LocalTargetInfo? targetAIsNot = null, LocalTargetInfo? targetBIsNot = null, LocalTargetInfo? targetCIsNot = null)
	{
		if (!p.Spawned)
		{
			return false;
		}
		return p.Map.reservationManager.ReservedBy<TDriver>(target, p, targetAIsNot, targetBIsNot, targetCIsNot);
	}

	public static bool CanReserveNew(this Pawn p, LocalTargetInfo target)
	{
		if (target.IsValid && !p.HasReserved(target))
		{
			return p.CanReserve(target);
		}
		return false;
	}
}
