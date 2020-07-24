using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public class Tile
	{
		public struct RoadLink
		{
			public int neighbor;

			public RoadDef road;
		}

		public struct RiverLink
		{
			public int neighbor;

			public RiverDef river;
		}

		public const int Invalid = -1;

		public BiomeDef biome;

		public float elevation = 100f;

		public Hilliness hilliness;

		public float temperature = 20f;

		public float rainfall;

		public float swampiness;

		public WorldFeature feature;

		public List<RoadLink> potentialRoads;

		public List<RiverLink> potentialRivers;

		public bool WaterCovered => elevation <= 0f;

		public List<RoadLink> Roads
		{
			get
			{
				if (!biome.allowRoads)
				{
					return null;
				}
				return potentialRoads;
			}
		}

		public List<RiverLink> Rivers
		{
			get
			{
				if (!biome.allowRivers)
				{
					return null;
				}
				return potentialRivers;
			}
		}

		public override string ToString()
		{
			return string.Concat("(", biome, " elev=", elevation, "m hill=", hilliness, " temp=", temperature, "°C rain=", rainfall, "mm swampiness=", swampiness.ToStringPercent(), " potentialRoads=", (potentialRoads != null) ? potentialRoads.Count : 0, " (allowed=", biome.allowRoads.ToString(), ") potentialRivers=", (potentialRivers != null) ? potentialRivers.Count : 0, " (allowed=", biome.allowRivers.ToString(), "))");
		}
	}
}
