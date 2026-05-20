using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Profile;

namespace RimWorld;

public sealed class Autosaver
{
	private int ticksSinceSave;

	public const float MaxPermadeathModeAutosaveInterval = 1f;

	private float AutosaveIntervalDays
	{
		get
		{
			float num = Prefs.AutosaveIntervalDays;
			if (Current.Game.Info.permadeathMode && num > 1f)
			{
				num = 1f;
			}
			return num;
		}
	}

	private int AutosaveIntervalTicks => Mathf.RoundToInt(AutosaveIntervalDays * 60000f);

	public void AutosaverTick()
	{
		ticksSinceSave++;
		if (ticksSinceSave >= AutosaveIntervalTicks && !GameDataSaveLoader.SavingIsTemporarilyDisabled)
		{
			LongEventHandler.QueueLongEvent(DoAutosave, "Autosaving", doAsynchronously: false, null);
			ticksSinceSave = 0;
		}
	}

	public void DoAutosave()
	{
		string fileName = ((!Current.Game.Info.permadeathMode) ? NewAutosaveFileName() : Current.Game.Info.permadeathModeUniqueName);
		GameDataSaveLoader.SaveGame(fileName);
	}

	private void DoMemoryCleanup()
	{
		MemoryUtility.UnloadUnusedUnityAssets();
	}

	private string NewAutosaveFileName()
	{
		string text = AutoSaveNames().FirstOrDefault((string name) => !SaveGameFilesUtility.SavedGameNamedExists(name));
		if (text != null)
		{
			return text;
		}
		return AutoSaveNames().MinBy((string name) => new FileInfo(GenFilePaths.FilePathForSavedGame(name)).LastWriteTime);
	}

	private IEnumerable<string> AutoSaveNames()
	{
		for (int i = 1; i <= Prefs.AutosavesCount; i++)
		{
			yield return "Autosave-" + i;
		}
	}
}
