using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using RimWorld;
using UnityEngine;
using Verse.AI.Group;

namespace Verse;

public static class BackCompatibility
{
	public static readonly Pair<int, int>[] SaveCompatibleMinorVersions = new Pair<int, int>[1]
	{
		new Pair<int, int>(17, 18)
	};

	private static List<BackCompatibilityConverter> conversionChain = new List<BackCompatibilityConverter>
	{
		new BackCompatibilityConverter_0_17_AndLower(),
		new BackCompatibilityConverter_0_18(),
		new BackCompatibilityConverter_0_19(),
		new BackCompatibilityConverter_1_0(),
		new BackCompatibilityConverter_1_2(),
		new BackCompatibilityConverter_1_3(),
		new BackCompatibilityConverter_1_4(),
		new BackCompatibilityConverter_1_5(),
		new BackCompatibilityConverter_Universal()
	};

	private static readonly List<Tuple<string, Type>> RemovedDefs = new List<Tuple<string, Type>>
	{
		new Tuple<string, Type>("PsychicSilencer", typeof(HediffDef)),
		new Tuple<string, Type>("PsychicSilencer", typeof(ThingDef)),
		new Tuple<string, Type>("Gun_Slugthrower", typeof(ThingDef)),
		new Tuple<string, Type>("LazyWorker78", typeof(BackstoryDef)),
		new Tuple<string, Type>("ExMilitary9", typeof(BackstoryDef)),
		new Tuple<string, Type>("InsuranceBroker9", typeof(BackstoryDef)),
		new Tuple<string, Type>("StreetUrchin22", typeof(BackstoryDef)),
		new Tuple<string, Type>("CardCounter25", typeof(BackstoryDef)),
		new Tuple<string, Type>("CaesicMarshal72", typeof(BackstoryDef)),
		new Tuple<string, Type>("GrownMate56", typeof(BackstoryDef)),
		new Tuple<string, Type>("DepartmentHead61", typeof(BackstoryDef)),
		new Tuple<string, Type>("ArmyCommander14", typeof(BackstoryDef)),
		new Tuple<string, Type>("DivorceKid95", typeof(BackstoryDef)),
		new Tuple<string, Type>("PetTorturer60", typeof(BackstoryDef)),
		new Tuple<string, Type>("PoliticalClimber28", typeof(BackstoryDef)),
		new Tuple<string, Type>("NoInteraction", typeof(PrisonerInteractionModeDef)),
		new Tuple<string, Type>("Study", typeof(DesignationDef)),
		new Tuple<string, Type>("Mote_HateChant", typeof(ThingDef)),
		new Tuple<string, Type>("GeneticChemicalDependency", typeof(ThoughtDef))
	};

	private static List<Thing> tmpThingsToSpawnLater = new List<Thing>();

	public static bool IsSaveCompatibleWith(string version)
	{
		if (VersionControl.MajorFromVersionString(version) == VersionControl.CurrentMajor && VersionControl.MinorFromVersionString(version) == VersionControl.CurrentMinor)
		{
			return true;
		}
		if (VersionControl.MajorFromVersionString(version) >= 1 && VersionControl.MajorFromVersionString(version) == VersionControl.CurrentMajor && VersionControl.MinorFromVersionString(version) <= VersionControl.CurrentMinor)
		{
			return true;
		}
		if (VersionControl.MajorFromVersionString(version) == 0 && VersionControl.CurrentMajor == 0)
		{
			int num = VersionControl.MinorFromVersionString(version);
			int currentMinor = VersionControl.CurrentMinor;
			for (int i = 0; i < SaveCompatibleMinorVersions.Length; i++)
			{
				if (SaveCompatibleMinorVersions[i].First == num && SaveCompatibleMinorVersions[i].Second == currentMinor)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static void PreLoadSavegame(string loadingVersion)
	{
		for (int i = 0; i < conversionChain.Count; i++)
		{
			if (conversionChain[i].AppliesToLoadedGameVersion(allowInactiveScribe: true))
			{
				try
				{
					conversionChain[i].PreLoadSavegame(loadingVersion);
				}
				catch (Exception ex)
				{
					Log.Error("Error in PreLoadSavegame of " + conversionChain[i].GetType()?.ToString() + "\n" + ex);
				}
			}
		}
	}

	public static void PostLoadSavegame(string loadingVersion)
	{
		for (int i = 0; i < conversionChain.Count; i++)
		{
			if (conversionChain[i].AppliesToLoadedGameVersion(allowInactiveScribe: true))
			{
				try
				{
					conversionChain[i].PostLoadSavegame(loadingVersion);
				}
				catch (Exception ex)
				{
					Log.Error("Error in PostLoadSavegame of " + conversionChain[i].GetType()?.ToString() + "\n" + ex);
				}
			}
		}
	}

	public static string BackCompatibleDefName(Type defType, string defName, bool forDefInjections = false, XmlNode node = null)
	{
		if (CheckSaveIdenticalToCurrentEnvironment())
		{
			return defName;
		}
		if (GenDefDatabase.GetDefSilentFail(defType, defName, specialCaseForSoundDefs: false) != null)
		{
			return defName;
		}
		string text = defName;
		for (int i = 0; i < conversionChain.Count; i++)
		{
			if (Scribe.mode != LoadSaveMode.Inactive && !conversionChain[i].AppliesToLoadedGameVersion())
			{
				continue;
			}
			try
			{
				string text2 = conversionChain[i].BackCompatibleDefName(defType, text, forDefInjections, node);
				if (text2 != null)
				{
					text = text2;
				}
			}
			catch (Exception ex)
			{
				Log.Error("Error in BackCompatibleDefName of " + conversionChain[i].GetType()?.ToString() + "\n" + ex);
			}
		}
		return text;
	}

	public static object BackCompatibleEnum(Type enumType, string enumName)
	{
		if (enumType == typeof(QualityCategory))
		{
			if (enumName == "Shoddy")
			{
				return QualityCategory.Poor;
			}
			if (enumName == "Superior")
			{
				return QualityCategory.Excellent;
			}
		}
		return null;
	}

	public static Type GetBackCompatibleType(Type baseType, string providedClassName, XmlNode node)
	{
		if (CheckSaveIdenticalToCurrentEnvironment())
		{
			return GenTypes.GetTypeInAnyAssembly(providedClassName);
		}
		for (int i = 0; i < conversionChain.Count; i++)
		{
			if (!conversionChain[i].AppliesToLoadedGameVersion())
			{
				continue;
			}
			try
			{
				Type backCompatibleType = conversionChain[i].GetBackCompatibleType(baseType, providedClassName, node);
				if (backCompatibleType != null)
				{
					return backCompatibleType;
				}
			}
			catch (Exception ex)
			{
				Log.Error("Error in GetBackCompatibleType of " + conversionChain[i].GetType()?.ToString() + "\n" + ex);
			}
		}
		return GenTypes.GetTypeInAnyAssembly(providedClassName);
	}

	public static Type GetBackCompatibleTypeDirect(Type baseType, string providedClassName)
	{
		for (int i = 0; i < conversionChain.Count; i++)
		{
			if (!conversionChain[i].AppliesToVersion(VersionControl.CurrentMajor, VersionControl.CurrentMinor))
			{
				continue;
			}
			try
			{
				Type backCompatibleType = conversionChain[i].GetBackCompatibleType(baseType, providedClassName, null);
				if (backCompatibleType != null)
				{
					return backCompatibleType;
				}
			}
			catch (Exception ex)
			{
				Log.Error("Error in GetBackCompatibleType of " + conversionChain[i].GetType()?.ToString() + "\n" + ex);
			}
		}
		return GenTypes.GetTypeInAnyAssembly(providedClassName);
	}

	public static int GetBackCompatibleBodyPartIndex(BodyDef body, int index)
	{
		if (CheckSaveIdenticalToCurrentEnvironment())
		{
			return index;
		}
		for (int i = 0; i < conversionChain.Count; i++)
		{
			if (conversionChain[i].AppliesToLoadedGameVersion())
			{
				try
				{
					index = conversionChain[i].GetBackCompatibleBodyPartIndex(body, index);
				}
				catch (Exception ex)
				{
					Log.Error("Error in GetBackCompatibleBodyPartIndex of " + body?.ToString() + "\n" + ex);
				}
			}
		}
		return index;
	}

	public static bool WasDefRemoved(string defName, Type type)
	{
		foreach (Tuple<string, Type> removedDef in RemovedDefs)
		{
			if (removedDef.Item1 == defName && removedDef.Item2 == type)
			{
				return true;
			}
		}
		return false;
	}

	public static void PostExposeData(object obj)
	{
		if (Scribe.mode == LoadSaveMode.Saving || CheckSaveIdenticalToCurrentEnvironment())
		{
			return;
		}
		for (int i = 0; i < conversionChain.Count; i++)
		{
			if (conversionChain[i].AppliesToLoadedGameVersion())
			{
				try
				{
					conversionChain[i].PostExposeData(obj);
				}
				catch (Exception ex)
				{
					Log.Error("Error in PostExposeData of " + conversionChain[i].GetType()?.ToString() + "\n" + ex);
				}
			}
		}
	}

	public static void PostCouldntLoadDef(string defName)
	{
		if (CheckSaveIdenticalToCurrentEnvironment())
		{
			return;
		}
		for (int i = 0; i < conversionChain.Count; i++)
		{
			if (conversionChain[i].AppliesToLoadedGameVersion())
			{
				try
				{
					conversionChain[i].PostCouldntLoadDef(defName);
				}
				catch (Exception ex)
				{
					Log.Error("Error in PostCouldntLoadDef of " + conversionChain[i].GetType()?.ToString() + "\n" + ex);
				}
			}
		}
	}

	public static void PawnTrainingTrackerPostLoadInit(Pawn_TrainingTracker tracker, ref DefMap<TrainableDef, bool> wantedTrainables, ref DefMap<TrainableDef, int> steps, ref DefMap<TrainableDef, bool> learned)
	{
		if (wantedTrainables == null)
		{
			wantedTrainables = new DefMap<TrainableDef, bool>();
		}
		if (steps == null)
		{
			steps = new DefMap<TrainableDef, int>();
		}
		if (learned == null)
		{
			learned = new DefMap<TrainableDef, bool>();
		}
		if (tracker.GetSteps(TrainableDefOf.Tameness) == 0 && DefDatabase<TrainableDef>.AllDefsListForReading.Any((TrainableDef td) => tracker.GetSteps(td) != 0))
		{
			tracker.Train(TrainableDefOf.Tameness, null, complete: true);
		}
		foreach (TrainableDef item in DefDatabase<TrainableDef>.AllDefsListForReading)
		{
			if (!tracker.CanAssignToTrain(item).Accepted)
			{
				wantedTrainables[item] = false;
				learned[item] = false;
				steps[item] = 0;
				if (item == TrainableDefOf.Obedience && tracker.pawn.playerSettings != null)
				{
					tracker.pawn.playerSettings.Master = null;
					tracker.pawn.playerSettings.followDrafted = false;
					tracker.pawn.playerSettings.followFieldwork = false;
				}
			}
			if (tracker.GetSteps(item) == item.steps)
			{
				tracker.Train(item, null, complete: true);
			}
		}
	}

	public static void TriggerDataFractionColonyDamageTakenNull(Trigger_FractionColonyDamageTaken trigger, Map map)
	{
		trigger.data = new TriggerData_FractionColonyDamageTaken();
		((TriggerData_FractionColonyDamageTaken)trigger.data).startColonyDamage = map.damageWatcher.DamageTakenEver;
	}

	public static void TriggerDataPawnCycleIndNull(Trigger_KidnapVictimPresent trigger)
	{
		trigger.data = new TriggerData_PawnCycleInd();
	}

	public static void TriggerDataTicksPassedNull(Trigger_TicksPassed trigger)
	{
		trigger.data = new TriggerData_TicksPassed();
	}

	public static TerrainDef BackCompatibleTerrainWithShortHash(ushort hash)
	{
		if (hash == 16442)
		{
			return TerrainDefOf.WaterMovingChestDeep;
		}
		return null;
	}

	public static ThingDef BackCompatibleThingDefWithShortHash(ushort hash)
	{
		if (hash == 62520)
		{
			return ThingDefOf.MineableComponentsIndustrial;
		}
		return null;
	}

	public static ThingDef BackCompatibleThingDefWithShortHash_Force(ushort hash, int major, int minor)
	{
		if (major == 0 && minor <= 18 && hash == 27292)
		{
			return ThingDefOf.MineableSteel;
		}
		return null;
	}

	public static bool CheckSpawnBackCompatibleThingAfterLoading(Thing thing, Map map)
	{
		if (ScribeMetaHeaderUtility.loadedGameVersionMajor == 0 && ScribeMetaHeaderUtility.loadedGameVersionMinor <= 18 && thing.stackCount > thing.def.stackLimit && thing.stackCount != 1 && thing.def.stackLimit != 1)
		{
			tmpThingsToSpawnLater.Add(thing);
			return true;
		}
		return false;
	}

	public static void PreCheckSpawnBackCompatibleThingAfterLoading(Map map)
	{
		tmpThingsToSpawnLater.Clear();
	}

	public static void PostCheckSpawnBackCompatibleThingAfterLoading(Map map)
	{
		for (int i = 0; i < tmpThingsToSpawnLater.Count; i++)
		{
			GenPlace.TryPlaceThing(tmpThingsToSpawnLater[i], tmpThingsToSpawnLater[i].Position, map, ThingPlaceMode.Near);
		}
		tmpThingsToSpawnLater.Clear();
	}

	public static void FactionManagerPostLoadInit()
	{
		if (ModsConfig.RoyaltyActive && Find.FactionManager.FirstFactionOfDef(FactionDefOf.Empire) == null && Find.World.info.factions == null)
		{
			FactionGenerator.CreateFactionAndAddToManager(FactionDefOf.Empire);
		}
		if (ModsConfig.AnomalyActive)
		{
			if (Find.FactionManager.FirstFactionOfDef(FactionDefOf.HoraxCult) == null)
			{
				FactionGenerator.CreateFactionAndAddToManager(FactionDefOf.HoraxCult);
			}
			if (Find.FactionManager.FirstFactionOfDef(FactionDefOf.Entities) == null)
			{
				FactionGenerator.CreateFactionAndAddToManager(FactionDefOf.Entities);
			}
		}
		if (ModsConfig.OdysseyActive)
		{
			if (Find.FactionManager.FirstFactionOfDef(FactionDefOf.TradersGuild) == null)
			{
				FactionGenerator.CreateFactionAndAddToManager(Find.WorldGrid.Orbit, FactionDefOf.TradersGuild);
			}
			if (Find.FactionManager.FirstFactionOfDef(FactionDefOf.Salvagers) == null)
			{
				FactionGenerator.CreateFactionAndAddToManager(Find.WorldGrid.Orbit, FactionDefOf.Salvagers);
			}
		}
	}

	public static void IdeoManagerPostloadInit()
	{
		if (ModsConfig.AnomalyActive && ModsConfig.IdeologyActive && !Find.IdeoManager.classicMode)
		{
			Faction cult = Find.FactionManager.OfHoraxCult;
			if (cult?.ideos != null && cult.def.fixedIdeo && !cult.def.forcedMemes.NullOrEmpty() && !cult.def.forcedMemes.All((MemeDef x) => cult.ideos.PrimaryIdeo.memes.Contains(x)))
			{
				Ideo ideo = IdeoGenerator.MakeFixedIdeo(new IdeoGenerationParms(cult.def, forceNoExpansionIdeo: false, null, null, name: cult.def.ideoName, styles: cult.def.styles, deities: cult.def.deityPresets, hidden: cult.def.hiddenIdeo, description: cult.def.ideoDescription, forcedMemes: cult.def.forcedMemes, classicExtra: false, forceNoWeaponPreference: false, forNewFluidIdeo: false, fixedIdeo: true, requiredPreceptsOnly: cult.def.requiredPreceptsOnly));
				ideo.primaryFactionColor = cult.Color;
				cult.ideos.SetPrimary(ideo);
				Find.IdeoManager.Add(ideo);
			}
		}
	}

	public static void ResearchManagerPostLoadInit()
	{
		List<ResearchProjectDef> allDefsListForReading = DefDatabase<ResearchProjectDef>.AllDefsListForReading;
		for (int i = 0; i < allDefsListForReading.Count; i++)
		{
			if ((allDefsListForReading[i].IsFinished || allDefsListForReading[i].ProgressReal > 0f) && allDefsListForReading[i].TechprintsApplied < allDefsListForReading[i].TechprintCount)
			{
				Find.ResearchManager.AddTechprints(allDefsListForReading[i], allDefsListForReading[i].TechprintCount - allDefsListForReading[i].TechprintsApplied);
			}
		}
	}

	public static void PrefsDataPostLoad(PrefsData prefsData)
	{
		if (prefsData.pauseOnUrgentLetter.HasValue)
		{
			if (prefsData.pauseOnUrgentLetter.Value)
			{
				prefsData.automaticPauseMode = AutomaticPauseMode.MajorThreat;
			}
			else
			{
				prefsData.automaticPauseMode = AutomaticPauseMode.Never;
			}
		}
		if (prefsData.debugActionPalette == null)
		{
			prefsData.debugActionPalette = new List<string>();
		}
	}

	private static bool CheckSaveIdenticalToCurrentEnvironment()
	{
		if (Scribe.mode == LoadSaveMode.Inactive)
		{
			return false;
		}
		if (ScribeMetaHeaderUtility.modListChanged)
		{
			return false;
		}
		if (VersionControl.CurrentBuild != ScribeMetaHeaderUtility.loadedGameVersionBuild)
		{
			return false;
		}
		if (Application.isEditor)
		{
			return false;
		}
		return true;
	}
}
