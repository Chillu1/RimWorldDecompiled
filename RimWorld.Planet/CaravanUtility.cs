using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.Planet;

public static class CaravanUtility
{
	public static bool IsOwner(Pawn pawn, Faction caravanFaction)
	{
		if (caravanFaction == null)
		{
			Log.Warning("Called IsOwner with null faction.");
			return false;
		}
		if (!pawn.NonHumanlikeOrWildMan() && pawn.Faction == caravanFaction && pawn.HostFaction == null)
		{
			return !pawn.IsSlave;
		}
		return false;
	}

	public static bool IsCaravanMember(this Pawn pawn)
	{
		return pawn.GetCaravan() != null;
	}

	public static bool IsPlayerControlledCaravanMember(this Pawn pawn)
	{
		return pawn.GetCaravan()?.IsPlayerControlled ?? false;
	}

	public static PlanetTile BestGotoDestNear(PlanetTile tile, Caravan c)
	{
		if (IsGoodDest(tile))
		{
			return tile;
		}
		GenWorldClosest.TryFindClosestTile(tile, IsGoodDest, out var foundTile, 50);
		return foundTile;
		bool IsGoodDest(PlanetTile t)
		{
			if (Find.World.Impassable(t))
			{
				return false;
			}
			if (!c.CanReach(t))
			{
				return false;
			}
			return true;
		}
	}

	public static bool PlayerHasAnyCaravan()
	{
		List<Caravan> caravans = Find.WorldObjects.Caravans;
		for (int i = 0; i < caravans.Count; i++)
		{
			if (caravans[i].IsPlayerControlled)
			{
				return true;
			}
		}
		return false;
	}

	public static Pawn RandomOwner(this Caravan caravan)
	{
		return caravan.PawnsListForReading.Where((Pawn p) => caravan.IsOwner(p)).RandomElement();
	}

	public static bool ShouldAutoCapture(Pawn p, Faction caravanFaction)
	{
		if (p.RaceProps.Humanlike && !p.Dead && p.Faction != caravanFaction)
		{
			if (p.IsPrisoner)
			{
				return p.HostFaction != caravanFaction;
			}
			return true;
		}
		return false;
	}

	public static PlanetTile GetTileCurrentlyOver(this Caravan caravan)
	{
		if (caravan.pather.Moving && caravan.pather.IsNextTilePassable() && 1f - caravan.pather.nextTileCostLeft / caravan.pather.nextTileCostTotal > 0.5f)
		{
			return caravan.pather.nextTile;
		}
		return caravan.Tile;
	}

	public static Caravan GetCaravan(this Thing thing)
	{
		if (thing.ParentHolder is Caravan result)
		{
			return result;
		}
		if (thing.ParentHolder is Pawn_InventoryTracker pawn_InventoryTracker)
		{
			return pawn_InventoryTracker.pawn.GetCaravan();
		}
		if (thing.ParentHolder is Thing thing2)
		{
			return thing2.GetCaravan();
		}
		return null;
	}

	public static bool IsInCaravan(this Thing thing)
	{
		return thing.GetCaravan() != null;
	}
}
