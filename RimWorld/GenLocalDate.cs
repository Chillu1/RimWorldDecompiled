using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class GenLocalDate
	{
		private static int TicksAbs => GenTicks.TicksAbs;

		public static int DayOfYear(Map map)
		{
			return DayOfYear(map.Tile);
		}

		public static int HourOfDay(Map map)
		{
			return HourOfDay(map.Tile);
		}

		public static int DayOfTwelfth(Map map)
		{
			return DayOfTwelfth(map.Tile);
		}

		public static Twelfth Twelfth(Map map)
		{
			return Twelfth(map.Tile);
		}

		public static Season Season(Map map)
		{
			return Season(map.Tile);
		}

		public static int Year(Map map)
		{
			if (Current.ProgramState != ProgramState.Playing)
			{
				return 5500;
			}
			return Year(map.Tile);
		}

		public static int DayOfSeason(Map map)
		{
			return DayOfSeason(map.Tile);
		}

		public static int DayOfQuadrum(Map map)
		{
			return DayOfQuadrum(map.Tile);
		}

		public static int DayTick(Map map)
		{
			return DayTick(map.Tile);
		}

		public static float DayPercent(Map map)
		{
			return DayPercent(map.Tile);
		}

		public static float YearPercent(Map map)
		{
			return YearPercent(map.Tile);
		}

		public static int HourInteger(Map map)
		{
			return HourInteger(map.Tile);
		}

		public static float HourFloat(Map map)
		{
			return HourFloat(map.Tile);
		}

		public static int DayOfYear(Thing thing)
		{
			if (Current.ProgramState == ProgramState.Playing)
			{
				return GenDate.DayOfYear(TicksAbs, LongitudeForDate(thing));
			}
			return 0;
		}

		public static int HourOfDay(Thing thing)
		{
			return GenDate.HourOfDay(TicksAbs, LongitudeForDate(thing));
		}

		public static int DayOfTwelfth(Thing thing)
		{
			return GenDate.DayOfTwelfth(TicksAbs, LongitudeForDate(thing));
		}

		public static Twelfth Twelfth(Thing thing)
		{
			return GenDate.Twelfth(TicksAbs, LongitudeForDate(thing));
		}

		public static Season Season(Thing thing)
		{
			return GenDate.Season(TicksAbs, LocationForDate(thing));
		}

		public static int Year(Thing thing)
		{
			if (Current.ProgramState != ProgramState.Playing)
			{
				return 5500;
			}
			return GenDate.Year(TicksAbs, LongitudeForDate(thing));
		}

		public static int DayOfSeason(Thing thing)
		{
			return GenDate.DayOfSeason(TicksAbs, LongitudeForDate(thing));
		}

		public static int DayOfQuadrum(Thing thing)
		{
			return GenDate.DayOfQuadrum(TicksAbs, LongitudeForDate(thing));
		}

		public static int DayTick(Thing thing)
		{
			return GenDate.DayTick(TicksAbs, LongitudeForDate(thing));
		}

		public static float DayPercent(Thing thing)
		{
			return GenDate.DayPercent(TicksAbs, LongitudeForDate(thing));
		}

		public static float YearPercent(Thing thing)
		{
			return GenDate.YearPercent(TicksAbs, LongitudeForDate(thing));
		}

		public static int HourInteger(Thing thing)
		{
			return GenDate.HourInteger(TicksAbs, LongitudeForDate(thing));
		}

		public static float HourFloat(Thing thing)
		{
			return GenDate.HourFloat(TicksAbs, LongitudeForDate(thing));
		}

		public static int DayOfYear(int tile)
		{
			if (Current.ProgramState == ProgramState.Playing)
			{
				return GenDate.DayOfYear(TicksAbs, Find.WorldGrid.LongLatOf(tile).x);
			}
			return 0;
		}

		public static int HourOfDay(int tile)
		{
			return GenDate.HourOfDay(TicksAbs, Find.WorldGrid.LongLatOf(tile).x);
		}

		public static int DayOfTwelfth(int tile)
		{
			return GenDate.DayOfTwelfth(TicksAbs, Find.WorldGrid.LongLatOf(tile).x);
		}

		public static Twelfth Twelfth(int tile)
		{
			return GenDate.Twelfth(TicksAbs, Find.WorldGrid.LongLatOf(tile).x);
		}

		public static Season Season(int tile)
		{
			return GenDate.Season(TicksAbs, Find.WorldGrid.LongLatOf(tile));
		}

		public static int Year(int tile)
		{
			if (Current.ProgramState != ProgramState.Playing)
			{
				return 5500;
			}
			return GenDate.Year(TicksAbs, Find.WorldGrid.LongLatOf(tile).x);
		}

		public static int DayOfSeason(int tile)
		{
			return GenDate.DayOfSeason(TicksAbs, Find.WorldGrid.LongLatOf(tile).x);
		}

		public static int DayOfQuadrum(int tile)
		{
			return GenDate.DayOfQuadrum(TicksAbs, Find.WorldGrid.LongLatOf(tile).x);
		}

		public static int DayTick(int tile)
		{
			return GenDate.DayTick(TicksAbs, Find.WorldGrid.LongLatOf(tile).x);
		}

		public static float DayPercent(int tile)
		{
			return GenDate.DayPercent(TicksAbs, Find.WorldGrid.LongLatOf(tile).x);
		}

		public static float YearPercent(int tile)
		{
			return GenDate.YearPercent(TicksAbs, Find.WorldGrid.LongLatOf(tile).x);
		}

		public static int HourInteger(int tile)
		{
			return GenDate.HourInteger(TicksAbs, Find.WorldGrid.LongLatOf(tile).x);
		}

		public static float HourFloat(int tile)
		{
			return GenDate.HourFloat(TicksAbs, Find.WorldGrid.LongLatOf(tile).x);
		}

		private static float LongitudeForDate(Thing thing)
		{
			return LocationForDate(thing).x;
		}

		private static Vector2 LocationForDate(Thing thing)
		{
			int tile = thing.Tile;
			if (tile >= 0)
			{
				return Find.WorldGrid.LongLatOf(tile);
			}
			return Vector2.zero;
		}
	}
}
