using System;
using System.Linq;

namespace Verse;

public static class GameAndMapInitExceptionHandlers
{
	public static void ErrorWhileLoadingAssets(Exception e)
	{
		string text = "ErrorWhileLoadingAssets".Translate();
		if (ModsConfig.ActiveModsInLoadOrder.Any((ModMetaData x) => !x.Official) || !ModsConfig.ActiveModsInLoadOrder.Any((ModMetaData x) => x.IsCoreMod))
		{
			text += "\n\n" + "ErrorWhileLoadingAssets_ModsInfo".Translate();
		}
		DelayedErrorWindowRequest.Add(text, "ErrorWhileLoadingAssetsTitle".Translate());
		GenScene.GoToMainMenu();
	}

	public static void ErrorWhileGeneratingMap(Exception e)
	{
		DelayedErrorWindowRequest.Add("ErrorWhileGeneratingMap".Translate(), "ErrorWhileGeneratingMapTitle".Translate());
		Scribe.ForceStop();
		GenScene.GoToMainMenu();
	}

	public static void ErrorWhileLoadingGame(Exception e)
	{
		string text = "ErrorWhileLoadingMap".Translate();
		if (!ScribeMetaHeaderUtility.LoadedModsMatchesActiveMods(out var loadedModsSummary, out var runningModsSummary))
		{
			text += "\n\n" + "ModsMismatchWarningText".Translate(loadedModsSummary, runningModsSummary);
		}
		DelayedErrorWindowRequest.Add(text, "ErrorWhileLoadingMapTitle".Translate());
		Scribe.ForceStop();
		GenScene.GoToMainMenu();
	}
}
