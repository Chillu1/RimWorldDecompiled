using RimWorld;
using System;
using System.Collections.Generic;
using System.Xml;
using Verse.AI.Group;

namespace Verse
{
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
			new BackCompatibilityConverter_Universal()
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
						Log.Error("Error in PreLoadSavegame of " + conversionChain[i].GetType() + "\n" + ex);
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
						Log.Error("Error in PostLoadSavegame of " + conversionChain[i].GetType() + "\n" + ex);
					}
				}
			}
		}

		public static string BackCompatibleDefName(Type defType, string defName, bool forDefInjections = false, XmlNode node = null)
		{
			if (GenDefDatabase.GetDefSilentFail(defType, defName, specialCaseForSoundDefs: false) != null)
			{
				return defName;
			}
			string text = defName;
			for (int i = 0; i < conversionChain.Count; i++)
			{
				if (Scribe.mode == LoadSaveMode.Inactive || conversionChain[i].AppliesToLoadedGameVersion())
				{
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
						Log.Error("Error in BackCompatibleDefName of " + conversionChain[i].GetType() + "\n" + ex);
					}
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
			for (int i = 0; i < conversionChain.Count; i++)
			{
				if (conversionChain[i].AppliesToLoadedGameVersion())
				{
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
						Log.Error("Error in GetBackCompatibleType of " + conversionChain[i].GetType() + "\n" + ex);
					}
				}
			}
			return GenTypes.GetTypeInAnyAssembly(providedClassName);
		}

		public static int GetBackCompatibleBodyPartIndex(BodyDef body, int index)
		{
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
						Log.Error("Error in GetBackCompatibleBodyPartIndex of " + body + "\n" + ex);
					}
				}
			}
			return index;
		}

		public static void PostExposeData(object obj)
		{
			if (Scribe.mode == LoadSaveMode.Saving)
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
						Log.Error("Error in PostExposeData of " + conversionChain[i].GetType() + "\n" + ex);
					}
				}
			}
		}

		public static void PostCouldntLoadDef(string defName)
		{
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
						Log.Error("Error in PostCouldntLoadDef of " + conversionChain[i].GetType() + "\n" + ex);
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
			if (VersionControl.MajorFromVersionString(ScribeMetaHeaderUtility.loadedGameVersion) == 0 && VersionControl.MinorFromVersionString(ScribeMetaHeaderUtility.loadedGameVersion) <= 18 && thing.stackCount > thing.def.stackLimit && thing.stackCount != 1 && thing.def.stackLimit != 1)
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
			if (ModsConfig.RoyaltyActive && Find.FactionManager.FirstFactionOfDef(FactionDefOf.Empire) == null)
			{
				Faction faction = FactionGenerator.NewGeneratedFaction(FactionDefOf.Empire);
				Find.FactionManager.Add(faction);
			}
		}

		public static void ResearchManagerPostLoadInit()
		{
			List<ResearchProjectDef> allDefsListForReading = DefDatabase<ResearchProjectDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				if ((allDefsListForReading[i].IsFinished || allDefsListForReading[i].ProgressReal > 0f) && allDefsListForReading[i].TechprintsApplied < allDefsListForReading[i].techprintCount)
				{
					Find.ResearchManager.AddTechprints(allDefsListForReading[i], allDefsListForReading[i].techprintCount - allDefsListForReading[i].TechprintsApplied);
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
		}
	}
}
