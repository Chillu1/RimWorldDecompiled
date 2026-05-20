using System;
using System.IO;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using UnityEngine.Analytics;
using Verse.AI;
using Verse.Sound;
using Verse.Steam;

namespace Verse;

public abstract class Root : MonoBehaviour
{
	private static bool globalInitDone;

	private static bool prefsApplied;

	protected static bool checkedAutostartSaveFile;

	protected bool destroyed;

	private static readonly SimpleMovingAverage rawAverageDeltaTime = new SimpleMovingAverage(200);

	private static readonly ExponentialMovingAverage smoothedAverageDeltaTime = new ExponentialMovingAverage(0.1f);

	public SoundRoot soundRoot;

	public UIRoot uiRoot;

	public static float AverageFrameTime => smoothedAverageDeltaTime.GetAverage();

	public virtual void Start()
	{
		try
		{
			CultureInfoUtility.EnsureEnglish();
			Current.Notify_LoadedSceneChanged();
			GlobalTextureAtlasManager.FreeAllRuntimeAtlases();
			CheckGlobalInit();
			Action action = delegate
			{
				DeepProfiler.Start("Misc Init (InitializingInterface)");
				try
				{
					soundRoot = new SoundRoot();
					DeepProfiler.Start("Instantiate UIRoot");
					if (GenScene.InPlayScene)
					{
						uiRoot = new UIRoot_Play();
					}
					else if (GenScene.InEntryScene)
					{
						uiRoot = new UIRoot_Entry();
					}
					DeepProfiler.End();
					uiRoot.Init();
					Messages.Notify_LoadedLevelChanged();
					if (Current.SubcameraDriver != null)
					{
						Current.SubcameraDriver.Init();
					}
				}
				finally
				{
					DeepProfiler.End();
				}
			};
			if (!PlayDataLoader.Loaded)
			{
				Application.runInBackground = true;
				LongEventHandler.QueueLongEvent(delegate
				{
					PlayDataLoader.LoadAllPlayData();
				}, null, doAsynchronously: true, null);
				LongEventHandler.QueueLongEvent(action, "InitializingInterface", doAsynchronously: false, null);
			}
			else
			{
				action();
			}
		}
		catch (Exception ex)
		{
			Log.Error("Critical error in root Start(): " + ex);
		}
	}

	private static void CheckGlobalInit()
	{
		if (!globalInitDone)
		{
			string[] commandLineArgs = Environment.GetCommandLineArgs();
			if (commandLineArgs != null && commandLineArgs.Length > 1)
			{
				Log.Message("Command line arguments: " + GenText.ToSpaceList(commandLineArgs.Skip(1)));
			}
			PerformanceReporting.enabled = false;
			Application.targetFrameRate = Mathf.RoundToInt((float)Screen.currentResolution.refreshRateRatio.value);
			UnityDataInitializer.CopyUnityData();
			SteamManager.InitIfNeeded();
			VersionControl.LogVersionNumber();
			Prefs.Init();
			Application.logMessageReceivedThreaded += Log.Notify_MessageReceivedThreadedInternal;
			if (Prefs.DevMode)
			{
				StaticConstructorOnStartupUtility.ReportProbablyMissingAttributes();
			}
			LongEventHandler.QueueLongEvent(StaticConstructorOnStartupUtility.CallAll, null, doAsynchronously: false, null);
			globalInitDone = true;
		}
	}

	public virtual void Update()
	{
		try
		{
			ResolutionUtility.Update();
			RealTime.Update();
			rawAverageDeltaTime.AddValue(RealTime.deltaTime * 1000f);
			smoothedAverageDeltaTime.AddValue(rawAverageDeltaTime.GetAverage());
			LongEventHandler.LongEventsUpdate(out var sceneChanged);
			if (sceneChanged)
			{
				destroyed = true;
			}
			else if (!LongEventHandler.ShouldWaitForEvent)
			{
				Rand.EnsureStateStackEmpty();
				Widgets.EnsureMousePositionStackEmpty();
				SteamManager.Update();
				PortraitsCache.PortraitsCacheUpdate();
				AttackTargetsCache.AttackTargetsCacheStaticUpdate();
				Pawn_MeleeVerbs.PawnMeleeVerbsStaticUpdate();
				Storyteller.StorytellerStaticUpdate();
				CaravanInventoryUtility.CaravanInventoryUtilityStaticUpdate();
				uiRoot.UIRootUpdate();
				if (Time.frameCount > 3 && !prefsApplied)
				{
					prefsApplied = true;
					Prefs.Apply();
				}
				soundRoot.Update();
			}
		}
		catch (Exception ex)
		{
			Log.Error("Root level exception in Update(): " + ex);
		}
	}

	public void OnGUI()
	{
		try
		{
			if (destroyed)
			{
				return;
			}
			GUI.depth = 50;
			UI.ApplyUIScale();
			LongEventHandler.LongEventsOnGUI();
			if (LongEventHandler.ShouldWaitForEvent)
			{
				ScreenFader.OverlayOnGUI(new Vector2(UI.screenWidth, UI.screenHeight));
				return;
			}
			uiRoot.UIRootOnGUI();
			ScreenFader.OverlayOnGUI(new Vector2(UI.screenWidth, UI.screenHeight));
			if (Find.CameraDriver != null && Find.CameraDriver.isActiveAndEnabled)
			{
				Find.CameraDriver.CameraDriverOnGUI();
			}
			if (Find.WorldCameraDriver != null && Find.WorldCameraDriver.isActiveAndEnabled)
			{
				Find.WorldCameraDriver.WorldCameraDriverOnGUI();
			}
		}
		catch (Exception ex)
		{
			Log.Error("Root level exception in OnGUI(): " + ex);
		}
	}

	public static void Shutdown()
	{
		try
		{
			SteamManager.ShutdownSteam();
		}
		catch (Exception ex)
		{
			Log.Error("Error in ShutdownSteam(): " + ex);
		}
		try
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(GenFilePaths.TempFolderPath);
			FileInfo[] files = directoryInfo.GetFiles();
			for (int i = 0; i < files.Length; i++)
			{
				files[i].Delete();
			}
			DirectoryInfo[] directories = directoryInfo.GetDirectories();
			for (int i = 0; i < directories.Length; i++)
			{
				directories[i].Delete(recursive: true);
			}
		}
		catch (Exception ex2)
		{
			Log.Error("Could not delete temporary files: " + ex2);
		}
		Application.Quit();
	}
}
