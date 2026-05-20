using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ListerBuldingOfDefInProximity
	{
		public struct CellRequest : IEquatable<CellRequest>
		{
			public readonly IntVec3 cell;

			public readonly float radius;

			public readonly Thing forThing;

			public readonly ThingDef def;

			public readonly List<MeditationFocusOffsetPerBuilding> defs;

			public CellRequest(IntVec3 c, float r, List<MeditationFocusOffsetPerBuilding> d, Thing t = null)
			{
				cell = c;
				radius = r;
				defs = d;
				forThing = t;
				def = null;
			}

			public CellRequest(IntVec3 c, float r, ThingDef d, Thing t = null)
			{
				cell = c;
				radius = r;
				def = d;
				defs = null;
				forThing = t;
			}

			public bool Equals(CellRequest other)
			{
				if (cell.Equals(other.cell))
				{
					float num = radius;
					if (num.Equals(other.radius) && GenCollection.ListsEqual(defs, other.defs) && def == other.def)
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
				if (defs != null)
				{
					for (int i = 0; i < defs.Count; i++)
					{
						hashCode ^= defs[i].GetHashCode();
					}
				}
				if (def != null)
				{
					hashCode ^= def.GetHashCode();
				}
				return hashCode;
			}
		}

		private Map map;

		private Dictionary<CellRequest, List<Thing>> requestCache = new Dictionary<CellRequest, List<Thing>>();

		public ListerBuldingOfDefInProximity(Map map)
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

		public List<Thing> GetForCell(IntVec3 cell, float radius, List<MeditationFocusOffsetPerBuilding> defs, Thing forThing = null)
		{
			CellRequest key = new CellRequest(cell, radius, defs, forThing);
			if (!requestCache.TryGetValue(key, out var value))
			{
				value = new List<Thing>();
				foreach (Thing t in GenRadial.RadialDistinctThingsAround(cell, map, radius, useCenter: false))
				{
					if (defs.Any((MeditationFocusOffsetPerBuilding d) => d.building == t.def) && t.GetRoom() == cell.GetRoom(map) && t != forThing)
					{
						value.Add(t);
					}
				}
				value.SortBy(delegate(Thing thing)
				{
					float num = thing.Position.DistanceTo(cell);
					MeditationFocusOffsetPerBuilding meditationFocusOffsetPerBuilding = defs.FirstOrDefault((MeditationFocusOffsetPerBuilding d) => d.building == thing.def);
					if (meditationFocusOffsetPerBuilding != null)
					{
						num -= meditationFocusOffsetPerBuilding.offset * 100000f;
					}
					return num;
				});
				requestCache[key] = value;
			}
			return value;
		}

		public List<Thing> GetForCell(IntVec3 cell, float radius, ThingDef def, Thing forThing = null)
		{
			CellRequest key = new CellRequest(cell, radius, def, forThing);
			if (!requestCache.TryGetValue(key, out var value))
			{
				value = new List<Thing>();
				foreach (Thing item in GenRadial.RadialDistinctThingsAround(cell, map, radius, useCenter: false))
				{
					if (def == item.def && item.GetRoom() == cell.GetRoom(map) && item != forThing)
					{
						value.Add(item);
					}
				}
				requestCache[key] = value;
			}
			return value;
		}
	}
}
