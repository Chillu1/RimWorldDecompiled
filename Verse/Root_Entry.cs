using System;
using System.IO;
using RimWorld;

namespace Verse;

public class Root_Entry : Root
{
	public MusicManagerEntry musicManagerEntry;

	public override void Start()
	{
		base.Start();
		try
		{
			Current.Game = null;
			musicManagerEntry = new MusicManagerEntry();
			FileInfo fileInfo = (Root.checkedAutostartSaveFile ? null : SaveGameFilesUtility.GetAutostartSaveFile());
			Root.checkedAutostartSaveFile = true;
			if (fileInfo != null)
			{
				GameDataSaveLoader.LoadGame(fileInfo);
			}
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
			musicManagerEntry.MusicManagerEntryUpdate();
			if (Find.World != null)
			{
				Find.World.WorldUpdate();
			}
			if (Current.Game != null)
			{
				Current.Game.UpdateEntry();
			}
		}
		catch (Exception ex)
		{
			Log.Error("Root level exception in Update(): " + ex);
		}
	}
}
