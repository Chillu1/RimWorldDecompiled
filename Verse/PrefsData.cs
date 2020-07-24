using RimWorld;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class PrefsData
	{
		public float volumeGame = 0.8f;

		public float volumeMusic = 0.4f;

		public float volumeAmbient = 1f;

		public int screenWidth;

		public int screenHeight;

		public bool fullscreen;

		public float uiScale = 1f;

		public bool customCursorEnabled = true;

		public bool hatsOnlyOnMap;

		public bool plantWindSway = true;

		public bool showRealtimeClock;

		public AnimalNameDisplayMode animalNameMode;

		public bool extremeDifficultyUnlocked;

		public bool adaptiveTrainingEnabled = true;

		public List<string> preferredNames = new List<string>();

		public bool resourceReadoutCategorized;

		public bool runInBackground;

		public bool edgeScreenScroll = true;

		public TemperatureDisplayMode temperatureMode;

		public float autosaveIntervalDays = 1f;

		public bool testMapSizes;

		[LoadAlias("maxNumberOfPlayerHomes")]
		public int maxNumberOfPlayerSettlements = 1;

		public bool pauseOnLoad;

		public AutomaticPauseMode automaticPauseMode = AutomaticPauseMode.MajorThreat;

		public float mapDragSensitivity = 1.3f;

		[Unsaved(true)]
		public bool? pauseOnUrgentLetter;

		public bool devMode;

		public string langFolderName = "unknown";

		public bool logVerbose;

		public bool pauseOnError;

		public bool resetModsConfigOnCrash = true;

		public bool simulateNotOwningRoyalty;

		public void Apply()
		{
			if (UnityData.IsInMainThread)
			{
				if (customCursorEnabled)
				{
					CustomCursor.Activate();
				}
				else
				{
					CustomCursor.Deactivate();
				}
				AudioListener.volume = volumeGame;
				Application.runInBackground = runInBackground;
				if (screenWidth == 0 || screenHeight == 0)
				{
					ResolutionUtility.SetNativeResolutionRaw();
				}
				else
				{
					ResolutionUtility.SetResolutionRaw(screenWidth, screenHeight, fullscreen);
				}
			}
		}
	}
}
