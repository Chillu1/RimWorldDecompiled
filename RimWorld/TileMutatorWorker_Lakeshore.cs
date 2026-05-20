using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class TileMutatorWorker_Lakeshore : TileMutatorWorker_Coast
{
	protected override float MaxForDeepWater => 0.2f;

	protected override float MaxForShallowWater => 0.5f;

	protected override float MaxForSand => 0.55f;

	protected override FloatRange CoastOffset => new FloatRange(0.25f, 0.35f);

	public TileMutatorWorker_Lakeshore(TileMutatorDef def)
		: base(def)
	{
	}

	protected override TerrainDef DeepWaterTerrainAt(IntVec3 cell, Map map)
	{
		return MapGenUtility.DeepFreshWaterTerrainAt(cell, map);
	}

	protected override TerrainDef ShallowWaterTerrainAt(IntVec3 cell, Map map)
	{
		return MapGenUtility.ShallowFreshWaterTerrainAt(cell, map);
	}

	protected override TerrainDef BeachTerrainAt(IntVec3 cell, Map map)
	{
		return MapGenUtility.LakeshoreTerrainAt(cell, map);
	}

	protected override float GetCoastAngle(PlanetTile tile)
	{
		return Find.World.CoastAngleAt(tile, BiomeDefOf.Lake).Value;
	}
}
