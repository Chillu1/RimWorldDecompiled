using System;
using System.IO;
using RimWorld;
using Verse.Profile;

namespace Verse;

public static class GameDataSaveLoader
{
	private static int lastSaveTick = -9999;

	private static bool isSavingOrLoadingExternalIdeo;

	public const string SavedScenarioParentNodeName = "savedscenario";

	public const string SavedWorldParentNodeName = "savedworld";

	public const string SavedGameParentNodeName = "savegame";

	public const string SavedIdeoParentNodeName = "savedideo";

	public const string SavedXenotypeParentNodeName = "savedXenotype";

	public const string SavedXenogermParentNode = "savedXenogerm";

	public const string SavedModListParentNodeName = "savedModList";

	public const string SavedCameraConfigParentNodeName = "cameraConfig";

	public const string GameNodeName = "game";

	public const string WorldNodeName = "world";

	public const string ScenarioNodeName = "scenario";

	public const string IdeoNodeName = "ideo";

	public const string XenotypeNodeName = "xenotype";

	public const string XenogermNodeName = "xenogerm";

	public const string ModListNodeName = "modList";

	public const string CameraConfigNodeName = "camConfig";

	public const string AutosavePrefix = "Autosave";

	public const string AutostartSaveName = "autostart";

	public static bool CurrentGameStateIsValuable => Find.TickManager.TicksGame > lastSaveTick + 60;

	public static bool SavingIsTemporarilyDisabled
	{
		get
		{
			if (!Find.TilePicker.Active && !Find.WindowStack.WindowsPreventSave)
			{
				return WorldComponent_GravshipController.CutsceneInProgress;
			}
			return true;
		}
	}

	public static bool IsSavingOrLoadingExternalIdeo => isSavingOrLoadingExternalIdeo;

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
		using (Log.LockMessages())
		{
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
			catch (Exception arg)
			{
				Log.Error($"Exception loading scenario: {arg}");
				scen = null;
				Scribe.ForceStop();
			}
			if (scen == null)
			{
				return false;
			}
			for (int num = scen.parts.Count - 1; num >= 0; num--)
			{
				if (scen.parts[num]?.def == null || scen.parts[num].HasNullDefs())
				{
					scen.parts.RemoveAt(num);
					scen.valid = false;
				}
			}
			return true;
		}
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
		catch (Exception ex)
		{
			Log.Error("Exception while saving game: " + ex);
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
		LongEventHandler.QueueLongEvent(PreLoadAct, "Play", "LoadingLongEvent", doAsynchronously: true, null);
		Current.Game?.Dispose();
		void PreLoadAct()
		{
			MemoryUtility.ClearAllMapsAndWorld();
			Current.Game = new Game();
			Current.Game.InitData = new GameInitData
			{
				gameToLoad = saveFileName
			};
		}
	}

	public static void LoadGame(FileInfo saveFile)
	{
		LoadGame(Path.GetFileNameWithoutExtension(saveFile.Name));
	}

	public static void SaveIdeo(Ideo ideo, string absFilePath)
	{
		try
		{
			isSavingOrLoadingExternalIdeo = true;
			ideo.fileName = Path.GetFileNameWithoutExtension(absFilePath);
			SafeSaver.Save(absFilePath, "savedideo", delegate
			{
				ScribeMetaHeaderUtility.WriteMetaHeader();
				Scribe_Deep.Look(ref ideo, "ideo");
			});
		}
		catch (Exception ex)
		{
			Log.Error("Exception while saving ideo: " + ex.ToString());
		}
		finally
		{
			isSavingOrLoadingExternalIdeo = false;
		}
	}

	public static bool TryLoadIdeo(string absPath, out Ideo ideo)
	{
		ideo = null;
		try
		{
			isSavingOrLoadingExternalIdeo = true;
			Scribe.loader.InitLoading(absPath);
			try
			{
				ScribeMetaHeaderUtility.LoadGameDataHeader(ScribeMetaHeaderUtility.ScribeHeaderMode.Ideo, logVersionConflictWarning: true);
				Scribe_Deep.Look(ref ideo, "ideo");
				Scribe.loader.FinalizeLoading();
			}
			catch
			{
				Scribe.ForceStop();
				throw;
			}
			ideo.fileName = Path.GetFileNameWithoutExtension(new FileInfo(absPath).Name);
		}
		catch (Exception ex)
		{
			Log.Error("Exception loading ideo: " + ex.ToString());
			ideo = null;
			Scribe.ForceStop();
		}
		finally
		{
			isSavingOrLoadingExternalIdeo = false;
		}
		return ideo != null;
	}

	public static void SaveXenotype(CustomXenotype xenotype, string absFilePath)
	{
		try
		{
			xenotype.fileName = Path.GetFileNameWithoutExtension(absFilePath);
			SafeSaver.Save(absFilePath, "savedXenotype", delegate
			{
				ScribeMetaHeaderUtility.WriteMetaHeader();
				Scribe_Deep.Look(ref xenotype, "xenotype");
			});
		}
		catch (Exception ex)
		{
			Log.Error("Exception while saving xenotype: " + ex.ToString());
		}
	}

	public static bool TryLoadXenotype(string absPath, out CustomXenotype xenotype)
	{
		xenotype = null;
		try
		{
			Scribe.loader.InitLoading(absPath);
			try
			{
				ScribeMetaHeaderUtility.LoadGameDataHeader(ScribeMetaHeaderUtility.ScribeHeaderMode.Xenotype, logVersionConflictWarning: true);
				Scribe_Deep.Look(ref xenotype, "xenotype");
				Scribe.loader.FinalizeLoading();
			}
			catch
			{
				Scribe.ForceStop();
				throw;
			}
			xenotype.fileName = Path.GetFileNameWithoutExtension(new FileInfo(absPath).Name);
		}
		catch (Exception ex)
		{
			Log.Error("Exception loading xenotype: " + ex.ToString());
			xenotype = null;
			Scribe.ForceStop();
		}
		return xenotype != null;
	}

	public static void SaveModList(ModList modList, string absFilePath)
	{
		try
		{
			modList.fileName = Path.GetFileNameWithoutExtension(absFilePath);
			SafeSaver.Save(absFilePath, "savedModList", delegate
			{
				ScribeMetaHeaderUtility.WriteMetaHeader();
				Scribe_Deep.Look(ref modList, "modList");
			});
		}
		catch (Exception ex)
		{
			Log.Error("Exception while saving mod list: " + ex);
		}
	}

	public static bool TryLoadModList(string absPath, out ModList modList)
	{
		modList = null;
		try
		{
			Scribe.loader.InitLoading(absPath);
			try
			{
				ScribeMetaHeaderUtility.LoadGameDataHeader(ScribeMetaHeaderUtility.ScribeHeaderMode.ModList, logVersionConflictWarning: true);
				Scribe_Deep.Look(ref modList, "modList");
				Scribe.loader.FinalizeLoading();
			}
			catch
			{
				Scribe.ForceStop();
				throw;
			}
			modList.fileName = Path.GetFileNameWithoutExtension(new FileInfo(absPath).Name);
		}
		catch (Exception ex)
		{
			Log.Error("Exception loading mod list: " + ex.ToString());
			modList = null;
			Scribe.ForceStop();
		}
		return modList != null;
	}

	public static void SaveCameraConfig(CameraMapConfig config, string absFilePath)
	{
		try
		{
			config.fileName = Path.GetFileNameWithoutExtension(absFilePath);
			SafeSaver.Save(absFilePath, "cameraConfig", delegate
			{
				ScribeMetaHeaderUtility.WriteMetaHeader();
				Scribe_Deep.Look(ref config, "camConfig");
			});
		}
		catch (Exception ex)
		{
			Log.Error("Exception while saving camera config: " + ex.ToString());
		}
	}

	public static bool TryLoadCameraConfig(string absPath, out CameraMapConfig config)
	{
		config = null;
		try
		{
			Scribe.loader.InitLoading(absPath);
			try
			{
				ScribeMetaHeaderUtility.LoadGameDataHeader(ScribeMetaHeaderUtility.ScribeHeaderMode.CameraConfig, logVersionConflictWarning: true);
				Scribe_Deep.Look(ref config, "camConfig");
				Scribe.loader.FinalizeLoading();
			}
			catch
			{
				Scribe.ForceStop();
				throw;
			}
			config.fileName = Path.GetFileNameWithoutExtension(new FileInfo(absPath).Name);
		}
		catch (Exception ex)
		{
			Log.Error("Exception loading camera config: " + ex.ToString());
			config = null;
			Scribe.ForceStop();
		}
		return config != null;
	}
}
