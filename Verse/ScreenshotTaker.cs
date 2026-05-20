using System;
using System.IO;
using RimWorld;
using Steamworks;
using UnityEngine;
using Verse.Steam;

namespace Verse;

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
				return;
			}
			catch (Exception ex)
			{
				Log.Warning("Could not take Steam screenshot. Steam offline? Taking normal screenshot. Exception: " + ex);
				TakeNonSteamShot();
				return;
			}
		}
		TakeNonSteamShot();
	}

	public static void TakeNonSteamShot(string fileName = null)
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
			if (fileName != null)
			{
				text = $"{screenshotFolderPath}{Path.DirectorySeparatorChar}{fileName}.png";
			}
			else
			{
				do
				{
					screenshotCount++;
					string[] obj = new string[5] { screenshotFolderPath, null, null, null, null };
					char directorySeparatorChar = Path.DirectorySeparatorChar;
					obj[1] = directorySeparatorChar.ToString();
					obj[2] = "screenshot";
					obj[3] = screenshotCount.ToString();
					obj[4] = ".png";
					text = string.Concat(obj);
				}
				while (File.Exists(text));
			}
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
