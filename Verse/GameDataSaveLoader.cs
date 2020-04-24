using RimWorld;
using System;
using System.IO;
using Verse.Profile;

namespace Verse
{
	public static class GameDataSaveLoader
	{
		private static int lastSaveTick = -9999;

		public const string SavedScenarioParentNodeName = "savedscenario";

		public const string SavedWorldParentNodeName = "savedworld";

		public const string SavedGameParentNodeName = "savegame";

		public const string GameNodeName = "game";

		public const string WorldNodeName = "world";

		public const string ScenarioNodeName = "scenario";

		public const string AutosavePrefix = "Autosave";

		public const string AutostartSaveName = "autostart";

		public static bool CurrentGameStateIsValuable => Find.TickManager.TicksGame > lastSaveTick + 60;

		public static void SaveScenario(Scenario scen, string absFilePath)
		{
			try
			{
				scen.fileName = Path.GetFileNameWithoutExtension(absFilePath);
				SafeSaver.Save(absFilePath, "savedscenario", delegate
				{
					ScribeMetaHeaderUtility.WriteMetaHeader();
					Scribe_Deep.Look(ref scen, "scenario");
				});
			}
			catch (Exception ex)
			{
				Log.Error("Exception while saving world: " + ex.ToString());
			}
		}

		public static bool TryLoadScenario(string absPath, ScenarioCategory category, out Scenario scen)
		{
			scen = null;
			try
			{
				Scribe.loader.InitLoading(absPath);
				try
				{
					ScribeMetaHeaderUtility.LoadGameDataHeader(ScribeMetaHeaderUtility.ScribeHeaderMode.Scenario, logVersionConflictWarning: true);
					Scribe_Deep.Look(ref scen, "scenario");
					Scribe.loader.FinalizeLoading();
				}
				catch
				{
					Scribe.ForceStop();
					throw;
				}
				scen.fileName = Path.GetFileNameWithoutExtension(new FileInfo(absPath).Name);
				scen.Category = category;
			}
			catch (Exception ex)
			{
				Log.Error("Exception loading scenario: " + ex.ToString());
				scen = null;
				Scribe.ForceStop();
			}
			return scen != null;
		}

		public static void SaveGame(string fileName)
		{
			try
			{
				SafeSaver.Save(GenFilePaths.FilePathForSavedGame(fileName), "savegame", delegate
				{
					ScribeMetaHeaderUtility.WriteMetaHeader();
					Game target = Current.Game;
					Scribe_Deep.Look(ref target, "game");
				}, Find.GameInfo.permadeathMode);
				lastSaveTick = Find.TickManager.TicksGame;
			}
			catch (Exception arg)
			{
				Log.Error("Exception while saving game: " + arg);
			}
		}

		public static void CheckVersionAndLoadGame(string saveFileName)
		{
			PreLoadUtility.CheckVersionAndLoad(GenFilePaths.FilePathForSavedGame(saveFileName), ScribeMetaHeaderUtility.ScribeHeaderMode.Map, delegate
			{
				LoadGame(saveFileName);
			});
		}

		public static void LoadGame(string saveFileName)
		{
			LongEventHandler.QueueLongEvent(delegate
			{
				MemoryUtility.ClearAllMapsAndWorld();
				Current.Game = new Game();
				Current.Game.InitData = new GameInitData();
				Current.Game.InitData.gameToLoad = saveFileName;
			}, "Play", "LoadingLongEvent", doAsynchronously: true, null);
		}

		public static void LoadGame(FileInfo saveFile)
		{
			LoadGame(Path.GetFileNameWithoutExtension(saveFile.Name));
		}
	}
}
