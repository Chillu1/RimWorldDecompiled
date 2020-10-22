namespace Verse
{
	public static class DebugViewSettings
	{
		public static bool drawFog = true;

		public static bool drawSnow = true;

		public static bool drawTerrain = true;

		public static bool drawTerrainWater = true;

		public static bool drawThingsDynamic = true;

		public static bool drawThingsPrinted = true;

		public static bool drawShadows = true;

		public static bool drawLightingOverlay = true;

		public static bool drawWorldOverlays = true;

		public static bool drawPaths = false;

		public static bool drawCastPositionSearch = false;

		public static bool drawDestSearch = false;

		public static bool drawSectionEdges = false;

		public static bool drawRiverDebug = false;

		public static bool drawPawnDebug = false;

		public static bool drawPawnRotatorTarget = false;

		public static bool drawRegions = false;

		public static bool drawRegionLinks = false;

		public static bool drawRegionDirties = false;

		public static bool drawRegionTraversal = false;

		public static bool drawRegionThings = false;

		public static bool drawRooms = false;

		public static bool drawRoomGroups = false;

		public static bool drawPower = false;

		public static bool drawPowerNetGrid = false;

		public static bool drawOpportunisticJobs = false;

		public static bool drawTooltipEdges = false;

		public static bool drawRecordedNoise = false;

		public static bool drawFoodSearchFromMouse = false;

		public static bool drawPreyInfo = false;

		public static bool drawGlow = false;

		public static bool drawAvoidGrid = false;

		public static bool drawLords = false;

		public static bool drawDuties = false;

		public static bool drawShooting = false;

		public static bool drawInfestationChance = false;

		public static bool drawStealDebug = false;

		public static bool drawDeepResources = false;

		public static bool drawAttackTargetScores = false;

		public static bool drawInteractionCells = false;

		public static bool drawDoorsDebug = false;

		public static bool drawDestReservations = false;

		public static bool drawDamageRects = false;

		public static bool writeGame = false;

		public static bool writeSteamItems = false;

		public static bool writeConcepts = false;

		public static bool writeReservations = false;

		public static bool writePathCosts = false;

		public static bool writeFertility = false;

		public static bool writeLinkFlags = false;

		public static bool writeCover = false;

		public static bool writeCellContents = false;

		public static bool writeMusicManagerPlay = false;

		public static bool writeStoryteller = false;

		public static bool writePlayingSounds = false;

		public static bool writeSoundEventsRecord = false;

		public static bool writeMoteSaturation = false;

		public static bool writeSnowDepth = false;

		public static bool writeEcosystem = false;

		public static bool writeRecentStrikes = false;

		public static bool writeBeauty = false;

		public static bool writeListRepairableBldgs = false;

		public static bool writeListFilthInHomeArea = false;

		public static bool writeListHaulables = false;

		public static bool writeListMergeables = false;

		public static bool writeTotalSnowDepth = false;

		public static bool writeCanReachColony = false;

		public static bool writeMentalStateCalcs = false;

		public static bool writeWind = false;

		public static bool writeTerrain = false;

		public static bool writeApparelScore = false;

		public static bool writeWorkSettings = false;

		public static bool writeSkyManager = false;

		public static bool writeMemoryUsage = false;

		public static bool writeMapGameConditions = false;

		public static bool writeAttackTargets = false;

		public static bool logIncapChance = false;

		public static bool logInput = false;

		public static bool logApparelGeneration = false;

		public static bool logLordToilTransitions = false;

		public static bool logGrammarResolution = false;

		public static bool logCombatLogMouseover = false;

		public static bool logCauseOfDeath = false;

		public static bool logMapLoad = false;

		public static bool logTutor = false;

		public static bool logSignals = false;

		public static bool logWorldPawnGC = false;

		public static bool logTaleRecording = false;

		public static bool logHourlyScreenshot = false;

		public static bool logFilthSummary = false;

		public static bool debugApparelOptimize = false;

		public static bool showAllRoomStats = false;

		public static bool showFloatMenuWorkGivers = false;

		public static void drawTerrainWaterToggled()
		{
			if (Find.CurrentMap != null)
			{
				Find.CurrentMap.mapDrawer.WholeMapChanged(MapMeshFlag.Terrain);
			}
		}

		public static void drawShadowsToggled()
		{
			if (Find.CurrentMap != null)
			{
				Find.CurrentMap.mapDrawer.WholeMapChanged((MapMeshFlag)(-1));
			}
		}
	}
}
