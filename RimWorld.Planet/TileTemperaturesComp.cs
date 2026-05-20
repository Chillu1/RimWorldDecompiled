using System.Collections.Generic;
using Verse;
using Verse.Noise;

namespace RimWorld.Planet;

public class TileTemperaturesComp : WorldComponent
{
	private class CachedTileTemperatureData
	{
		private readonly PlanetTile tile;

		private int tickCachesNeedReset = int.MinValue;

		private float cachedOutdoorTemp = float.MinValue;

		private float cachedSeasonalTemp = float.MinValue;

		private float[] twelfthlyTempAverages;

		private Perlin dailyVariationPerlinCached;

		private const int CachedTempUpdateInterval = 60;

		public CachedTileTemperatureData(PlanetTile tile)
		{
			this.tile = tile;
			int seed = Gen.HashCombineInt(tile.GetHashCode(), 199372327);
			dailyVariationPerlinCached = new Perlin(4.999999873689376E-06, 2.0, 0.5, 3, seed, QualityMode.Medium);
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

	private Dictionary<PlanetLayerDef, CachedTileTemperatureData[]> cache;

	private Dictionary<PlanetLayerDef, List<int>> usedSlots;

	public TileTemperaturesComp(World world)
		: base(world)
	{
		ClearCaches();
	}

	public override void WorldComponentTick()
	{
		PlanetLayerDef key;
		List<int> value;
		foreach (KeyValuePair<PlanetLayerDef, List<int>> usedSlot in usedSlots)
		{
			usedSlot.Deconstruct(out key, out value);
			PlanetLayerDef key2 = key;
			foreach (int item in value)
			{
				cache[key2][item].CheckCache();
			}
		}
		if (Find.TickManager.TicksGame % 300 != 84)
		{
			return;
		}
		foreach (KeyValuePair<PlanetLayerDef, List<int>> usedSlot2 in usedSlots)
		{
			usedSlot2.Deconstruct(out key, out value);
			PlanetLayerDef key3 = key;
			List<int> list = value;
			if (list.Any())
			{
				cache[key3][list[0]] = null;
				list.RemoveAt(0);
			}
		}
	}

	public float GetOutdoorTemp(PlanetTile tile)
	{
		return RetrieveCachedData(tile).GetOutdoorTemp();
	}

	public float GetSeasonalTemp(PlanetTile tile)
	{
		return RetrieveCachedData(tile).GetSeasonalTemp();
	}

	public float OutdoorTemperatureAt(PlanetTile tile, int absTick)
	{
		return RetrieveCachedData(tile).OutdoorTemperatureAt(absTick);
	}

	public float OffsetFromDailyRandomVariation(PlanetTile tile, int absTick)
	{
		return RetrieveCachedData(tile).OffsetFromDailyRandomVariation(absTick);
	}

	public float AverageTemperatureForTwelfth(PlanetTile tile, Twelfth twelfth)
	{
		return RetrieveCachedData(tile).AverageTemperatureForTwelfth(twelfth);
	}

	public bool SeasonAcceptableFor(PlanetTile tile, ThingDef animalRace)
	{
		if (!tile.Valid)
		{
			return true;
		}
		float seasonalTemp = GetSeasonalTemp(tile);
		if (seasonalTemp > animalRace.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin))
		{
			return seasonalTemp < animalRace.GetStatValueAbstract(StatDefOf.ComfyTemperatureMax);
		}
		return false;
	}

	public bool OutdoorTemperatureAcceptableFor(PlanetTile tile, ThingDef animalRace)
	{
		if (!tile.Valid)
		{
			return true;
		}
		float outdoorTemp = GetOutdoorTemp(tile);
		if (outdoorTemp > animalRace.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin))
		{
			return outdoorTemp < animalRace.GetStatValueAbstract(StatDefOf.ComfyTemperatureMax);
		}
		return false;
	}

	public bool SeasonAndOutdoorTemperatureAcceptableFor(PlanetTile tile, ThingDef animalRace)
	{
		if (SeasonAcceptableFor(tile, animalRace))
		{
			return OutdoorTemperatureAcceptableFor(tile, animalRace);
		}
		return false;
	}

	public void ClearCaches()
	{
		cache = new Dictionary<PlanetLayerDef, CachedTileTemperatureData[]>();
		usedSlots = new Dictionary<PlanetLayerDef, List<int>>();
	}

	private CachedTileTemperatureData RetrieveCachedData(PlanetTile tile)
	{
		if (!cache.ContainsKey(tile.LayerDef))
		{
			cache[tile.LayerDef] = new CachedTileTemperatureData[tile.Layer.TilesCount];
		}
		if (!usedSlots.ContainsKey(tile.LayerDef))
		{
			usedSlots[tile.LayerDef] = new List<int>();
		}
		if (cache[tile.LayerDef][tile.tileId] != null)
		{
			return cache[tile.LayerDef][tile.tileId];
		}
		cache[tile.LayerDef][tile.tileId] = new CachedTileTemperatureData(tile);
		usedSlots[tile.LayerDef].Add(tile.tileId);
		return cache[tile.LayerDef][tile.tileId];
	}
}
