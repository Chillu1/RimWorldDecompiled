using Verse;
using Verse.Noise;

namespace RimWorld
{
	public static class BeachMaker
	{
		private static ModuleBase beachNoise;

		private const float PerlinFrequency = 0.03f;

		private const float MaxForDeepWater = 0.1f;

		private const float MaxForShallowWater = 0.45f;

		private const float MaxForSand = 1f;

		private static readonly FloatRange CoastWidthRange = new FloatRange(20f, 60f);

		public static void Init(Map map)
		{
			Rot4 a = Find.World.CoastDirectionAt(map.Tile);
			if (!a.IsValid)
			{
				beachNoise = null;
				return;
			}
			ModuleBase input = new Perlin(0.029999999329447746, 2.0, 0.5, 3, Rand.Range(0, int.MaxValue), QualityMode.Medium);
			input = new ScaleBias(0.5, 0.5, input);
			NoiseDebugUI.StoreNoiseRender(input, "BeachMaker base", map.Size.ToIntVec2);
			ModuleBase input2 = new DistFromAxis(CoastWidthRange.RandomInRange);
			if (a == Rot4.North)
			{
				input2 = new Rotate(0.0, 90.0, 0.0, input2);
				input2 = new Translate(0.0, 0.0, -map.Size.z, input2);
			}
			else if (a == Rot4.East)
			{
				input2 = new Translate(-map.Size.x, 0.0, 0.0, input2);
			}
			else if (a == Rot4.South)
			{
				input2 = new Rotate(0.0, 90.0, 0.0, input2);
			}
			input2 = new ScaleBias(1.0, -1.0, input2);
			input2 = new Clamp(-1.0, 2.5, input2);
			NoiseDebugUI.StoreNoiseRender(input2, "BeachMaker axis bias");
			beachNoise = new Add(input, input2);
			NoiseDebugUI.StoreNoiseRender(beachNoise, "beachNoise");
		}

		public static void Cleanup()
		{
			beachNoise = null;
		}

		public static TerrainDef BeachTerrainAt(IntVec3 loc, BiomeDef biome)
		{
			if (beachNoise == null)
			{
				return null;
			}
			float value = beachNoise.GetValue(loc);
			if (value < 0.1f)
			{
				return TerrainDefOf.WaterOceanDeep;
			}
			if (value < 0.45f)
			{
				return TerrainDefOf.WaterOceanShallow;
			}
			if (value < 1f)
			{
				if (biome != BiomeDefOf.SeaIce)
				{
					return TerrainDefOf.Sand;
				}
				return TerrainDefOf.Ice;
			}
			return null;
		}
	}
}
