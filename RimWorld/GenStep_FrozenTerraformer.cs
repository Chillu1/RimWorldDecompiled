using System.Collections.Generic;
using RimWorld.BaseGen;
using Verse;
using Verse.Noise;

namespace RimWorld;

public class GenStep_FrozenTerraformer : GenStep_BaseRuins
{
	private ModuleBase iceNoise;

	private const float NoiseFrequency = 0.01f;

	private const float FalloffStrength = 0.7f;

	private const float IceRadius = 0.3f;

	private const float IceThreshold = 0.5f;

	private const float IceTerrainThreshold = 0.4f;

	private const int BuildingSize = 40;

	private static readonly SimpleCurve SentryCountFromPointsCurve = new SimpleCurve(new CurvePoint[4]
	{
		new CurvePoint(0f, 0f),
		new CurvePoint(100f, 2f),
		new CurvePoint(1000f, 8f),
		new CurvePoint(5000f, 16f)
	});

	public override int SeedPart => 9871234;

	protected override LayoutDef LayoutDef => LayoutDefOf.AncientRuinsFrozenTerraformer;

	protected override Faction Faction => null;

	public override void Generate(Map map, GenStepParams parms)
	{
		if (!ModLister.CheckOdyssey("Frozen Terraformer"))
		{
			return;
		}
		iceNoise = new Perlin(0.009999999776482582, 2.0, 0.5, 6, Rand.Int, QualityMode.Medium);
		ModuleBase input = new DistFromPoint((float)map.Size.x * 0.3f * 2f);
		input = new ScaleBias(-1.0, 1.0, input);
		input = new Translate(-map.Center.x, 0.0, -map.Center.z, input);
		iceNoise = new Blend(iceNoise, input, new Const(0.699999988079071));
		MapGenerator.GetOrGenerateVar<List<CellRect>>("UsedRects");
		foreach (IntVec3 allCell in map.AllCells)
		{
			if (allCell.GetEdifice(map) == null)
			{
				float value = iceNoise.GetValue(allCell);
				if (value > 0.4f)
				{
					map.terrainGrid.SetTerrain(allCell, TerrainDefOf.Ice);
				}
				if (value > 0.5f)
				{
					GenSpawn.Spawn(ThingDefOf.SolidIce, allCell, map);
				}
			}
		}
		CellRect cellRect = CellRect.CenteredOn(map.Center, 20);
		GenerateAndSpawn(cellRect, map, parms, LayoutDef);
		MapGenerator.SetVar("SpawnRect", cellRect);
	}

	public override void PostMapInitialized(Map map, GenStepParams parms)
	{
		BaseGenUtility.ScatterSentryDronesInMap(SentryCountFromPointsCurve, map, Faction.OfAncientsHostile, parms);
	}
}
