using System.Collections.Generic;

namespace Verse
{
	public sealed class ZoneManager : IExposable
	{
		public Map map;

		private List<Zone> allZones = new List<Zone>();

		private Zone[] zoneGrid;

		public List<Zone> AllZones => allZones;

		public ZoneManager(Map map)
		{
			this.map = map;
			zoneGrid = new Zone[map.cellIndices.NumGridCells];
		}

		public void ExposeData()
		{
			Scribe_Collections.Look(ref allZones, "allZones", LookMode.Deep);
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				UpdateZoneManagerLinks();
				RebuildZoneGrid();
			}
		}

		private void UpdateZoneManagerLinks()
		{
			for (int i = 0; i < allZones.Count; i++)
			{
				allZones[i].zoneManager = this;
			}
		}

		private void RebuildZoneGrid()
		{
			CellIndices cellIndices = map.cellIndices;
			zoneGrid = new Zone[cellIndices.NumGridCells];
			foreach (Zone allZone in allZones)
			{
				foreach (IntVec3 item in allZone)
				{
					zoneGrid[cellIndices.CellToIndex(item)] = allZone;
				}
			}
		}

		public void RegisterZone(Zone newZone)
		{
			allZones.Add(newZone);
			newZone.PostRegister();
		}

		public void DeregisterZone(Zone oldZone)
		{
			allZones.Remove(oldZone);
			oldZone.PostDeregister();
		}

		internal void AddZoneGridCell(Zone zone, IntVec3 c)
		{
			zoneGrid[map.cellIndices.CellToIndex(c)] = zone;
		}

		internal void ClearZoneGridCell(IntVec3 c)
		{
			zoneGrid[map.cellIndices.CellToIndex(c)] = null;
		}

		public Zone ZoneAt(IntVec3 c)
		{
			return zoneGrid[map.cellIndices.CellToIndex(c)];
		}

		public string NewZoneName(string nameBase)
		{
			for (int i = 1; i <= 1000; i++)
			{
				string cand = nameBase + " " + i;
				if (!allZones.Any((Zone z) => z.label == cand))
				{
					return cand;
				}
			}
			Log.Error("Ran out of zone names.");
			return "Zone X";
		}

		internal void Notify_NoZoneOverlapThingSpawned(Thing thing)
		{
			CellRect cellRect = thing.OccupiedRect();
			for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
			{
				for (int j = cellRect.minX; j <= cellRect.maxX; j++)
				{
					IntVec3 c = new IntVec3(j, 0, i);
					Zone zone = ZoneAt(c);
					if (zone != null)
					{
						zone.RemoveCell(c);
						zone.CheckContiguous();
					}
				}
			}
		}
	}
}
