using System;
using System.IO;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace Verse
{
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
			catch (Exception arg)
			{
				Log.Error("Critical error in root Start(): " + arg);
			}
		}

		public override void Update()
		{
			base.Update();
			if (!LongEventHandler.ShouldWaitForEvent && !destroyed)
			{
				try
				{
					ShipCountdown.ShipCountdownUpdate();
					TargetHighlighter.TargetHighlighterUpdate();
					Current.Game.UpdatePlay();
					musicManagerPlay.MusicUpdate();
				}
				catch (Exception arg)
				{
					Log.Error("Root level exception in Update(): " + arg);
				}
			}
		}

		private static void SetupForQuickTestPlay()
		{
			Current.ProgramState = ProgramState.Entry;
			Current.Game = new Game();
			Current.Game.InitData = new GameInitData();
			Current.Game.Scenario = ScenarioDefOf.Crashlanded.scenario;
			Find.Scenario.PreConfigure();
			Current.Game.storyteller = new Storyteller(StorytellerDefOf.Cassandra, DifficultyDefOf.Rough);
			Current.Game.World = WorldGenerator.GenerateWorld(0.05f, GenText.RandomSeedString(), OverallRainfall.Normal, OverallTemperature.Normal, OverallPopulation.Normal);
			Find.GameInitData.ChooseRandomStartingTile();
			Find.GameInitData.mapSize = 150;
			Find.GameInitData.PrepForMapGen();
			Find.Scenario.PreMapGenerate();
		}
	}
}
