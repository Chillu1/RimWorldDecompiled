using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Verse
{
	public sealed class TickManager : IExposable
	{
		private int ticksGameInt;

		public int gameStartAbsTick;

		private float realTimeToTickThrough;

		private TimeSpeed curTimeSpeed = TimeSpeed.Normal;

		public TimeSpeed prePauseTimeSpeed;

		private int startingYearInt = 5500;

		private Stopwatch clock = new Stopwatch();

		private TickList tickListNormal = new TickList(TickerType.Normal);

		private TickList tickListRare = new TickList(TickerType.Rare);

		private TickList tickListLong = new TickList(TickerType.Long);

		public TimeSlower slower = new TimeSlower();

		private int lastAutoScreenshot;

		private float WorstAllowedFPS = 22f;

		private int lastNothingHappeningCheckTick = -1;

		private bool nothingHappeningCached;

		public int TicksGame => ticksGameInt;

		public int TicksAbs
		{
			get
			{
				if (gameStartAbsTick == 0)
				{
					Log.ErrorOnce("Accessing TicksAbs but gameStartAbsTick is not set yet (you most likely want to use GenTicks.TicksAbs instead).", 1049580013);
					return ticksGameInt;
				}
				return ticksGameInt + gameStartAbsTick;
			}
		}

		public int StartingYear => startingYearInt;

		public float TickRateMultiplier
		{
			get
			{
				if (slower.ForcedNormalSpeed)
				{
					if (curTimeSpeed == TimeSpeed.Paused)
					{
						return 0f;
					}
					return 1f;
				}
				switch (curTimeSpeed)
				{
				case TimeSpeed.Paused:
					return 0f;
				case TimeSpeed.Normal:
					return 1f;
				case TimeSpeed.Fast:
					return 3f;
				case TimeSpeed.Superfast:
					if (Find.Maps.Count == 0)
					{
						return 120f;
					}
					if (NothingHappeningInGame())
					{
						return 12f;
					}
					return 6f;
				case TimeSpeed.Ultrafast:
					if (Find.Maps.Count == 0)
					{
						return 150f;
					}
					return 15f;
				default:
					return -1f;
				}
			}
		}

		private float CurTimePerTick
		{
			get
			{
				if (TickRateMultiplier == 0f)
				{
					return 0f;
				}
				return 1f / (60f * TickRateMultiplier);
			}
		}

		public bool Paused
		{
			get
			{
				if (curTimeSpeed != 0 && !Find.WindowStack.WindowsForcePause)
				{
					return LongEventHandler.ForcePause;
				}
				return true;
			}
		}

		public bool NotPlaying
		{
			get
			{
				if (Find.MainTabsRoot.OpenTab == MainButtonDefOf.Menu)
				{
					return true;
				}
				return false;
			}
		}

		public TimeSpeed CurTimeSpeed
		{
			get
			{
				return curTimeSpeed;
			}
			set
			{
				curTimeSpeed = value;
			}
		}

		public void TogglePaused()
		{
			if (curTimeSpeed != 0)
			{
				prePauseTimeSpeed = curTimeSpeed;
				curTimeSpeed = TimeSpeed.Paused;
			}
			else if (prePauseTimeSpeed != curTimeSpeed)
			{
				curTimeSpeed = prePauseTimeSpeed;
			}
			else
			{
				curTimeSpeed = TimeSpeed.Normal;
			}
		}

		public void Pause()
		{
			if (curTimeSpeed != 0)
			{
				TogglePaused();
			}
		}

		private bool NothingHappeningInGame()
		{
			if (lastNothingHappeningCheckTick != TicksGame)
			{
				nothingHappeningCached = true;
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					List<Pawn> list = maps[i].mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer);
					for (int j = 0; j < list.Count; j++)
					{
						Pawn pawn = list[j];
						if (pawn.HostFaction == null && pawn.RaceProps.Humanlike && pawn.Awake())
						{
							nothingHappeningCached = false;
							break;
						}
					}
					if (!nothingHappeningCached)
					{
						break;
					}
				}
				if (nothingHappeningCached)
				{
					for (int k = 0; k < maps.Count; k++)
					{
						if (maps[k].IsPlayerHome && (int)maps[k].dangerWatcher.DangerRating >= 1)
						{
							nothingHappeningCached = false;
							break;
						}
					}
				}
				lastNothingHappeningCheckTick = TicksGame;
			}
			return nothingHappeningCached;
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref ticksGameInt, "ticksGame", 0);
			Scribe_Values.Look(ref gameStartAbsTick, "gameStartAbsTick", 0);
			Scribe_Values.Look(ref startingYearInt, "startingYear", 0);
		}

		public void RegisterAllTickabilityFor(Thing t)
		{
			TickListFor(t)?.RegisterThing(t);
		}

		public void DeRegisterAllTickabilityFor(Thing t)
		{
			TickListFor(t)?.DeregisterThing(t);
		}

		private TickList TickListFor(Thing t)
		{
			switch (t.def.tickerType)
			{
			case TickerType.Never:
				return null;
			case TickerType.Normal:
				return tickListNormal;
			case TickerType.Rare:
				return tickListRare;
			case TickerType.Long:
				return tickListLong;
			default:
				throw new InvalidOperationException();
			}
		}

		public void TickManagerUpdate()
		{
			if (Paused)
			{
				return;
			}
			float curTimePerTick = CurTimePerTick;
			if (Mathf.Abs(Time.deltaTime - curTimePerTick) < curTimePerTick * 0.1f)
			{
				realTimeToTickThrough += curTimePerTick;
			}
			else
			{
				realTimeToTickThrough += Time.deltaTime;
			}
			int num = 0;
			float tickRateMultiplier = TickRateMultiplier;
			clock.Reset();
			clock.Start();
			while (realTimeToTickThrough > 0f && (float)num < tickRateMultiplier * 2f)
			{
				DoSingleTick();
				realTimeToTickThrough -= curTimePerTick;
				num++;
				if (Paused || (float)clock.ElapsedMilliseconds > 1000f / WorstAllowedFPS)
				{
					break;
				}
			}
			if (realTimeToTickThrough > 0f)
			{
				realTimeToTickThrough = 0f;
			}
		}

		public void DoSingleTick()
		{
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				maps[i].MapPreTick();
			}
			if (!DebugSettings.fastEcology)
			{
				ticksGameInt++;
			}
			else
			{
				ticksGameInt += 2000;
			}
			Shader.SetGlobalFloat(ShaderPropertyIDs.GameSeconds, TicksGame.TicksToSeconds());
			tickListNormal.Tick();
			tickListRare.Tick();
			tickListLong.Tick();
			try
			{
				Find.DateNotifier.DateNotifierTick();
			}
			catch (Exception ex)
			{
				Log.Error(ex.ToString());
			}
			try
			{
				Find.Scenario.TickScenario();
			}
			catch (Exception ex2)
			{
				Log.Error(ex2.ToString());
			}
			try
			{
				Find.World.WorldTick();
			}
			catch (Exception ex3)
			{
				Log.Error(ex3.ToString());
			}
			try
			{
				Find.StoryWatcher.StoryWatcherTick();
			}
			catch (Exception ex4)
			{
				Log.Error(ex4.ToString());
			}
			try
			{
				Find.GameEnder.GameEndTick();
			}
			catch (Exception ex5)
			{
				Log.Error(ex5.ToString());
			}
			try
			{
				Find.Storyteller.StorytellerTick();
			}
			catch (Exception ex6)
			{
				Log.Error(ex6.ToString());
			}
			try
			{
				Find.TaleManager.TaleManagerTick();
			}
			catch (Exception ex7)
			{
				Log.Error(ex7.ToString());
			}
			try
			{
				Find.QuestManager.QuestManagerTick();
			}
			catch (Exception ex8)
			{
				Log.Error(ex8.ToString());
			}
			try
			{
				Find.World.WorldPostTick();
			}
			catch (Exception ex9)
			{
				Log.Error(ex9.ToString());
			}
			for (int j = 0; j < maps.Count; j++)
			{
				maps[j].MapPostTick();
			}
			try
			{
				Find.History.HistoryTick();
			}
			catch (Exception ex10)
			{
				Log.Error(ex10.ToString());
			}
			GameComponentUtility.GameComponentTick();
			try
			{
				Find.LetterStack.LetterStackTick();
			}
			catch (Exception ex11)
			{
				Log.Error(ex11.ToString());
			}
			try
			{
				Find.Autosaver.AutosaverTick();
			}
			catch (Exception ex12)
			{
				Log.Error(ex12.ToString());
			}
			if (DebugViewSettings.logHourlyScreenshot && Find.TickManager.TicksGame >= lastAutoScreenshot + 2500)
			{
				ScreenshotTaker.QueueSilentScreenshot();
				lastAutoScreenshot = Find.TickManager.TicksGame / 2500 * 2500;
			}
			try
			{
				FilthMonitor.FilthMonitorTick();
			}
			catch (Exception ex13)
			{
				Log.Error(ex13.ToString());
			}
			UnityEngine.Debug.developerConsoleVisible = false;
		}

		public void RemoveAllFromMap(Map map)
		{
			tickListNormal.RemoveWhere((Thing x) => x.Map == map);
			tickListRare.RemoveWhere((Thing x) => x.Map == map);
			tickListLong.RemoveWhere((Thing x) => x.Map == map);
		}

		public void DebugSetTicksGame(int newTicksGame)
		{
			ticksGameInt = newTicksGame;
		}

		public void Notify_GeneratedPotentiallyHostileMap()
		{
			Pause();
			slower.SignalForceNormalSpeedShort();
		}
	}
}
