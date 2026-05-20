using System;

namespace Verse
{
	public static class PreLoadUtility
	{
		public static void CheckVersionAndLoad(string path, ScribeMetaHeaderUtility.ScribeHeaderMode mode, Action loadAct, bool skipOnMismatch = false)
		{
			try
			{
				Scribe.loader.InitLoadingMetaHeaderOnly(path);
				ScribeMetaHeaderUtility.LoadGameDataHeader(mode, logVersionConflictWarning: false);
				Scribe.loader.FinalizeLoading();
			}
			catch (Exception ex)
			{
				Log.Warning("Exception loading " + path + ": " + ex);
				Scribe.ForceStop();
			}
			if ((!skipOnMismatch || ScribeMetaHeaderUtility.LoadedModsMatchesActiveMods(out var _, out var _)) && !ScribeMetaHeaderUtility.TryCreateDialogsForVersionMismatchWarnings(loadAct))
			{
				loadAct();
			}
		}
	}
}
