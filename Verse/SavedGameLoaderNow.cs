using System;
using System.Linq;

namespace Verse
{
	public class SavedGameLoaderNow
	{
		public static void LoadGameFromSaveFileNow(string fileName)
		{
			string str = LoadedModManager.RunningMods.Select((ModContentPack mod) => mod.PackageIdPlayerFacing).ToLineList("  - ");
			Log.Message("Loading game from file " + fileName + " with mods:\n" + str);
			DeepProfiler.Start("Loading game from file " + fileName);
			Current.Game = new Game();
			DeepProfiler.Start("InitLoading (read file)");
			Scribe.loader.InitLoading(GenFilePaths.FilePathForSavedGame(fileName));
			DeepProfiler.End();
			try
			{
				ScribeMetaHeaderUtility.LoadGameDataHeader(ScribeMetaHeaderUtility.ScribeHeaderMode.Map, logVersionConflictWarning: true);
				if (!Scribe.EnterNode("game"))
				{
					Log.Error("Could not find game XML node.");
					Scribe.ForceStop();
					return;
				}
				Current.Game = new Game();
				Current.Game.LoadGame();
			}
			catch (Exception)
			{
				Scribe.ForceStop();
				throw;
			}
			PermadeathModeUtility.CheckUpdatePermadeathModeUniqueNameOnGameLoad(fileName);
			DeepProfiler.End();
		}
	}
}
