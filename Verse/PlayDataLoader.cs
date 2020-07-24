using RimWorld;
using RimWorld.BaseGen;
using RimWorld.IO;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse.AI;

namespace Verse
{
	public static class PlayDataLoader
	{
		private static bool loadedInt;

		public static bool Loaded => loadedInt;

		public static void LoadAllPlayData(bool recovering = false)
		{
			if (loadedInt)
			{
				Log.Error("Loading play data when already loaded. Call ClearAllPlayData first.");
				return;
			}
			DeepProfiler.Start("LoadAllPlayData");
			try
			{
				DoPlayLoad();
			}
			catch (Exception arg)
			{
				if (!Prefs.ResetModsConfigOnCrash)
				{
					throw;
				}
				if (recovering)
				{
					Log.Warning("Could not recover from errors loading play data. Giving up.");
					throw;
				}
				IEnumerable<ModMetaData> activeModsInLoadOrder = ModsConfig.ActiveModsInLoadOrder;
				if (!activeModsInLoadOrder.Any((ModMetaData x) => !x.Official) && activeModsInLoadOrder.Any((ModMetaData x) => x.IsCoreMod))
				{
					throw;
				}
				Log.Warning("Caught exception while loading play data but there are active mods other than Core. Resetting mods config and trying again.\nThe exception was: " + arg);
				try
				{
					ClearAllPlayData();
				}
				catch
				{
					Log.Warning("Caught exception while recovering from errors and trying to clear all play data. Ignoring it.\nThe exception was: " + arg);
				}
				ModsConfig.Reset();
				DirectXmlCrossRefLoader.Clear();
				LoadAllPlayData(recovering: true);
				return;
			}
			finally
			{
				DeepProfiler.End();
			}
			loadedInt = true;
			if (recovering)
			{
				Log.Message("Successfully recovered from errors and loaded play data.");
				DelayedErrorWindowRequest.Add("RecoveredFromErrorsText".Translate(), "RecoveredFromErrorsDialogTitle".Translate());
			}
		}

		private static void DoPlayLoad()
		{
			DeepProfiler.Start("GraphicDatabase.Clear()");
			try
			{
				GraphicDatabase.Clear();
			}
			finally
			{
				DeepProfiler.End();
			}
			DeepProfiler.Start("Load all active mods.");
			try
			{
				LoadedModManager.LoadAllActiveMods();
			}
			finally
			{
				DeepProfiler.End();
			}
			DeepProfiler.Start("Load language metadata.");
			try
			{
				LanguageDatabase.InitAllMetadata();
			}
			finally
			{
				DeepProfiler.End();
			}
			LongEventHandler.SetCurrentEventText("LoadingDefs".Translate());
			DeepProfiler.Start("Copy all Defs from mods to global databases.");
			try
			{
				foreach (Type item in typeof(Def).AllSubclasses())
				{
					GenGeneric.InvokeStaticMethodOnGenericType(typeof(DefDatabase<>), item, "AddAllInMods");
				}
			}
			finally
			{
				DeepProfiler.End();
			}
			DeepProfiler.Start("Resolve cross-references between non-implied Defs.");
			try
			{
				DirectXmlCrossRefLoader.ResolveAllWantedCrossReferences(FailMode.Silent);
			}
			finally
			{
				DeepProfiler.End();
			}
			DeepProfiler.Start("Rebind defs (early).");
			try
			{
				DefOfHelper.RebindAllDefOfs(earlyTryMode: true);
			}
			finally
			{
				DeepProfiler.End();
			}
			DeepProfiler.Start("TKeySystem.BuildMappings()");
			try
			{
				TKeySystem.BuildMappings();
			}
			finally
			{
				DeepProfiler.End();
			}
			DeepProfiler.Start("Inject selected language data into game data (early pass).");
			try
			{
				LanguageDatabase.activeLanguage.InjectIntoData_BeforeImpliedDefs();
			}
			finally
			{
				DeepProfiler.End();
			}
			DeepProfiler.Start("Generate implied Defs (pre-resolve).");
			try
			{
				DefGenerator.GenerateImpliedDefs_PreResolve();
			}
			finally
			{
				DeepProfiler.End();
			}
			DeepProfiler.Start("Resolve cross-references between Defs made by the implied defs.");
			try
			{
				DirectXmlCrossRefLoader.ResolveAllWantedCrossReferences(FailMode.LogErrors);
			}
			finally
			{
				DirectXmlCrossRefLoader.Clear();
				DeepProfiler.End();
			}
			DeepProfiler.Start("Rebind DefOfs (final).");
			try
			{
				DefOfHelper.RebindAllDefOfs(earlyTryMode: false);
			}
			finally
			{
				DeepProfiler.End();
			}
			DeepProfiler.Start("Other def binding, resetting and global operations (pre-resolve).");
			try
			{
				PlayerKnowledgeDatabase.ReloadAndRebind();
				LessonAutoActivator.Reset();
				CostListCalculator.Reset();
				Pawn.ResetStaticData();
				PawnApparelGenerator.Reset();
				RestUtility.Reset();
				ThoughtUtility.Reset();
				ThinkTreeKeyAssigner.Reset();
				ThingCategoryNodeDatabase.FinalizeInit();
				TrainableUtility.Reset();
				HaulAIUtility.Reset();
				GenConstruct.Reset();
				MedicalCareUtility.Reset();
				InspectPaneUtility.Reset();
				GraphicDatabaseHeadRecords.Reset();
				DateReadout.Reset();
				ResearchProjectDef.GenerateNonOverlappingCoordinates();
				BaseGen.Reset();
				ResourceCounter.ResetDefs();
				ApparelProperties.ResetStaticData();
				WildPlantSpawner.ResetStaticData();
				PawnGenerator.Reset();
				TunnelHiveSpawner.ResetStaticData();
				Hive.ResetStaticData();
				ExpectationsUtility.Reset();
				WealthWatcher.ResetStaticData();
				SkillUI.Reset();
				QuestNode_GetThingPlayerCanProduce.ResetStaticData();
				Pawn_PsychicEntropyTracker.ResetStaticData();
				ColoredText.ResetStaticData();
				QuestNode_GetRandomNegativeGameCondition.ResetStaticData();
				RoyalTitleUtility.ResetStaticData();
				RewardsGenerator.ResetStaticData();
				WorkGiver_FillFermentingBarrel.ResetStaticData();
				WorkGiver_DoBill.ResetStaticData();
				WorkGiver_InteractAnimal.ResetStaticData();
				WorkGiver_Warden_DoExecution.ResetStaticData();
				WorkGiver_GrowerSow.ResetStaticData();
				WorkGiver_Miner.ResetStaticData();
				WorkGiver_FixBrokenDownBuilding.ResetStaticData();
				WorkGiver_ConstructDeliverResources.ResetStaticData();
			}
			finally
			{
				DeepProfiler.End();
			}
			DeepProfiler.Start("Resolve references.");
			try
			{
				DeepProfiler.Start("ThingCategoryDef resolver");
				try
				{
					DefDatabase<ThingCategoryDef>.ResolveAllReferences();
				}
				finally
				{
					DeepProfiler.End();
				}
				DeepProfiler.Start("RecipeDef resolver");
				try
				{
					DeepProfiler.enabled = false;
					DefDatabase<RecipeDef>.ResolveAllReferences(onlyExactlyMyType: true, parallel: true);
					DeepProfiler.enabled = true;
				}
				finally
				{
					DeepProfiler.End();
				}
				DeepProfiler.Start("Static resolver calls");
				try
				{
					foreach (Type item2 in typeof(Def).AllSubclasses())
					{
						if (!(item2 == typeof(ThingDef)) && !(item2 == typeof(ThingCategoryDef)) && !(item2 == typeof(RecipeDef)))
						{
							GenGeneric.InvokeStaticMethodOnGenericType(typeof(DefDatabase<>), item2, "ResolveAllReferences", true, false);
						}
					}
				}
				finally
				{
					DeepProfiler.End();
				}
				DeepProfiler.Start("ThingDef resolver");
				try
				{
					DefDatabase<ThingDef>.ResolveAllReferences();
				}
				finally
				{
					DeepProfiler.End();
				}
			}
			finally
			{
				DeepProfiler.End();
			}
			DeepProfiler.Start("Generate implied Defs (post-resolve).");
			try
			{
				DefGenerator.GenerateImpliedDefs_PostResolve();
			}
			finally
			{
				DeepProfiler.End();
			}
			DeepProfiler.Start("Other def binding, resetting and global operations (post-resolve).");
			try
			{
				PawnWeaponGenerator.Reset();
				BuildingProperties.FinalizeInit();
				ThingSetMakerUtility.Reset();
			}
			finally
			{
				DeepProfiler.End();
			}
			if (Prefs.DevMode)
			{
				DeepProfiler.Start("Error check all defs.");
				try
				{
					foreach (Type item3 in typeof(Def).AllSubclasses())
					{
						GenGeneric.InvokeStaticMethodOnGenericType(typeof(DefDatabase<>), item3, "ErrorCheckAllDefs");
					}
				}
				finally
				{
					DeepProfiler.End();
				}
			}
			LongEventHandler.SetCurrentEventText("Initializing".Translate());
			DeepProfiler.Start("Load keyboard preferences.");
			try
			{
				KeyPrefs.Init();
			}
			finally
			{
				DeepProfiler.End();
			}
			DeepProfiler.Start("Short hash giving.");
			try
			{
				ShortHashGiver.GiveAllShortHashes();
			}
			finally
			{
				DeepProfiler.End();
			}
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				DeepProfiler.Start("Load backstories.");
				try
				{
					BackstoryDatabase.ReloadAllBackstories();
				}
				finally
				{
					DeepProfiler.End();
				}
			});
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				DeepProfiler.Start("Inject selected language data into game data.");
				try
				{
					LanguageDatabase.activeLanguage.InjectIntoData_AfterImpliedDefs();
					GenLabel.ClearCache();
				}
				finally
				{
					DeepProfiler.End();
				}
			});
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				DeepProfiler.Start("Static constructor calls");
				try
				{
					StaticConstructorOnStartupUtility.CallAll();
					if (Prefs.DevMode)
					{
						StaticConstructorOnStartupUtility.ReportProbablyMissingAttributes();
					}
				}
				finally
				{
					DeepProfiler.End();
				}
				DeepProfiler.Start("Garbage Collection");
				try
				{
					AbstractFilesystem.ClearAllCache();
					GC.Collect(int.MaxValue, GCCollectionMode.Forced);
				}
				finally
				{
					DeepProfiler.End();
				}
			});
		}

		public static void ClearAllPlayData()
		{
			LanguageDatabase.Clear();
			LoadedModManager.ClearDestroy();
			foreach (Type item in typeof(Def).AllSubclasses())
			{
				GenGeneric.InvokeStaticMethodOnGenericType(typeof(DefDatabase<>), item, "Clear");
			}
			ThingCategoryNodeDatabase.Clear();
			BackstoryDatabase.Clear();
			SolidBioDatabase.Clear();
			Current.Game = null;
			loadedInt = false;
		}
	}
}
