using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;

namespace Verse;

public class MapTemperature : ICellBoolGiver
{
	private static readonly List<(float, Color)> TemperatureColorMap = new List<(float, Color)>
	{
		(-25f, ColorLibrary.DarkBlue),
		(0f, ColorLibrary.Blue),
		(25f, ColorLibrary.Green),
		(50f, ColorLibrary.Yellow),
		(100f, ColorLibrary.Red)
	};

	private const int TemperatureOverlayUpdateInterval = 60;

	private Map map;

	private CellBoolDrawer drawerInt;

	public float OutdoorTemp
	{
		get
		{
			if (map.Biome.constantOutdoorTemperature.HasValue)
			{
				return map.Biome.constantOutdoorTemperature.Value;
			}
			if (map.IsPocketMap)
			{
				return map.generatorDef.pocketMapProperties.temperature;
			}
			return Find.World.tileTemperatures.GetOutdoorTemp(map.Tile);
		}
	}

	public float SeasonalTemp
	{
		get
		{
			if (map.Biome.constantOutdoorTemperature.HasValue)
			{
				return map.Biome.constantOutdoorTemperature.Value;
			}
			if (map.IsPocketMap)
			{
				return map.generatorDef.pocketMapProperties.temperature;
			}
			return Find.World.tileTemperatures.GetSeasonalTemp(map.Tile);
		}
	}

	public CellBoolDrawer Drawer => drawerInt ?? (drawerInt = new CellBoolDrawer(this, map.Size.x, map.Size.z, 3600));

	public Color Color => Color.white;

	public MapTemperature(Map map)
	{
		this.map = map;
	}

	public void MapTemperatureTick()
	{
		if (Find.TickManager.TicksGame % 120 == 7 || DebugSettings.fastEcology)
		{
			IReadOnlyList<Room> allRooms = map.regionGrid.AllRooms;
			for (int i = 0; i < allRooms.Count; i++)
			{
				allRooms[i].TempTracker.EqualizeTemperature();
			}
		}
		if (Find.TickManager.TicksGame % 60 == 0)
		{
			Drawer.SetDirty();
		}
	}

	public void TemperatureUpdate()
	{
		if (Find.PlaySettings.showTemperatureOverlay && !Find.ScreenshotModeHandler.Active)
		{
			Drawer.MarkForDraw();
		}
		Drawer.CellBoolDrawerUpdate();
	}

	public bool SeasonAcceptableFor(ThingDef animalRace, float buffer = 0f)
	{
		float seasonalTemp = SeasonalTemp;
		if (seasonalTemp > animalRace.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin) - buffer)
		{
			return seasonalTemp < animalRace.GetStatValueAbstract(StatDefOf.ComfyTemperatureMax) + buffer;
		}
		return false;
	}

	public bool OutdoorTemperatureAcceptableFor(ThingDef animalRace, float buffer = 0f)
	{
		float outdoorTemp = OutdoorTemp;
		if (outdoorTemp > animalRace.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin) - buffer)
		{
			return outdoorTemp < animalRace.GetStatValueAbstract(StatDefOf.ComfyTemperatureMax) + buffer;
		}
		return false;
	}

	public bool SeasonAndOutdoorTemperatureAcceptableFor(ThingDef animalRace, float buffer = 0f)
	{
		if (SeasonAcceptableFor(animalRace, buffer))
		{
			return OutdoorTemperatureAcceptableFor(animalRace, buffer);
		}
		return false;
	}

	public bool LocalSeasonsAreMeaningful()
	{
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < 12; i++)
		{
			float num = Find.World.tileTemperatures.AverageTemperatureForTwelfth(map.Tile, (Twelfth)i);
			if (num > 0f)
			{
				flag2 = true;
			}
			if (num < 0f)
			{
				flag = true;
			}
		}
		return flag2 && flag;
	}

	public bool GetCellBool(int index)
	{
		IntVec3 intVec = map.cellIndices.IndexToCell(index);
		if (intVec.Fogged(map))
		{
			return false;
		}
		if (ModsConfig.OdysseyActive && intVec.GetTerrain(map) == TerrainDefOf.Space)
		{
			return false;
		}
		return intVec.GetRoom(map) != null;
	}

	public Color GetCellExtraColor(int index)
	{
		float temperature = map.cellIndices.IndexToCell(index).GetTemperature(map);
		return GetColorForTemperature(temperature);
	}

	private Color GetColorForTemperature(float temperature)
	{
		List<(float, Color)> temperatureColorMap = TemperatureColorMap;
		if (temperature <= temperatureColorMap[0].Item1)
		{
			return temperatureColorMap[0].Item2;
		}
		if (temperature >= temperatureColorMap[temperatureColorMap.Count - 1].Item1)
		{
			return temperatureColorMap[temperatureColorMap.Count - 1].Item2;
		}
		for (int i = 1; i < temperatureColorMap.Count; i++)
		{
			if (temperatureColorMap[i].Item1 > temperature)
			{
				if (i >= temperatureColorMap.Count - 1)
				{
					return temperatureColorMap[i].Item2;
				}
				int index = i - 1;
				int index2 = i;
				float item = temperatureColorMap[index].Item1;
				float item2 = temperatureColorMap[index2].Item1;
				float t = (temperature - item) / (item2 - item);
				return Color.Lerp(temperatureColorMap[index].Item2, temperatureColorMap[index2].Item2, t);
			}
		}
		Log.Error("Error when trying to determine correct color for temperature grid.");
		return Color.white;
	}

	public void Notify_ThingSpawned(Thing thing)
	{
		if (thing.def.AffectsRegions)
		{
			Drawer.SetDirty();
		}
	}

	public void DebugLogTemps()
	{
		StringBuilder stringBuilder = new StringBuilder();
		float latitude = ((Find.CurrentMap != null) ? Find.WorldGrid.LongLatOf(Find.CurrentMap.Tile).y : 0f);
		stringBuilder.AppendLine("Latitude " + latitude);
		stringBuilder.AppendLine("-----Temperature for each hour this day------");
		stringBuilder.AppendLine("Hour    Temp    SunEffect");
		int num = Find.TickManager.TicksAbs - Find.TickManager.TicksAbs % 60000;
		for (int i = 0; i < 24; i++)
		{
			int absTick = num + i * 2500;
			stringBuilder.Append(i.ToString().PadRight(5));
			stringBuilder.Append(Find.World.tileTemperatures.OutdoorTemperatureAt(map.Tile, absTick).ToString("F2").PadRight(8));
			stringBuilder.Append(GenTemperature.OffsetFromSunCycle(absTick, map.Tile).ToString("F2"));
			stringBuilder.AppendLine();
		}
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("-----Temperature for each twelfth this year------");
		for (int j = 0; j < 12; j++)
		{
			Twelfth twelfth = (Twelfth)j;
			float num2 = Find.World.tileTemperatures.AverageTemperatureForTwelfth(map.Tile, twelfth);
			stringBuilder.AppendLine(twelfth.GetQuadrum().ToString() + "/" + SeasonUtility.GetReportedSeason(twelfth.GetMiddleYearPct(), latitude).ToString() + " - " + twelfth.ToString() + " " + num2.ToString("F2"));
		}
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("-----Temperature for each day this year------");
		stringBuilder.AppendLine("Tile avg: " + map.TileInfo.temperature + "°C");
		stringBuilder.AppendLine("Seasonal shift: " + GenTemperature.SeasonalShiftAmplitudeAt(map.Tile));
		stringBuilder.AppendLine("Equatorial distance: " + Find.WorldGrid.DistanceFromEquatorNormalized(map.Tile));
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("Day  Lo   Hi   OffsetFromSeason RandomDailyVariation");
		for (int k = 0; k < 60; k++)
		{
			int absTick2 = (int)((float)(k * 60000) + 15000f);
			int absTick3 = (int)((float)(k * 60000) + 45000f);
			stringBuilder.Append(k.ToString().PadRight(8));
			stringBuilder.Append(Find.World.tileTemperatures.OutdoorTemperatureAt(map.Tile, absTick2).ToString("F2").PadRight(11));
			stringBuilder.Append(Find.World.tileTemperatures.OutdoorTemperatureAt(map.Tile, absTick3).ToString("F2").PadRight(11));
			stringBuilder.Append(GenTemperature.OffsetFromSeasonCycle(absTick3, map.Tile).ToString("F2").PadRight(11));
			stringBuilder.Append(Find.World.tileTemperatures.OffsetFromDailyRandomVariation(map.Tile, absTick3).ToString("F2"));
			stringBuilder.AppendLine();
		}
		Log.Message(stringBuilder.ToString());
	}

	public void DebugLogTemperatureOverlayColors()
	{
		int num = 150;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("-----Temperature overlay colors by temperature------");
		for (int i = -50; i <= num; i++)
		{
			stringBuilder.AppendLine(i + ": " + GetColorForTemperature(i).ToString());
		}
		Log.Message(stringBuilder.ToString());
	}
}
