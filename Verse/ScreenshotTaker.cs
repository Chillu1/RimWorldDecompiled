using RimWorld;
using Steamworks;
using System;
using System.IO;
using UnityEngine;
using Verse.Steam;

namespace Verse
{
	public static class ScreenshotTaker
	{
		private static int lastShotFrame = -999;

		private static int screenshotCount = 0;

		private static string lastShotFilePath;

		private static bool suppressMessage;

		private static bool takeScreenshot;

		public static void Update()
		{
			if (LongEventHandler.ShouldWaitForEvent)
			{
				return;
			}
			if (KeyBindingDefOf.TakeScreenshot.JustPressed || takeScreenshot)
			{
				TakeShot();
				takeScreenshot = false;
			}
			if (Time.frameCount == lastShotFrame + 1)
			{
				if (suppressMessage)
				{
					suppressMessage = false;
				}
				else
				{
					Messages.Message("MessageScreenshotSavedAs".Translate(lastShotFilePath), MessageTypeDefOf.TaskCompletion, historical: false);
				}
			}
		}

		public static void QueueSilentScreenshot()
		{
			takeScreenshot = (suppressMessage = true);
		}

		private static void TakeShot()
		{
			if (SteamManager.Initialized && SteamUtils.IsOverlayEnabled())
			{
				try
				{
					SteamScreenshots.TriggerScreenshot();
				}
				catch (Exception arg)
				{
					Log.Warning("Could not take Steam screenshot. Steam offline? Taking normal screenshot. Exception: " + arg);
					TakeNonSteamShot();
				}
			}
			else
			{
				TakeNonSteamShot();
			}
		}

		private static void TakeNonSteamShot()
		{
			string screenshotFolderPath = GenFilePaths.ScreenshotFolderPath;
			try
			{
				DirectoryInfo directoryInfo = new DirectoryInfo(screenshotFolderPath);
				if (!directoryInfo.Exists)
				{
					directoryInfo.Create();
				}
				string text;
				do
				{
					screenshotCount++;
					text = screenshotFolderPath + Path.DirectorySeparatorChar.ToString() + "screenshot" + screenshotCount + ".png";
				}
				while (File.Exists(text));
				ScreenCapture.CaptureScreenshot(text);
				lastShotFrame = Time.frameCount;
				lastShotFilePath = text;
			}
			catch (Exception ex)
			{
				Log.Error("Failed to save screenshot in " + screenshotFolderPath + "\nException follows:\n" + ex.ToString());
			}
		}
	}
}
