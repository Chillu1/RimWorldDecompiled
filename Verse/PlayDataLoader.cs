using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LudeonTK;
using RimWorld;
using RimWorld.BaseGen;
using RimWorld.IO;
using RimWorld.Planet;
using RimWorld.QuestGen;
using UnityEngine;
using Verse.AI;

namespace Verse;

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
		catch (Exception ex)
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
			Log.Warning("Caught exception while loading play data but there are active mods other than Core. Resetting mods config and trying again.\nThe exception was: " + ex);
			try
			{
				ClearAllPlayData();
			}
			catch
			{
				Log.Warning("Caught exception while recovering from errors and trying to clear all play data. Ignoring it.\nThe exception was: " + ex);
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
		GlobalTextureAtlasManager.ClearStaticAtlasBuildQueue();
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
			Parallel.ForEach(typeof(Def).AllSubclasses(), delegate(Type defType)
			{
				GenGeneric.InvokeStaticMethodOnGenericType(typeof(DefDatabase<>), defType, "AddAllInMods");
			});
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
		DeepProfiler.Start("Rebind DefOfs (early).");
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
		DeepProfiler.Start("Legacy backstory translations.");
		try
		{
			BackstoryTranslationUtility.LoadAndInjectBackstoryData(LanguageDatabase.activeLanguage.AllDirectories);
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
		DeepProfiler.Start("Global operations (early pass).");
		try
		{
			ColoredText.ResetStaticData();
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
			ResetStaticDataPre();
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
				DeepProfiler.enabled = false;
				DefDatabase<ThingCategoryDef>.ResolveAllReferences(onlyExactlyMyType: true, parallel: true);
				DeepProfiler.enabled = true;
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
				foreach (Type item in typeof(Def).AllSubclasses())
				{
					if (!(item == typeof(ThingDef)) && !(item == typeof(ThingCategoryDef)) && !(item == typeof(RecipeDef)))
					{
						GenGeneric.InvokeStaticMethodOnGenericType(typeof(DefDatabase<>), item, "ResolveAllReferences", true, false);
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
			ResetStaticDataPost();
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
				Parallel.ForEach(typeof(Def).AllSubclasses(), delegate(Type defType)
				{
					GenGeneric.InvokeStaticMethodOnGenericType(typeof(DefDatabase<>), defType, "ErrorCheckAllDefs");
				});
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
			DeepProfiler.Start("Load all bios");
			try
			{
				SolidBioDatabase.LoadAllBios();
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
			FloatMenuMakerMap.Init();
			DeepProfiler.Start("Atlas baking.");
			try
			{
				GlobalTextureAtlasManager.BakeStaticAtlases();
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
				Resources.UnloadUnusedAssets();
			}
			finally
			{
				DeepProfiler.End();
			}
		});
		LongEventHandler.ExecuteWhenFinished(Log.ResetMessageCount);
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
		SolidBioDatabase.Clear();
		Current.Game = null;
		loadedInt = false;
	}

	public static void HotReloadDefs()
	{
		Dictionary<BodyPartRecord, int> prevBodyPartIndices = DefDatabase<BodyDef>.AllDefsListForReading.SelectMany((BodyDef x) => x.AllParts).ToDictionary((BodyPartRecord x) => x, (BodyPartRecord x) => x.Index);
		LongEventHandler.QueueLongEvent(delegate
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
				LoadedModManager.LoadAllActiveMods(hotReload: true);
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
			DeepProfiler.Start("Global operations (early pass).");
			try
			{
				ColoredText.ResetStaticData();
			}
			finally
			{
				DeepProfiler.End();
			}
			LongEventHandler.ForceExecuteToExecuteWhenFinished();
			DeepProfiler.Start("Generate implied Defs (pre-resolve).");
			try
			{
				DefGenerator.GenerateImpliedDefs_PreResolve(hotReload: true);
			}
			finally
			{
				DeepProfiler.End();
			}
			DeepProfiler.Start("Resolve cross-references.");
			try
			{
				DirectXmlCrossRefLoader.ResolveAllWantedCrossReferences(FailMode.LogErrors);
			}
			finally
			{
				DirectXmlCrossRefLoader.Clear();
				DeepProfiler.End();
			}
			DeepProfiler.Start("Other def binding, resetting and global operations (pre-resolve).");
			try
			{
				ResetStaticDataPre();
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
					DeepProfiler.enabled = false;
					DefDatabase<ThingCategoryDef>.ResolveAllReferences(onlyExactlyMyType: true, parallel: true);
					DeepProfiler.enabled = true;
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
					foreach (Type item in typeof(Def).AllSubclasses())
					{
						if (!(item == typeof(ThingDef)) && !(item == typeof(ThingCategoryDef)) && !(item == typeof(RecipeDef)))
						{
							GenGeneric.InvokeStaticMethodOnGenericType(typeof(DefDatabase<>), item, "ResolveAllReferences", true, false);
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
				DefGenerator.GenerateImpliedDefs_PostResolve(hotReload: true);
			}
			finally
			{
				DeepProfiler.End();
			}
			DeepProfiler.Start("Other def binding, resetting and global operations (post-resolve).");
			try
			{
				ResetStaticDataPost();
			}
			finally
			{
				DeepProfiler.End();
			}
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				DeepProfiler.Start("Match new CompProperties");
				foreach (Map map in Find.Maps)
				{
					List<CompProperties> list = new List<CompProperties>();
					foreach (Thing item2 in (IEnumerable<Thing>)map.spawnedThings)
					{
						if (item2 is ThingWithComps { AllComps: not null } thingWithComps)
						{
							list.Clear();
							list.AddRange(thingWithComps.def.comps);
							List<ThingComp> allComps = thingWithComps.AllComps;
							for (int i = 0; i < allComps.Count; i++)
							{
								CompProperties compProperties = null;
								for (int j = 0; j < list.Count; j++)
								{
									if (list[j].compClass == allComps[i].GetType())
									{
										compProperties = list[j];
										list.RemoveAt(j);
										break;
									}
								}
								if (compProperties != null)
								{
									allComps[i].props = compProperties;
								}
							}
							for (int k = 0; k < list.Count; k++)
							{
								try
								{
									ThingComp thingComp = (ThingComp)Activator.CreateInstance(list[k].compClass);
									thingComp.parent = thingWithComps;
									allComps.Add(thingComp);
									thingComp.Initialize(list[k]);
								}
								catch (Exception ex)
								{
									Log.Error("Could not instantiate or initialize a ThingComp: " + ex);
								}
							}
						}
					}
				}
				DeepProfiler.End();
				DeepProfiler.Start("Match new body parts");
				foreach (Pawn item3 in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead)
				{
					foreach (Hediff hediff in item3.health.hediffSet.hediffs)
					{
						if (hediff.Part != null)
						{
							hediff.Part = item3.RaceProps.body.AllParts[prevBodyPartIndices[hediff.Part]];
						}
					}
				}
				DeepProfiler.End();
				foreach (Map map2 in Find.Maps)
				{
					DeepProfiler.Start("Notify things");
					foreach (Thing item4 in (IEnumerable<Thing>)map2.spawnedThings)
					{
						item4.Notify_DefsHotReloaded();
					}
					DeepProfiler.End();
					DeepProfiler.Start("Rebuild MapDrawer");
					map2.mapDrawer.RegenerateEverythingNow();
					DeepProfiler.End();
				}
				foreach (Window item5 in Find.WindowStack.Windows.ToList())
				{
					item5.Close(doCloseSound: false);
				}
			});
			LongEventHandler.ExecuteWhenFinished(Log.ResetMessageCount);
		}, "LoadingDefs", doAsynchronously: false, null);
	}

	private static void ResetStaticDataPre()
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
		DateReadout.Reset();
		ResearchProjectDef.GenerateNonOverlappingCoordinates();
		BaseGen.Reset();
		ResourceCounter.ResetDefs();
		ApparelProperties.ResetStaticData();
		WildPlantSpawner.ResetStaticData();
		PawnGenerator.Reset();
		GroundSpawner.ResetStaticData();
		Hive.ResetStaticData();
		ExpectationsUtility.Reset();
		SkillUI.Reset();
		QuestNode_GetThingPlayerCanProduce.ResetStaticData();
		Pawn_PsychicEntropyTracker.ResetStaticData();
		QuestNode_GetRandomNegativeGameCondition.ResetStaticData();
		RoyalTitleUtility.ResetStaticData();
		RewardsGenerator.ResetStaticData();
		ThoughtWorker_Precept_HasAutomatedTurrets.ResetStaticData();
		AnimalPenUtility.ResetStaticData();
		StorageSettings.ResetStaticData();
		Listing_TreeThingFilter.ResetStaticData();
		PawnSkinColors.ResetStaticData();
		PawnHairColors.ResetStaticData();
		GenStuff.ResetStaticData();
		StatDef.ResetStaticData();
		MouseoverUtility.Reset();
		Dialog_Debug.ResetStaticData();
		JobGiver_GetHemogen.ResetStaticData();
		CachedTexture.ResetStaticData();
		CachedMaterial.ResetStaticData();
		Storyteller.ResetStaticData();
		PawnRenderUtility.ResetStaticData();
		QuickSearchUtility.ResetStaticData();
		ExpandableLandmarksUtility.ResetStaticData();
		WorkGiver_FillFermentingBarrel.ResetStaticData();
		WorkGiver_InteractAnimal.ResetStaticData();
		WorkGiver_Warden_DoExecution.ResetStaticData();
		WorkGiver_GrowerSow.ResetStaticData();
		MineAIUtility.ResetStaticData();
		WorkGiver_FixBrokenDownBuilding.ResetStaticData();
		WorkGiver_ConstructDeliverResources.ResetStaticData();
	}

	private static void ResetStaticDataPost()
	{
		PawnWeaponGenerator.Reset();
		BuildingProperties.FinalizeInit();
		ThingSetMakerUtility.Reset();
		StatDef.SetImmutability();
		WealthWatcher.ResetStaticData();
		GenLabel.ClearCache();
	}
}
