using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using LudeonTK;
using RimWorld;
using UnityEngine;

namespace Verse;

public static class Prefs
{
	private static PrefsData data;

	public static float VolumeMaster
	{
		get
		{
			return data.volumeMaster;
		}
		set
		{
			if (data.volumeMaster != value)
			{
				data.volumeMaster = value;
				Apply();
			}
		}
	}

	public static float VolumeGame
	{
		get
		{
			return data.volumeGame;
		}
		set
		{
			if (data.volumeGame != value)
			{
				data.volumeGame = value;
				Apply();
			}
		}
	}

	public static float VolumeMusic
	{
		get
		{
			return data.volumeMusic;
		}
		set
		{
			if (data.volumeMusic != value)
			{
				data.volumeMusic = value;
				Apply();
			}
		}
	}

	public static float VolumeAmbient
	{
		get
		{
			return data.volumeAmbient;
		}
		set
		{
			if (data.volumeAmbient != value)
			{
				data.volumeAmbient = value;
				Apply();
			}
		}
	}

	public static float VolumeUI
	{
		get
		{
			return data.volumeUI;
		}
		set
		{
			if (data.volumeUI != value)
			{
				data.volumeUI = value;
				Apply();
			}
		}
	}

	public static bool AdaptiveTrainingEnabled
	{
		get
		{
			return data.adaptiveTrainingEnabled;
		}
		set
		{
			if (data.adaptiveTrainingEnabled != value)
			{
				data.adaptiveTrainingEnabled = value;
				Apply();
			}
		}
	}

	public static bool SteamDeckKeyboardMode
	{
		get
		{
			return data.steamDeckKeyboardMode;
		}
		set
		{
			if (data.steamDeckKeyboardMode != value)
			{
				data.steamDeckKeyboardMode = value;
				Apply();
			}
		}
	}

	public static bool EdgeScreenScroll
	{
		get
		{
			return data.edgeScreenScroll;
		}
		set
		{
			if (data.edgeScreenScroll != value)
			{
				data.edgeScreenScroll = value;
				Apply();
			}
		}
	}

	public static bool RememberDrawStlyes
	{
		get
		{
			return data.rememberDrawStyles;
		}
		set
		{
			if (data.rememberDrawStyles != value)
			{
				data.rememberDrawStyles = value;
				Apply();
			}
		}
	}

	public static bool ZoomSwitchWorldLayer
	{
		get
		{
			return data.zoomSwitchWorldLayer;
		}
		set
		{
			if (data.zoomSwitchWorldLayer != value)
			{
				data.zoomSwitchWorldLayer = value;
				Apply();
			}
		}
	}

	public static bool ZoomToMouse
	{
		get
		{
			return data.zoomToMouse;
		}
		set
		{
			if (data.zoomToMouse != value)
			{
				data.zoomToMouse = value;
				Apply();
			}
		}
	}

	public static float ScreenShakeIntensity
	{
		get
		{
			return data.screenShakeIntensity;
		}
		set
		{
			if (data.screenShakeIntensity != value)
			{
				data.screenShakeIntensity = value;
				Apply();
			}
		}
	}

	public static bool RunInBackground
	{
		get
		{
			return data.runInBackground;
		}
		set
		{
			if (data.runInBackground != value)
			{
				data.runInBackground = value;
				Apply();
			}
		}
	}

	public static TemperatureDisplayMode TemperatureMode
	{
		get
		{
			return data.temperatureMode;
		}
		set
		{
			if (data.temperatureMode != value)
			{
				data.temperatureMode = value;
				Apply();
			}
		}
	}

	public static float AutosaveIntervalDays
	{
		get
		{
			return data.autosaveIntervalDays;
		}
		set
		{
			if (data.autosaveIntervalDays != value)
			{
				data.autosaveIntervalDays = value;
				Apply();
			}
		}
	}

	public static bool CustomCursorEnabled
	{
		get
		{
			return data.customCursorEnabled;
		}
		set
		{
			if (data.customCursorEnabled != value)
			{
				data.customCursorEnabled = value;
				Apply();
			}
		}
	}

	public static DotHighlightDisplayMode DotHighlightDisplayMode
	{
		get
		{
			return data.dotHighlightDisplayMode;
		}
		set
		{
			if (data.dotHighlightDisplayMode != value)
			{
				data.dotHighlightDisplayMode = value;
				Apply();
			}
		}
	}

	public static HighlightStyleMode HighlightStyleMode
	{
		get
		{
			return data.highlightStyleMode;
		}
		set
		{
			if (data.highlightStyleMode != value)
			{
				data.highlightStyleMode = value;
				Apply();
			}
		}
	}

	public static AnimalNameDisplayMode AnimalNameMode
	{
		get
		{
			return data.animalNameMode;
		}
		set
		{
			if (data.animalNameMode != value)
			{
				data.animalNameMode = value;
				Apply();
			}
		}
	}

	public static MechNameDisplayMode MechNameMode
	{
		get
		{
			return data.mechNameMode;
		}
		set
		{
			if (data.mechNameMode != value)
			{
				data.mechNameMode = value;
				Apply();
			}
		}
	}

	public static ShowWeaponsUnderPortraitMode ShowWeaponsUnderPortraitMode
	{
		get
		{
			return data.showWeaponsUnderPortraitMode;
		}
		set
		{
			if (data.showWeaponsUnderPortraitMode != value)
			{
				data.showWeaponsUnderPortraitMode = value;
				Apply();
			}
		}
	}

	public static bool VisibleMood
	{
		get
		{
			return data.visibleMood;
		}
		set
		{
			if (data.visibleMood != value)
			{
				data.visibleMood = value;
				Apply();
			}
		}
	}

	public static bool DevMode
	{
		get
		{
			if (data == null)
			{
				return true;
			}
			return data.devMode;
		}
		set
		{
			if (data.devMode != value)
			{
				data.devMode = value;
				if (!data.devMode)
				{
					data.logVerbose = false;
					data.resetModsConfigOnCrash = true;
					DebugSettings.godMode = false;
					Find.WindowStack.TryRemove(typeof(Dialog_DevPalette));
				}
				Apply();
			}
		}
	}

	public static bool ResetModsConfigOnCrash
	{
		get
		{
			if (data == null)
			{
				return true;
			}
			return data.resetModsConfigOnCrash;
		}
		set
		{
			if (data.resetModsConfigOnCrash != value)
			{
				data.resetModsConfigOnCrash = value;
				Apply();
			}
		}
	}

	public static List<string> PreferredNames
	{
		get
		{
			return data.preferredNames;
		}
		set
		{
			if (data.preferredNames != value)
			{
				data.preferredNames = value;
				Apply();
			}
		}
	}

	public static string LangFolderName
	{
		get
		{
			return data.langFolderName;
		}
		set
		{
			if (!(data.langFolderName == value))
			{
				data.langFolderName = value;
				Apply();
			}
		}
	}

	public static bool LogVerbose
	{
		get
		{
			return data.logVerbose;
		}
		set
		{
			if (data.logVerbose != value)
			{
				data.logVerbose = value;
				Apply();
			}
		}
	}

	public static bool DisableQuickStartCryptoSickness
	{
		get
		{
			return data.disableQuickStartCryptoSickness;
		}
		set
		{
			if (data.disableQuickStartCryptoSickness != value)
			{
				data.disableQuickStartCryptoSickness = value;
				Apply();
			}
		}
	}

	public static bool StartDevPaletteOn
	{
		get
		{
			return data.quickStartDevPaletteOn;
		}
		set
		{
			if (data.quickStartDevPaletteOn != value)
			{
				data.quickStartDevPaletteOn = value;
				Apply();
			}
		}
	}

	public static bool PauseOnLoad
	{
		get
		{
			return data.pauseOnLoad;
		}
		set
		{
			data.pauseOnLoad = value;
		}
	}

	public static AutomaticPauseMode AutomaticPauseMode
	{
		get
		{
			return data.automaticPauseMode;
		}
		set
		{
			data.automaticPauseMode = value;
		}
	}

	public static bool ShowRealtimeClock
	{
		get
		{
			return data.showRealtimeClock;
		}
		set
		{
			data.showRealtimeClock = value;
		}
	}

	public static bool TwelveHourClockMode
	{
		get
		{
			return data.twelveHourClock;
		}
		set
		{
			data.twelveHourClock = value;
		}
	}

	public static bool DisableTinyText
	{
		get
		{
			return data.disableTinyText;
		}
		set
		{
			data.disableTinyText = value;
		}
	}

	public static bool TestMapSizes
	{
		get
		{
			return data.testMapSizes;
		}
		set
		{
			data.testMapSizes = value;
		}
	}

	public static int MaxNumberOfPlayerSettlements
	{
		get
		{
			return data.maxNumberOfPlayerSettlements;
		}
		set
		{
			data.maxNumberOfPlayerSettlements = value;
		}
	}

	public static bool PlantWindSway
	{
		get
		{
			return data.plantWindSway;
		}
		set
		{
			data.plantWindSway = value;
		}
	}

	public static bool TextureCompression
	{
		get
		{
			return data.textureCompression;
		}
		set
		{
			data.textureCompression = value;
		}
	}

	public static bool ResourceReadoutCategorized
	{
		get
		{
			return data.resourceReadoutCategorized;
		}
		set
		{
			if (value != data.resourceReadoutCategorized)
			{
				data.resourceReadoutCategorized = value;
				Save();
			}
		}
	}

	public static float UIScale
	{
		get
		{
			return data.uiScale;
		}
		set
		{
			data.uiScale = value;
		}
	}

	public static int ScreenWidth
	{
		get
		{
			return data.screenWidth;
		}
		set
		{
			data.screenWidth = value;
		}
	}

	public static int ScreenHeight
	{
		get
		{
			return data.screenHeight;
		}
		set
		{
			data.screenHeight = value;
		}
	}

	public static bool FullScreen
	{
		get
		{
			return data.fullscreen;
		}
		set
		{
			data.fullscreen = value;
		}
	}

	public static bool HatsOnlyOnMap
	{
		get
		{
			return data.hatsOnlyOnMap;
		}
		set
		{
			if (data.hatsOnlyOnMap != value)
			{
				data.hatsOnlyOnMap = value;
				Apply();
			}
		}
	}

	public static float MapDragSensitivity
	{
		get
		{
			return data.mapDragSensitivity;
		}
		set
		{
			data.mapDragSensitivity = value;
		}
	}

	public static ExpansionDef BackgroundImageExpansion
	{
		get
		{
			if (data.backgroundExpansionId != null)
			{
				ExpansionDef expansionWithIdentifier = ModLister.GetExpansionWithIdentifier(data.backgroundExpansionId);
				if (expansionWithIdentifier != null && expansionWithIdentifier.Status != ExpansionStatus.NotInstalled)
				{
					return expansionWithIdentifier;
				}
			}
			ExpansionDef lastInstalledExpansion = ModsConfig.LastInstalledExpansion;
			if (lastInstalledExpansion != null)
			{
				return lastInstalledExpansion;
			}
			return ExpansionDefOf.Core;
		}
		set
		{
			data.backgroundExpansionId = value?.linkedMod;
			((UI_BackgroundMain)UIMenuBackgroundManager.background).overrideBGImage = value?.BackgroundImage;
		}
	}

	public static bool RandomBackgroundImage
	{
		get
		{
			return data.randomBackground;
		}
		set
		{
			data.randomBackground = value;
		}
	}

	public static List<string> DebugActionsPalette
	{
		get
		{
			return data.debugActionPalette;
		}
		set
		{
			if (data.debugActionPalette != value)
			{
				data.debugActionPalette = value;
				Save();
			}
		}
	}

	public static Vector2 DevPalettePosition
	{
		get
		{
			return data.devPalettePosition;
		}
		set
		{
			if (data.devPalettePosition != value)
			{
				data.devPalettePosition = value;
				Save();
			}
		}
	}

	public static bool SmoothCameraJumps
	{
		get
		{
			return data.smoothCameraJumps;
		}
		set
		{
			if (data.smoothCameraJumps != value)
			{
				data.smoothCameraJumps = value;
				Apply();
			}
		}
	}

	public static bool GravshipCutscenes
	{
		get
		{
			return data.gravshipCutscenes;
		}
		set
		{
			if (data.gravshipCutscenes != value)
			{
				data.gravshipCutscenes = value;
				Apply();
			}
		}
	}

	public static bool OpenLogOnWarnings
	{
		get
		{
			return data?.openLogOnWarnings ?? false;
		}
		set
		{
			if (data.openLogOnWarnings != value)
			{
				data.openLogOnWarnings = value;
				Apply();
			}
		}
	}

	public static bool CloseLogWindowOnEscape
	{
		get
		{
			return data.closeLogWindowOnEscape;
		}
		set
		{
			if (data.closeLogWindowOnEscape != value)
			{
				data.closeLogWindowOnEscape = value;
				Apply();
			}
		}
	}

	public static int AutosavesCount
	{
		get
		{
			return data.autosavesCount;
		}
		set
		{
			if (data.autosavesCount != value)
			{
				data.autosavesCount = value;
				Apply();
			}
		}
	}

	public static void Init()
	{
		bool num = !new FileInfo(GenFilePaths.PrefsFilePath).Exists;
		data = new PrefsData();
		data = DirectXmlLoader.ItemFromXmlFile<PrefsData>(GenFilePaths.PrefsFilePath);
		BackCompatibility.PrefsDataPostLoad(data);
		if (num)
		{
			data.langFolderName = LanguageDatabase.SystemLanguageFolderName();
			data.uiScale = ResolutionUtility.GetRecommendedUIScale(data.screenWidth, data.screenHeight);
		}
		if (DevModePermanentlyDisabledUtility.Disabled)
		{
			DevMode = false;
		}
		Apply();
	}

	public static void Save()
	{
		try
		{
			XDocument xDocument = new XDocument();
			XElement content = DirectXmlSaver.XElementFromObject(data, typeof(PrefsData));
			xDocument.Add(content);
			xDocument.Save(GenFilePaths.PrefsFilePath);
		}
		catch (Exception ex)
		{
			GenUI.ErrorDialog("ProblemSavingFile".Translate(GenFilePaths.PrefsFilePath, ex.ToString()));
			Log.Error("Exception saving prefs: " + ex);
		}
	}

	public static void Apply()
	{
		data.Apply();
	}

	public static void Notify_NewExpansion()
	{
		data.backgroundExpansionId = null;
	}

	public static NameTriple RandomPreferredName()
	{
		if (PreferredNames.Where((string name) => !name.NullOrEmpty()).TryRandomElement(out var result))
		{
			return NameTriple.FromString(result);
		}
		return null;
	}
}
