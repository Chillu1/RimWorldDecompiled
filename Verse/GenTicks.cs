using RimWorld;
using UnityEngine;

namespace Verse
{
	public static class GenTicks
	{
		public const int TicksPerRealSecond = 60;

		public const int TickRareInterval = 250;

		public const int TickLongInterval = 2000;

		public static int TicksAbs
		{
			get
			{
				if (Current.ProgramState != ProgramState.Playing && Find.GameInitData != null && Find.GameInitData.gameToLoad.NullOrEmpty())
				{
					return ConfiguredTicksAbsAtGameStart;
				}
				if (Current.Game != null && Find.TickManager != null)
				{
					return Find.TickManager.TicksAbs;
				}
				return 0;
			}
		}

		public static int TicksGame
		{
			get
			{
				if (Current.Game != null && Find.TickManager != null)
				{
					return Find.TickManager.TicksGame;
				}
				return 0;
			}
		}

		public static int ConfiguredTicksAbsAtGameStart
		{
			get
			{
				GameInitData gameInitData = Find.GameInitData;
				ConfiguredTicksAbsAtGameStartCache ticksAbsCache = Find.World.ticksAbsCache;
				if (ticksAbsCache.TryGetCachedValue(gameInitData, out int ticksAbs))
				{
					return ticksAbs;
				}
				Vector2 vector = (gameInitData.startingTile < 0) ? Vector2.zero : Find.WorldGrid.LongLatOf(gameInitData.startingTile);
				Twelfth twelfth = (gameInitData.startingSeason != 0) ? gameInitData.startingSeason.GetFirstTwelfth(vector.y) : ((gameInitData.startingTile < 0) ? Season.Summer.GetFirstTwelfth(0f) : TwelfthUtility.FindStartingWarmTwelfth(gameInitData.startingTile));
				int num = (24 - GenDate.TimeZoneAt(vector.x)) % 24;
				int num2 = 300000 * (int)twelfth + 2500 * (6 + num);
				ticksAbsCache.Cache(num2, gameInitData);
				return num2;
			}
		}

		public static float TicksToSeconds(this int numTicks)
		{
			return (float)numTicks / 60f;
		}

		public static int SecondsToTicks(this float numSeconds)
		{
			return Mathf.RoundToInt(60f * numSeconds);
		}

		public static string ToStringSecondsFromTicks(this int numTicks)
		{
			return numTicks.TicksToSeconds().ToString("F1") + " " + "SecondsLower".Translate();
		}

		public static string ToStringTicksFromSeconds(this float numSeconds)
		{
			return numSeconds.SecondsToTicks().ToString();
		}
	}
}
