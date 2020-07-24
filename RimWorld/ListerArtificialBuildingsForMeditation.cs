using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ListerArtificialBuildingsForMeditation
	{
		private Map map;

		private List<Thing> artificialBuildings = new List<Thing>();

		private Dictionary<CellWithRadius, List<Thing>> artificialBuildingsPerCell = new Dictionary<CellWithRadius, List<Thing>>();

		public ListerArtificialBuildingsForMeditation(Map map)
		{
			this.map = map;
		}

		public void Notify_BuildingSpawned(Building b)
		{
			if (MeditationUtility.CountsAsArtificialBuilding(b))
			{
				artificialBuildings.Add(b);
				artificialBuildingsPerCell.Clear();
			}
		}

		public void Notify_BuildingDeSpawned(Building b)
		{
			if (MeditationUtility.CountsAsArtificialBuilding(b))
			{
				artificialBuildings.Remove(b);
				artificialBuildingsPerCell.Clear();
			}
		}

		public List<Thing> GetForCell(IntVec3 cell, float radius)
		{
			CellWithRadius key = new CellWithRadius(cell, radius);
			if (!artificialBuildingsPerCell.TryGetValue(key, out List<Thing> value))
			{
				value = new List<Thing>();
				foreach (Thing item in GenRadial.RadialDistinctThingsAround(cell, map, radius, useCenter: false))
				{
					if (MeditationUtility.CountsAsArtificialBuilding(item))
					{
						value.Add(item);
					}
				}
				artificialBuildingsPerCell[key] = value;
			}
			return value;
		}
	}
}
