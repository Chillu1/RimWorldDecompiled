using Verse;

namespace RimWorld;

public class TileMutatorWorker_DryLake : TileMutatorWorker_Lake
{
	protected override float LakeRadius => 0.6f;

	protected override bool GenerateDeepWater => false;

	public TileMutatorWorker_DryLake(TileMutatorDef def)
		: base(def)
	{
	}

	protected override void ProcessCell(IntVec3 cell, Map map)
	{
		float valAt = GetValAt(cell, map);
		if (valAt > 0.5f)
		{
			map.terrainGrid.SetTerrain(cell, TerrainDefOf.DryLakeBed);
		}
		else if (valAt > 0.45f)
		{
			map.terrainGrid.SetTerrain(cell, TerrainDefOf.Sand);
		}
	}
}
