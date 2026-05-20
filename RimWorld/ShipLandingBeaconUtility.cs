using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class ShipLandingBeaconUtility
{
	public static List<ShipLandingArea> tmpShipLandingAreas = new List<ShipLandingArea>();

	public static List<ShipLandingArea> GetLandingZones(Map map)
	{
		tmpShipLandingAreas.Clear();
		if (!ModsConfig.RoyaltyActive)
		{
			return tmpShipLandingAreas;
		}
		foreach (Thing item in map.listerThings.ThingsOfDef(ThingDefOf.ShipLandingBeacon))
		{
			CompShipLandingBeacon compShipLandingBeacon = item.TryGetComp<CompShipLandingBeacon>();
			if (compShipLandingBeacon == null || item.Faction != Faction.OfPlayer)
			{
				continue;
			}
			foreach (ShipLandingArea landingArea in compShipLandingBeacon.LandingAreas)
			{
				if (landingArea.Active && !tmpShipLandingAreas.Contains(landingArea))
				{
					landingArea.RecalculateBlockingThing();
					tmpShipLandingAreas.Add(landingArea);
				}
			}
		}
		return tmpShipLandingAreas;
	}

	public static void DrawLinesToNearbyBeacons(ThingDef myDef, IntVec3 myPos, Rot4 myRot, Map map, Thing thing = null)
	{
		Vector3 a = GenThing.TrueCenter(myPos, myRot, myDef.size, myDef.Altitude);
		foreach (Thing item in map.listerThings.ThingsOfDef(ThingDefOf.ShipLandingBeacon))
		{
			if ((thing == null || thing != item) && item.Faction == Faction.OfPlayer)
			{
				CompShipLandingBeacon compShipLandingBeacon = item.TryGetComp<CompShipLandingBeacon>();
				if (compShipLandingBeacon != null && CanLinkTo(myPos, compShipLandingBeacon) && !GenThing.CloserThingBetween(myDef, myPos, item.Position, map))
				{
					GenDraw.DrawLineBetween(a, item.TrueCenter(), SimpleColor.White);
				}
			}
		}
		float minEdgeDistance = myDef.GetCompProperties<CompProperties_ShipLandingBeacon>().edgeLengthRange.min - 1f;
		float maxEdgeDistance = myDef.GetCompProperties<CompProperties_ShipLandingBeacon>().edgeLengthRange.max - 1f;
		foreach (Thing item2 in map.listerThings.ThingsInGroup(ThingRequestGroup.Blueprint))
		{
			if ((thing == null || thing != item2) && item2.def.entityDefToBuild == myDef && (myPos.x == item2.Position.x || myPos.z == item2.Position.z) && !AlignedDistanceTooShort(myPos, item2.Position, minEdgeDistance) && !AlignedDistanceTooLong(myPos, item2.Position, maxEdgeDistance) && !GenThing.CloserThingBetween(myDef, myPos, item2.Position, map))
			{
				GenDraw.DrawLineBetween(a, item2.TrueCenter(), SimpleColor.White);
			}
		}
	}

	public static bool AlignedDistanceTooShort(IntVec3 position, IntVec3 otherPos, float minEdgeDistance)
	{
		if (position.x == otherPos.x)
		{
			return (float)Mathf.Abs(position.z - otherPos.z) < minEdgeDistance;
		}
		if (position.z == otherPos.z)
		{
			return (float)Mathf.Abs(position.x - otherPos.x) < minEdgeDistance;
		}
		return false;
	}

	private static bool AlignedDistanceTooLong(IntVec3 position, IntVec3 otherPos, float maxEdgeDistance)
	{
		if (position.x == otherPos.x)
		{
			return (float)Mathf.Abs(position.z - otherPos.z) >= maxEdgeDistance;
		}
		if (position.z == otherPos.z)
		{
			return (float)Mathf.Abs(position.x - otherPos.x) >= maxEdgeDistance;
		}
		return false;
	}

	public static bool CanLinkTo(IntVec3 position, CompShipLandingBeacon other)
	{
		if (position.x == other.parent.Position.x)
		{
			return other.parent.def.displayNumbersBetweenSameDefDistRange.Includes(Mathf.Abs(position.z - other.parent.Position.z) + 1);
		}
		if (position.z == other.parent.Position.z)
		{
			return other.parent.def.displayNumbersBetweenSameDefDistRange.Includes(Mathf.Abs(position.x - other.parent.Position.x) + 1);
		}
		return false;
	}
}
