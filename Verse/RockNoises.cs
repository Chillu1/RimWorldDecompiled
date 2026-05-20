using System.Collections.Generic;
using Verse.Noise;

namespace Verse;

public static class RockNoises
{
	public class RockNoise
	{
		public ThingDef rockDef;

		public ModuleBase noise;
	}

	public static List<RockNoise> rockNoises;

	private const float RockNoiseFreq = 0.005f;

	public static void Init(Map map)
	{
		rockNoises = new List<RockNoise>();
		foreach (ThingDef item in Find.World.NaturalRockTypesIn(map.Tile))
		{
			RockNoise rockNoise = new RockNoise();
			rockNoise.rockDef = item;
			rockNoise.noise = new Perlin(0.004999999888241291, 2.0, 0.5, 6, Rand.Range(0, int.MaxValue), QualityMode.Medium);
			rockNoises.Add(rockNoise);
			NoiseDebugUI.StoreNoiseRender(rockNoise.noise, rockNoise.rockDef?.ToString() + " score", map.Size.ToIntVec2);
		}
	}

	public static void Reset()
	{
		rockNoises = null;
	}
}
