using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class ListerBuildingWithTagInProximity
{
	public struct CellRequest : IEquatable<CellRequest>
	{
		public readonly IntVec3 cell;

		public readonly float radius;

		public readonly Thing forThing;

		public readonly string tag;

		public CellRequest(IntVec3 c, float r, string tag, Thing t = null)
		{
			cell = c;
			radius = r;
			this.tag = tag;
			forThing = t;
		}

		public bool Equals(CellRequest other)
		{
			if (cell.Equals(other.cell))
			{
				float num = radius;
				if (num.Equals(other.radius) && tag == other.tag)
				{
					return forThing == other.forThing;
				}
			}
			return false;
		}

		public override int GetHashCode()
		{
			int hashCode = cell.GetHashCode();
			int num = hashCode * 397;
			float num2 = radius;
			hashCode = num ^ num2.GetHashCode();
			if (forThing != null)
			{
				hashCode = (hashCode * 397) ^ forThing.GetHashCode();
			}
			return hashCode ^ tag.GetHashCode();
		}
	}

	private Map map;

	private Dictionary<CellRequest, List<Thing>> requestCache = new Dictionary<CellRequest, List<Thing>>();

	public ListerBuildingWithTagInProximity(Map map)
	{
		this.map = map;
	}

	public void Notify_BuildingSpawned(Building b)
	{
		requestCache.Clear();
	}

	public void Notify_BuildingDeSpawned(Building b)
	{
		requestCache.Clear();
	}

	public List<Thing> GetForCell(IntVec3 cell, float radius, string tag, Thing forThing = null)
	{
		CellRequest key = new CellRequest(cell, radius, tag, forThing);
		if (!requestCache.TryGetValue(key, out var value))
		{
			value = new List<Thing>();
			foreach (Thing item in GenRadial.RadialDistinctThingsAround(cell, map, radius, useCenter: false))
			{
				ThingDef thingDef = item.def;
				if ((item.def.IsBlueprint || item.def.isFrameInt) && item.def.entityDefToBuild is ThingDef thingDef2)
				{
					thingDef = thingDef2;
				}
				if (thingDef.building != null && thingDef.building.buildingTags.Contains(tag) && item.GetRoom() == cell.GetRoom(map) && item != forThing)
				{
					value.Add(item);
				}
			}
			value.SortBy((Thing t) => t.Position.DistanceTo(cell));
			requestCache[key] = value;
		}
		return value;
	}
}
