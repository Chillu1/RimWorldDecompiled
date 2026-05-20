using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet;

public class SurfaceTile : Tile
{
	public struct RoadLink
	{
		public PlanetTile neighbor;

		public RoadDef road;
	}

	public struct RiverLink
	{
		public PlanetTile neighbor;

		public RiverDef river;
	}

	public List<RoadLink> potentialRoads;

	public List<RiverLink> potentialRivers;

	public int riverDist;

	public override bool WaterCovered => elevation <= 0f;

	public List<RoadLink> Roads
	{
		get
		{
			if (!base.PrimaryBiome.allowRoads)
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
			if (!base.PrimaryBiome.allowRivers)
			{
				return null;
			}
			return potentialRivers;
		}
	}

	public SurfaceTile()
	{
	}

	public SurfaceTile(PlanetTile tile)
		: base(tile)
	{
	}

	public override string ToString()
	{
		return $"({base.PrimaryBiome} elev={elevation}m hill={hilliness} temp={temperature}°C rain={rainfall}mm" + $" swampiness={swampiness.ToStringPercent()} potentialRoads={potentialRoads?.Count ?? 0} (allowed={base.PrimaryBiome.allowRoads})" + $" potentialRivers={potentialRivers?.Count ?? 0} (allowed={base.PrimaryBiome.allowRivers}))";
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref riverDist, "riverDist", 0);
	}
}
