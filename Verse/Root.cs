using RimWorld;
using RimWorld.Planet;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Analytics;
using Verse.AI;
using Verse.Sound;
using Verse.Steam;

namespace Verse
{
	public abstract class Root : MonoBehaviour
	{
		private static bool globalInitDone;

		private static bool prefsApplied;

		protected static bool checkedAutostartSaveFile;

		protected bool destroyed;

		public SoundRoot soundRoot;

		public UIRoot uiRoot;

		public virtual void Start()
		{
			try
			{
				CultureInfoUtility.EnsureEnglish();
				Current.Notify_LoadedSceneChanged();
				CheckGlobalInit();
				Action action = delegate
				{
					DeepProfiler.Start("Misc Init (InitializingInterface)");
					try
					{
						soundRoot = new SoundRoot();
						if (GenScene.InPlayScene)
						{
							uiRoot = new UIRoot_Play();
						}
						else if (GenScene.InEntryScene)
						{
							uiRoot = new UIRoot_Entry();
						}
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
			catch (Exception arg)
			{
				Log.Error("Critical error in root Start(): " + arg);
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
				Application.targetFrameRate = 60;
				UnityDataInitializer.CopyUnityData();
				SteamManager.InitIfNeeded();
				VersionControl.LogVersionNumber();
				Prefs.Init();
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
				LongEventHandler.LongEventsUpdate(out bool sceneChanged);
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
			catch (Exception arg)
			{
				Log.Error("Root level exception in Update(): " + arg);
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
			catch (Exception arg)
			{
				Log.Error("Root level exception in OnGUI(): " + arg);
			}
		}

		public static void Shutdown()
		{
			SteamManager.ShutdownSteam();
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
			Application.Quit();
		}
	}
}
