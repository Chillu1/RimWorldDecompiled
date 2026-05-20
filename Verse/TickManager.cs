using System;
using System.Collections.Generic;
using System.Diagnostics;
using LudeonTK;
using RimWorld;
using UnityEngine;

namespace Verse;

public sealed class TickManager : IExposable
{
	[TweakValue("Gameplay", 0f, 100f)]
	private static bool UltraSpeedBoost;

	private int ticksGameInt;

	public int gameStartAbsTick;

	private float realTimeToTickThrough;

	private TimeSpeed curTimeSpeed = TimeSpeed.Normal;

	public TimeSpeed prePauseTimeSpeed;

	private int startingYearInt = 5500;

	private Stopwatch clock = new Stopwatch();

	private int lastSettleTicksInt;

	private TickList tickListNormal = new TickList(TickerType.Normal);

	private TickList tickListRare = new TickList(TickerType.Rare);

	private TickList tickListLong = new TickList(TickerType.Long);

	public TimeSlower slower = new TimeSlower();

	private int lastAutoScreenshot;

	private int ticksThisFrame;

	private SimpleMovingAverage rawTickTimeAverage = new SimpleMovingAverage(720);

	private ExponentialMovingAverage smoothedTickTimeAverage = new ExponentialMovingAverage(0.1f);

	public const float WorstAllowedFPS = 22f;

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

	public int TicksSinceSettle => ticksGameInt - lastSettleTicksInt;

	public int SettleTick => lastSettleTicksInt;

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
					return 18f;
				}
				if (NothingHappeningInGame())
				{
					return 12f;
				}
				return 6f;
			case TimeSpeed.Ultrafast:
				if (Find.Maps.Count == 0 || UltraSpeedBoost)
				{
					return 150f;
				}
				return 15f;
			default:
				return -1f;
			}
		}
	}

	public int TicksThisFrame => ticksThisFrame;

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
			if (curTimeSpeed != TimeSpeed.Paused)
			{
				return ForcePaused;
			}
			return true;
		}
	}

	public bool ForcePaused
	{
		get
		{
			if ((Find.WindowStack == null || !Find.WindowStack.WindowsForcePause) && !LongEventHandler.ForcePause && !Find.TilePicker.Active && !WorldComponent_GravshipController.CutsceneInProgress)
			{
				WorldComponent_GravshipController gravshipController = Find.GravshipController;
				if (gravshipController == null || !gravshipController.LandingAreaConfirmationInProgress)
				{
					return MapGenerator.debugMode;
				}
			}
			return true;
		}
	}

	private AcceptanceReport PlayerCanControl
	{
		get
		{
			WorldComponent_GravshipController gravshipController = Find.GravshipController;
			if (gravshipController != null && gravshipController.LandingAreaConfirmationInProgress)
			{
				return "MessageConfirmLandingAreaFirst".Translate();
			}
			if (!Current.Game.PlayerHasControl)
			{
				return false;
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
			if (!PlayerCanControl)
			{
				if (!PlayerCanControl.Reason.NullOrEmpty())
				{
					Messages.Message(PlayerCanControl.Reason, MessageTypeDefOf.RejectInput, historical: false);
				}
			}
			else
			{
				curTimeSpeed = value;
			}
		}
	}

	public bool HasSettledNewColony => lastSettleTicksInt > 0;

	public float MeanTickTime => smoothedTickTimeAverage.GetAverage();

	public void TogglePaused()
	{
		if (!PlayerCanControl)
		{
			if (!PlayerCanControl.Reason.NullOrEmpty())
			{
				Messages.Message(PlayerCanControl.Reason, MessageTypeDefOf.RejectInput, historical: false);
			}
		}
		else if (curTimeSpeed != TimeSpeed.Paused)
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
		if (curTimeSpeed != TimeSpeed.Paused)
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
					if (pawn.HostFaction == null && pawn.RaceProps.Humanlike && pawn.Awake() && !pawn.IsGhoul)
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
		Scribe_Values.Look(ref lastSettleTicksInt, "lastSettleTicks", 0);
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
		if (t is IThingHolder)
		{
			return tickListNormal;
		}
		return t.def.tickerType switch
		{
			TickerType.Never => null, 
			TickerType.Normal => tickListNormal, 
			TickerType.Rare => tickListRare, 
			TickerType.Long => tickListLong, 
			_ => throw new InvalidOperationException(), 
		};
	}

	public void TickManagerUpdate()
	{
		ticksThisFrame = 0;
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
		float tickRateMultiplier = TickRateMultiplier;
		clock.Reset();
		clock.Start();
		while (realTimeToTickThrough > 0f && (float)ticksThisFrame < tickRateMultiplier * 2f)
		{
			double totalMilliseconds = clock.Elapsed.TotalMilliseconds;
			DoSingleTick();
			double totalMilliseconds2 = clock.Elapsed.TotalMilliseconds;
			realTimeToTickThrough -= curTimePerTick;
			ticksThisFrame++;
			rawTickTimeAverage.AddValue((float)(totalMilliseconds2 - totalMilliseconds));
			smoothedTickTimeAverage.AddValue(rawTickTimeAverage.GetAverage());
			if (Paused || (float)clock.ElapsedMilliseconds > 45.454544f)
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
		try
		{
			Find.TransportShipManager.ShipObjectsTick();
		}
		catch (Exception ex14)
		{
			Log.Error(ex14.ToString());
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

	public void ResetSettlementTicks()
	{
		lastSettleTicksInt = TicksGame;
	}
}
