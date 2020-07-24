using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class SkyfallerUtility
	{
		public static bool CanPossiblyFallOnColonist(ThingDef skyfaller, IntVec3 c, Map map)
		{
			CellRect cellRect = GenAdj.OccupiedRect(c, Rot4.North, skyfaller.size);
			int dist = Mathf.Max(Mathf.CeilToInt(skyfaller.skyfaller.explosionRadius) + 7, 14);
			foreach (IntVec3 item in cellRect.ExpandedBy(dist))
			{
				if (!item.InBounds(map))
				{
					continue;
				}
				List<Thing> thingList = item.GetThingList(map);
				for (int i = 0; i < thingList.Count; i++)
				{
					Pawn pawn = thingList[i] as Pawn;
					if (pawn != null && pawn.IsColonist)
					{
						return true;
					}
				}
			}
			return false;
		}

		public static void MakeDropoffShuttle(Map map, List<Thing> contents, Faction faction = null)
		{
			if (!DropCellFinder.TryFindShipLandingArea(map, out IntVec3 result, out Thing firstBlockingThing))
			{
				if (firstBlockingThing != null)
				{
					Messages.Message("ShuttleBlocked".Translate("BlockedBy".Translate(firstBlockingThing).CapitalizeFirst()), firstBlockingThing, MessageTypeDefOf.NeutralEvent);
				}
				result = DropCellFinder.TryFindSafeLandingSpotCloseToColony(map, ThingDefOf.Shuttle.Size);
			}
			Thing thing = ThingMaker.MakeThing(ThingDefOf.Shuttle);
			thing.TryGetComp<CompShuttle>().dropEverythingOnArrival = true;
			for (int i = 0; i < contents.Count; i++)
			{
				Pawn p;
				if ((p = (contents[i] as Pawn)) != null)
				{
					Find.WorldPawns.RemovePawn(p);
				}
			}
			thing.SetFaction(faction);
			thing.TryGetComp<CompTransporter>().innerContainer.TryAddRangeOrTransfer(contents);
			GenPlace.TryPlaceThing(SkyfallerMaker.MakeSkyfaller(ThingDefOf.ShuttleIncoming, Gen.YieldSingle(thing)), result, map, ThingPlaceMode.Near);
		}
	}
}
