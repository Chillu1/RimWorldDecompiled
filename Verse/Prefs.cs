using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using RimWorld;

namespace Verse
{
	public static class Prefs
	{
		private static PrefsData data;

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

		[Obsolete]
		public static bool ExtremeDifficultyUnlocked
		{
			get
			{
				return data.extremeDifficultyUnlocked;
			}
			set
			{
				if (data.extremeDifficultyUnlocked != value)
				{
					data.extremeDifficultyUnlocked = value;
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

		public static bool SimulateNotOwningRoyalty
		{
			get
			{
				if (data == null)
				{
					return true;
				}
				return data.simulateNotOwningRoyalty;
			}
			set
			{
				if (data.simulateNotOwningRoyalty != value)
				{
					data.simulateNotOwningRoyalty = value;
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

		public static bool PauseOnError
		{
			get
			{
				if (data == null)
				{
					return false;
				}
				return data.pauseOnError;
			}
			set
			{
				data.pauseOnError = value;
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

		public static NameTriple RandomPreferredName()
		{
			if (PreferredNames.Where((string name) => !name.NullOrEmpty()).TryRandomElement(out var result))
			{
				return NameTriple.FromString(result);
			}
			return null;
		}
	}
}
