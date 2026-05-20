namespace Verse;

public static class DebugSettings
{
	public const bool DebugBuild = false;

	public static bool enableDamage = true;

	public static bool enablePlayerDamage = true;

	public static bool enableRandomMentalStates = true;

	public static bool enableStoryteller = true;

	public static bool enableRandomDiseases = true;

	public static bool enableTranslationWindowInEnglish = false;

	public static bool godMode = false;

	public static bool devPalette = false;

	public static bool pauseOnError = false;

	public static bool noAnimals = false;

	public static bool unlimitedPower = false;

	public static bool pathThroughWalls = false;

	public static bool instantRecruit = false;

	public static bool alwaysSocialFight = false;

	public static bool alwaysDoLovin = false;

	public static bool detectRegionListersBugs = false;

	public static bool instantVisitorsGift = false;

	public static bool lowFPS = false;

	public static bool allowUndraftedMechOrders = false;

	public static bool editableGlowerColors = false;

	public static bool showHiddenPawns = false;

	public static bool showHiddenInfo = false;

	public static bool anomalyDarkeningFX = true;

	public static bool fastResearch = false;

	public static bool fastLearning = false;

	public static bool fastEcology = false;

	public static bool fastEcologyRegrowRateOnly = false;

	public static bool fastCrafting = false;

	public static bool fastCaravans = false;

	public static bool fastMapUnpollution = false;

	public static bool activateAllBuildingDemands = false;

	public static bool activateAllIdeoRoles = false;

	public static bool showLocomotionUrgency = false;

	public static bool playRitualAmbience = true;

	public static bool simulateUsingSteamDeck = false;

	public static bool logRaidInfo = false;

	public static bool logTranslationLookupErrors = false;

	public static bool logPsychicRitualTransitions = false;

	public static bool fastMonolithRespawn = false;

	public static bool searchIgnoresRestrictions = false;

	public static bool alwaysRareCatches = false;

	public static bool alwaysNegativeCatches = false;

	public static bool logMismatchedLayoutFactions = false;

	public static bool loopGravshipCutscene = false;

	public static bool skipGravshipTileSelection = false;

	public static bool ignoreGravshipRange = false;

	public static bool ShowDevGizmos
	{
		get
		{
			if (Prefs.DevMode)
			{
				return godMode;
			}
			return false;
		}
	}
}
