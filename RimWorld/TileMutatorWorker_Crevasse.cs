using Verse;
using Verse.Noise;

namespace RimWorld;

public class TileMutatorWorker_Crevasse : TileMutatorWorker
{
	private ModuleBase crevasse;

	private const float CrevasseSpan = 0.15f;

	private const float CrevasseThreshold = 0.5f;

	private const float CrevasseNoiseFreq = 0.015f;

	private const float CrevasseNoiseStrength = 20f;

	public TileMutatorWorker_Crevasse(TileMutatorDef def)
		: base(def)
	{
	}

	public override void Init(Map map)
	{
		if (ModsConfig.OdysseyActive)
		{
			base.Init(map);
			float num = Rand.Range(0f, 180f);
			crevasse = new DistFromAxis((float)map.Size.x * 0.15f);
			crevasse = new Rotate(0.0, num, 0.0, crevasse);
			crevasse = new Translate((float)(-map.Size.x) / 2f, 0.0, (float)(-map.Size.z) / 2f, crevasse);
			crevasse = MapNoiseUtility.AddDisplacementNoise(crevasse, 0.015f, 20f);
			NoiseDebugUI.StoreNoiseRender(crevasse, "crevasse");
		}
	}

	public override void GeneratePostElevationFertility(Map map)
	{
		MapGenFloatGrid elevation = MapGenerator.Elevation;
		foreach (IntVec3 allCell in map.AllCells)
		{
			if (crevasse.GetValue(allCell) <= 0.5f)
			{
				elevation[allCell] = 0f;
			}
		}
	}

	public override void GeneratePostTerrain(Map map)
	{
		foreach (IntVec3 allCell in map.AllCells)
		{
			float value = crevasse.GetValue(allCell);
			if (!(value <= 0.5f))
			{
				if (value > 0.798f)
				{
					map.roofGrid.SetRoof(allCell, RoofDefOf.RoofRockThick);
				}
				else if (value > 0.728f)
				{
					map.roofGrid.SetRoof(allCell, RoofDefOf.RoofRockThin);
				}
				if (allCell.GetEdifice(map) == null)
				{
					GenSpawn.Spawn(ThingDefOf.SolidIce, allCell, map);
					map.terrainGrid.SetTerrain(allCell, TerrainDefOf.Ice);
				}
			}
		}
	}
}
