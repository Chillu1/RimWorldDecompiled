using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace RimWorld
{
	public class BiomeWorker_SeaIce : BiomeWorker
	{
		private ModuleBase cachedSeaIceAllowedNoise;

		private int cachedSeaIceAllowedNoiseForSeed;

		public override float GetScore(Tile tile, int tileID)
		{
			if (!tile.WaterCovered)
			{
				return -100f;
			}
			if (!AllowedAt(tileID))
			{
				return -100f;
			}
			return BiomeWorker_IceSheet.PermaIceScore(tile) - 23f;
		}

		private bool AllowedAt(int tile)
		{
			Vector3 tileCenter = Find.WorldGrid.GetTileCenter(tile);
			Vector3 viewCenter = Find.WorldGrid.viewCenter;
			float value = Vector3.Angle(viewCenter, tileCenter);
			float viewAngle = Find.WorldGrid.viewAngle;
			float num = Mathf.Min(7.5f, viewAngle * 0.12f);
			float num2 = Mathf.InverseLerp(viewAngle - num, viewAngle, value);
			if (num2 <= 0f)
			{
				return true;
			}
			if (cachedSeaIceAllowedNoise == null || cachedSeaIceAllowedNoiseForSeed != Find.World.info.Seed)
			{
				cachedSeaIceAllowedNoise = new Perlin(0.017000000923871994, 2.0, 0.5, 6, Find.World.info.Seed, QualityMode.Medium);
				cachedSeaIceAllowedNoiseForSeed = Find.World.info.Seed;
			}
			float headingFromTo = Find.WorldGrid.GetHeadingFromTo(viewCenter, tileCenter);
			float num3 = (float)cachedSeaIceAllowedNoise.GetValue(headingFromTo, 0.0, 0.0) * 0.5f + 0.5f;
			return num2 <= num3;
		}
	}
}
