using System.Collections.Generic;
using Verse;
using Verse.Noise;

namespace RimWorld.Planet
{
	public class TileTemperaturesComp : WorldComponent
	{
		private class CachedTileTemperatureData
		{
			private int tile;

			private int tickCachesNeedReset = int.MinValue;

			private float cachedOutdoorTemp = float.MinValue;

			private float cachedSeasonalTemp = float.MinValue;

			private float[] twelfthlyTempAverages;

			private Perlin dailyVariationPerlinCached;

			private const int CachedTempUpdateInterval = 60;

			public CachedTileTemperatureData(int tile)
			{
				this.tile = tile;
				int seed = Gen.HashCombineInt(tile, 199372327);
				dailyVariationPerlinCached = new Perlin(4.9999998736893758E-06, 2.0, 0.5, 3, seed, QualityMode.Medium);
				twelfthlyTempAverages = new float[12];
				for (int i = 0; i < 12; i++)
				{
					twelfthlyTempAverages[i] = GenTemperature.AverageTemperatureAtTileForTwelfth(tile, (Twelfth)i);
				}
				CheckCache();
			}

			public float GetOutdoorTemp()
			{
				return cachedOutdoorTemp;
			}

			public float GetSeasonalTemp()
			{
				return cachedSeasonalTemp;
			}

			public float OutdoorTemperatureAt(int absTick)
			{
				return CalculateOutdoorTemperatureAtTile(absTick, includeDailyVariations: true);
			}

			public float OffsetFromDailyRandomVariation(int absTick)
			{
				return (float)dailyVariationPerlinCached.GetValue(absTick, 0.0, 0.0) * 7f;
			}

			public float AverageTemperatureForTwelfth(Twelfth twelfth)
			{
				return twelfthlyTempAverages[(uint)twelfth];
			}

			public void CheckCache()
			{
				if (tickCachesNeedReset <= Find.TickManager.TicksGame)
				{
					tickCachesNeedReset = Find.TickManager.TicksGame + 60;
					Map map = Current.Game.FindMap(tile);
					cachedOutdoorTemp = OutdoorTemperatureAt(Find.TickManager.TicksAbs);
					if (map != null)
					{
						cachedOutdoorTemp += map.gameConditionManager.AggregateTemperatureOffset();
					}
					cachedSeasonalTemp = CalculateOutdoorTemperatureAtTile(Find.TickManager.TicksAbs, includeDailyVariations: false);
				}
			}

			private float CalculateOutdoorTemperatureAtTile(int absTick, bool includeDailyVariations)
			{
				if (absTick == 0)
				{
					absTick = 1;
				}
				float num = Find.WorldGrid[tile].temperature + GenTemperature.OffsetFromSeasonCycle(absTick, tile);
				if (includeDailyVariations)
				{
					num += OffsetFromDailyRandomVariation(absTick) + GenTemperature.OffsetFromSunCycle(absTick, tile);
				}
				return num;
			}
		}

		private CachedTileTemperatureData[] cache;

		private List<int> usedSlots;

		public TileTemperaturesComp(World world)
			: base(world)
		{
			ClearCaches();
		}

		public override void WorldComponentTick()
		{
			for (int i = 0; i < usedSlots.Count; i++)
			{
				cache[usedSlots[i]].CheckCache();
			}
			if (Find.TickManager.TicksGame % 300 == 84 && usedSlots.Any())
			{
				cache[usedSlots[0]] = null;
				usedSlots.RemoveAt(0);
			}
		}

		public float GetOutdoorTemp(int tile)
		{
			return RetrieveCachedData(tile).GetOutdoorTemp();
		}

		public float GetSeasonalTemp(int tile)
		{
			return RetrieveCachedData(tile).GetSeasonalTemp();
		}

		public float OutdoorTemperatureAt(int tile, int absTick)
		{
			return RetrieveCachedData(tile).OutdoorTemperatureAt(absTick);
		}

		public float OffsetFromDailyRandomVariation(int tile, int absTick)
		{
			return RetrieveCachedData(tile).OffsetFromDailyRandomVariation(absTick);
		}

		public float AverageTemperatureForTwelfth(int tile, Twelfth twelfth)
		{
			return RetrieveCachedData(tile).AverageTemperatureForTwelfth(twelfth);
		}

		public bool SeasonAcceptableFor(int tile, ThingDef animalRace)
		{
			float seasonalTemp = GetSeasonalTemp(tile);
			if (seasonalTemp > animalRace.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin))
			{
				return seasonalTemp < animalRace.GetStatValueAbstract(StatDefOf.ComfyTemperatureMax);
			}
			return false;
		}

		public bool OutdoorTemperatureAcceptableFor(int tile, ThingDef animalRace)
		{
			float outdoorTemp = GetOutdoorTemp(tile);
			if (outdoorTemp > animalRace.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin))
			{
				return outdoorTemp < animalRace.GetStatValueAbstract(StatDefOf.ComfyTemperatureMax);
			}
			return false;
		}

		public bool SeasonAndOutdoorTemperatureAcceptableFor(int tile, ThingDef animalRace)
		{
			if (SeasonAcceptableFor(tile, animalRace))
			{
				return OutdoorTemperatureAcceptableFor(tile, animalRace);
			}
			return false;
		}

		public void ClearCaches()
		{
			cache = new CachedTileTemperatureData[Find.WorldGrid.TilesCount];
			usedSlots = new List<int>();
		}

		private CachedTileTemperatureData RetrieveCachedData(int tile)
		{
			if (cache[tile] != null)
			{
				return cache[tile];
			}
			cache[tile] = new CachedTileTemperatureData(tile);
			usedSlots.Add(tile);
			return cache[tile];
		}
	}
}
