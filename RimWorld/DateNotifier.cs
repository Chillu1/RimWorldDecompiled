using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class DateNotifier : IExposable
{
	private Season lastSeason;

	public void ExposeData()
	{
		Scribe_Values.Look(ref lastSeason, "lastSeason", Season.Undefined);
	}

	public void DateNotifierTick()
	{
		Map map = FindPlayerHomeWithMinTimezone();
		float latitude = ((map != null) ? Find.WorldGrid.LongLatOf(map.Tile).y : 0f);
		float longitude = ((map != null) ? Find.WorldGrid.LongLatOf(map.Tile).x : 0f);
		Season season = GenDate.Season(Find.TickManager.TicksAbs, latitude, longitude);
		if (season == lastSeason || (lastSeason != Season.Undefined && season == lastSeason.GetPreviousSeason()))
		{
			return;
		}
		if (lastSeason != Season.Undefined && AnyPlayerHomeSeasonsAreMeaningful())
		{
			if (GenDate.YearsPassed == 0 && season == Season.Summer && AnyPlayerHomeAvgTempIsLowInWinter())
			{
				Find.LetterStack.ReceiveLetter("LetterLabelFirstSummerWarning".Translate(), "FirstSummerWarning".Translate(), LetterDefOf.NeutralEvent);
			}
			else if (GenDate.DaysPassed > 5)
			{
				Messages.Message("MessageSeasonBegun".Translate(season.Label()).CapitalizeFirst(), MessageTypeDefOf.NeutralEvent);
			}
		}
		lastSeason = season;
	}

	private Map FindPlayerHomeWithMinTimezone()
	{
		List<Map> maps = Find.Maps;
		Map map = null;
		int num = -1;
		for (int i = 0; i < maps.Count; i++)
		{
			if (maps[i].IsPlayerHome)
			{
				int num2 = GenDate.TimeZoneAt(Find.WorldGrid.LongLatOf(maps[i].Tile).x);
				if (map == null || num2 < num)
				{
					map = maps[i];
					num = num2;
				}
			}
		}
		return map;
	}

	private bool AnyPlayerHomeSeasonsAreMeaningful()
	{
		List<Map> maps = Find.Maps;
		for (int i = 0; i < maps.Count; i++)
		{
			if (maps[i].IsPlayerHome && maps[i].mapTemperature.LocalSeasonsAreMeaningful() && maps[i].Tile.Valid && !maps[i].Tile.LayerDef.isSpace)
			{
				return true;
			}
		}
		return false;
	}

	private bool AnyPlayerHomeAvgTempIsLowInWinter()
	{
		List<Map> maps = Find.Maps;
		for (int i = 0; i < maps.Count; i++)
		{
			if (maps[i].IsPlayerHome && GenTemperature.AverageTemperatureAtTileForTwelfth(maps[i].Tile, Season.Winter.GetMiddleTwelfth(Find.WorldGrid.LongLatOf(maps[i].Tile).y)) < 8f)
			{
				return true;
			}
		}
		return false;
	}
}
