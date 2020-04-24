using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class KidnapAIUtility
	{
		public static bool TryFindGoodKidnapVictim(Pawn kidnapper, float maxDist, out Pawn victim, List<Thing> disallowed = null)
		{
			if (!kidnapper.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) || !kidnapper.Map.reachability.CanReachMapEdge(kidnapper.Position, TraverseParms.For(kidnapper, Danger.Some)))
			{
				victim = null;
				return false;
			}
			Predicate<Thing> validator = delegate(Thing t)
			{
				Pawn pawn = t as Pawn;
				if (!pawn.RaceProps.Humanlike)
				{
					return false;
				}
				if (!pawn.Downed)
				{
					return false;
				}
				if (pawn.Faction != Faction.OfPlayer)
				{
					return false;
				}
				if (!pawn.Faction.HostileTo(kidnapper.Faction))
				{
					return false;
				}
				if (!kidnapper.CanReserve(pawn))
				{
					return false;
				}
				return (disallowed == null || !disallowed.Contains(pawn)) ? true : false;
			};
			victim = (Pawn)GenClosest.ClosestThingReachable(kidnapper.Position, kidnapper.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.OnCell, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Some), maxDist, validator);
			return victim != null;
		}

		public static Pawn ReachableWoundedGuest(Pawn searcher)
		{
			List<Pawn> list = searcher.Map.mapPawns.SpawnedPawnsInFaction(searcher.Faction);
			for (int i = 0; i < list.Count; i++)
			{
				Pawn pawn = list[i];
				if (pawn.guest != null && !pawn.IsPrisoner && pawn.Downed && searcher.CanReserveAndReach(pawn, PathEndMode.OnCell, Danger.Some))
				{
					return pawn;
				}
			}
			return null;
		}
	}
}
