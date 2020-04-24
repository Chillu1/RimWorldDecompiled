using System;
using System.Collections.Generic;
using Verse;
using Verse.Noise;

namespace RimWorld
{
	public class TerrainPatchMaker
	{
		private Map currentlyInitializedForMap;

		public List<TerrainThreshold> thresholds = new List<TerrainThreshold>();

		public float perlinFrequency = 0.01f;

		public float perlinLacunarity = 2f;

		public float perlinPersistence = 0.5f;

		public int perlinOctaves = 6;

		public float minFertility = -999f;

		public float maxFertility = 999f;

		public int minSize;

		[Unsaved(false)]
		private ModuleBase noise;

		private void Init(Map map)
		{
			noise = new Perlin(perlinFrequency, perlinLacunarity, perlinPersistence, perlinOctaves, Rand.Range(0, int.MaxValue), QualityMode.Medium);
			NoiseDebugUI.RenderSize = new IntVec2(map.Size.x, map.Size.z);
			NoiseDebugUI.StoreNoiseRender(noise, "TerrainPatchMaker " + thresholds[0].terrain.defName);
			currentlyInitializedForMap = map;
		}

		public void Cleanup()
		{
			noise = null;
			currentlyInitializedForMap = null;
		}

		public TerrainDef TerrainAt(IntVec3 c, Map map, float fertility)
		{
			if (fertility < minFertility || fertility > maxFertility)
			{
				return null;
			}
			if (noise != null && map != currentlyInitializedForMap)
			{
				Cleanup();
			}
			if (noise == null)
			{
				Init(map);
			}
			if (minSize > 0)
			{
				int count = 0;
				map.floodFiller.FloodFill(c, (Predicate<IntVec3>)((IntVec3 x) => TerrainThreshold.TerrainAtValue(thresholds, noise.GetValue(x)) != null), (Func<IntVec3, bool>)delegate
				{
					count++;
					return count >= minSize;
				}, int.MaxValue, rememberParents: false, (IEnumerable<IntVec3>)null);
				if (count < minSize)
				{
					return null;
				}
			}
			return TerrainThreshold.TerrainAtValue(thresholds, noise.GetValue(c));
		}
	}
}
