using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.Planet
{
	public static class CaravanUtility
	{
		public static bool IsOwner(Pawn pawn, Faction caravanFaction)
		{
			if (caravanFaction == null)
			{
				Log.Warning("Called IsOwner with null faction.");
				return false;
			}
			if (!pawn.NonHumanlikeOrWildMan() && pawn.Faction == caravanFaction)
			{
				return pawn.HostFaction == null;
			}
			return false;
		}

		public static Caravan GetCaravan(this Pawn pawn)
		{
			return pawn.ParentHolder as Caravan;
		}

		public static bool IsCaravanMember(this Pawn pawn)
		{
			return pawn.GetCaravan() != null;
		}

		public static bool IsPlayerControlledCaravanMember(this Pawn pawn)
		{
			return pawn.GetCaravan()?.IsPlayerControlled ?? false;
		}

		public static int BestGotoDestNear(int tile, Caravan c)
		{
			Predicate<int> predicate = delegate(int t)
			{
				if (Find.World.Impassable(t))
				{
					return false;
				}
				return c.CanReach(t) ? true : false;
			};
			if (predicate(tile))
			{
				return tile;
			}
			GenWorldClosest.TryFindClosestTile(tile, predicate, out int foundTile, 50);
			return foundTile;
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
	}
}
