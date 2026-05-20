using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class ResolutionUtility
{
	private static bool? borderlessFullscreenCached;

	public const int MinResolutionWidth = 1024;

	public const int MinResolutionHeight = 768;

	public const int MinRecommendedResolutionWidth = 1700;

	public const int MinRecommendedResolutionHeight = 910;

	public static Resolution NativeResolution
	{
		get
		{
			Resolution[] resolutions = Screen.resolutions;
			if (resolutions.Length == 0)
			{
				return Screen.currentResolution;
			}
			Resolution result = resolutions[0];
			for (int i = 1; i < resolutions.Length; i++)
			{
				if (resolutions[i].width > result.width || (resolutions[i].width == result.width && resolutions[i].height > result.height))
				{
					result = resolutions[i];
				}
			}
			return result;
		}
	}

	public static bool BorderlessFullscreen
	{
		get
		{
			if (!borderlessFullscreenCached.HasValue)
			{
				borderlessFullscreenCached = Environment.GetCommandLineArgs().Contains("-popupwindow");
			}
			return borderlessFullscreenCached.Value;
		}
	}

	public static void SafeSetResolution(Resolution res)
	{
		if (Screen.width != res.width || Screen.height != res.height)
		{
			IntVec2 oldRes = new IntVec2(Screen.width, Screen.height);
			SetResolutionRaw(res.width, res.height, Screen.fullScreen);
			Prefs.ScreenWidth = res.width;
			Prefs.ScreenHeight = res.height;
			Find.WindowStack.Add(new Dialog_ResolutionConfirm(oldRes));
		}
	}

	public static void SafeSetFullscreen(bool fullScreen)
	{
		if (Screen.fullScreen != fullScreen)
		{
			bool fullScreen2 = Screen.fullScreen;
			Screen.fullScreen = fullScreen;
			Prefs.FullScreen = fullScreen;
			Find.WindowStack.Add(new Dialog_ResolutionConfirm(fullScreen2));
		}
	}

	public static void SafeSetUIScale(float newScale)
	{
		if (Prefs.UIScale != newScale)
		{
			float uIScale = Prefs.UIScale;
			Prefs.UIScale = newScale;
			GenUI.ClearLabelWidthCache();
			Find.WindowStack.Add(new Dialog_ResolutionConfirm(uIScale));
		}
	}

	public static bool UIScaleSafeWithResolution(float scale, int w, int h)
	{
		if ((float)w / scale >= 1024f)
		{
			return (float)h / scale >= 768f;
		}
		return false;
	}

	public static void SetResolutionRaw(int w, int h, bool fullScreen)
	{
		if (!Application.isBatchMode)
		{
			if (w <= 0 || h <= 0)
			{
				Log.Error("Tried to set resolution to " + w + "x" + h);
			}
			else if (Screen.width != w || Screen.height != h || Screen.fullScreen != fullScreen)
			{
				Screen.SetResolution(w, h, fullScreen);
			}
		}
	}

	public static void SetNativeResolutionRaw()
	{
		Resolution nativeResolution = NativeResolution;
		SetResolutionRaw(nativeResolution.width, nativeResolution.height, !BorderlessFullscreen);
	}

	public static float GetRecommendedUIScale(int screenWidth, int screenHeight)
	{
		if (screenWidth == 0 || screenHeight == 0)
		{
			Resolution currentResolution = Screen.currentResolution;
			screenWidth = currentResolution.width;
			screenHeight = currentResolution.height;
		}
		if (screenWidth <= 1024 || screenHeight <= 768)
		{
			return 1f;
		}
		for (int num = Dialog_Options.UIScales.Length - 1; num >= 0; num--)
		{
			int num2 = Mathf.FloorToInt((float)screenWidth / Dialog_Options.UIScales[num]);
			int num3 = Mathf.FloorToInt((float)screenHeight / Dialog_Options.UIScales[num]);
			if (num2 >= 1700 && num3 >= 910)
			{
				return Dialog_Options.UIScales[num];
			}
		}
		return 1f;
	}

	public static void Update()
	{
		if (RealTime.frameCount % 30 != 0 || LongEventHandler.AnyEventNowOrWaiting)
		{
			return;
		}
		bool flag = false;
		if (!Screen.fullScreen)
		{
			if (Screen.width != Prefs.ScreenWidth)
			{
				Prefs.ScreenWidth = Screen.width;
				flag = true;
			}
			if (Screen.height != Prefs.ScreenHeight)
			{
				Prefs.ScreenHeight = Screen.height;
				flag = true;
			}
		}
		if (UI.screenWidth < 1024 && UI.screenHeight < 768 && !Prefs.DevMode)
		{
			int screenWidth = UI.screenWidth;
			int screenHeight = UI.screenHeight;
			Prefs.UIScale = GetRecommendedUIScale(0, 0);
			Log.ErrorOnce($"Resolution too small ({screenWidth}x{screenHeight}), resetting UI scale to {Prefs.UIScale}.", 72627836);
			flag = true;
		}
		if (flag)
		{
			Prefs.Save();
		}
	}
}
