using System;
using System.IO;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace Verse;

public class Root_Play : Root
{
	public MusicManagerPlay musicManagerPlay;

	public override void Start()
	{
		base.Start();
		try
		{
			musicManagerPlay = new MusicManagerPlay();
			FileInfo autostart = (Root.checkedAutostartSaveFile ? null : SaveGameFilesUtility.GetAutostartSaveFile());
			Root.checkedAutostartSaveFile = true;
			if (autostart != null)
			{
				LongEventHandler.QueueLongEvent(delegate
				{
					SavedGameLoaderNow.LoadGameFromSaveFileNow(Path.GetFileNameWithoutExtension(autostart.Name));
				}, "LoadingLongEvent", doAsynchronously: true, GameAndMapInitExceptionHandlers.ErrorWhileLoadingGame);
			}
			else if (Find.GameInitData != null && !Find.GameInitData.gameToLoad.NullOrEmpty())
			{
				LongEventHandler.QueueLongEvent(delegate
				{
					SavedGameLoaderNow.LoadGameFromSaveFileNow(Find.GameInitData.gameToLoad);
				}, "LoadingLongEvent", doAsynchronously: true, GameAndMapInitExceptionHandlers.ErrorWhileLoadingGame);
			}
			else
			{
				LongEventHandler.QueueLongEvent(delegate
				{
					if (Current.Game == null)
					{
						SetupForQuickTestPlay();
						Find.GameInitData.PrepForMapGen();
						Find.Scenario.PreMapGenerate();
					}
					Current.Game.InitNewGame();
				}, "GeneratingMap", doAsynchronously: true, GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap);
			}
			LongEventHandler.QueueLongEvent(delegate
			{
				ScreenFader.SetColor(Color.black);
				ScreenFader.StartFade(Color.clear, 0.5f);
			}, null, doAsynchronously: false, null);
		}
		catch (Exception ex)
		{
			Log.Error("Critical error in root Start(): " + ex);
		}
	}

	public override void Update()
	{
		base.Update();
		if (LongEventHandler.ShouldWaitForEvent || destroyed)
		{
			return;
		}
		try
		{
			ShipCountdown.ShipCountdownUpdate();
			if (ModsConfig.IdeologyActive)
			{
				ArchonexusCountdown.ArchonexusCountdownUpdate();
			}
			TargetHighlighter.TargetHighlighterUpdate();
			Current.Game.UpdatePlay();
			musicManagerPlay.MusicUpdate();
			PerformanceBenchmarkUtility.CheckBenchmark();
		}
		catch (Exception ex)
		{
			Log.Error("Root level exception in Update(): " + ex);
		}
	}

	public static void SetupForQuickTestPlay()
	{
		Current.ProgramState = ProgramState.Entry;
		Game.ClearCaches();
		Current.Game = new Game();
		Current.Game.InitData = new GameInitData();
		Current.Game.Scenario = ScenarioDefOf.Crashlanded.scenario;
		Find.Scenario.PreConfigure();
		Current.Game.storyteller = new Storyteller(StorytellerDefOf.Cassandra, DifficultyDefOf.Rough);
		Current.Game.World = WorldGenerator.GenerateWorld(0.3f, GenText.RandomSeedString(), OverallRainfall.Normal, OverallTemperature.Normal, OverallPopulation.Normal, LandmarkDensity.Normal);
		Find.GameInitData.ChooseRandomStartingTile();
		Find.GameInitData.mapSize = 250;
		Find.Scenario.PostIdeoChosen();
	}
}
