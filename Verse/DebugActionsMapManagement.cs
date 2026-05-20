using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LudeonTK;
using RimWorld;
using RimWorld.BaseGen;
using RimWorld.Planet;
using RimWorld.SketchGen;
using UnityEngine;

namespace Verse;

public static class DebugActionsMapManagement
{
	private static Map mapLeak;

	[DebugAction("Map", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
	private static List<DebugActionNode> UseScatterer()
	{
		return DebugTools_MapGen.Options_Scatterers();
	}

	[DebugAction("Map", "BaseGen", false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
	private static List<DebugActionNode> BaseGen()
	{
		List<DebugActionNode> list = new List<DebugActionNode>();
		foreach (string item in DefDatabase<RuleDef>.AllDefs.Select((RuleDef x) => x.symbol).Distinct())
		{
			string localSymbol = item;
			list.Add(new DebugActionNode(localSymbol)
			{
				action = delegate
				{
					DebugTool tool = null;
					IntVec3 firstCorner;
					tool = new DebugTool("first corner...", delegate
					{
						firstCorner = UI.MouseCell();
						DebugTools.curTool = new DebugTool("second corner...", delegate
						{
							IntVec3 second = UI.MouseCell();
							CellRect rect = CellRect.FromLimits(firstCorner, second).ClipInsideMap(Find.CurrentMap);
							RimWorld.BaseGen.BaseGen.globalSettings.map = Find.CurrentMap;
							RimWorld.BaseGen.BaseGen.symbolStack.Push(localSymbol, rect);
							RimWorld.BaseGen.BaseGen.Generate();
							DebugTools.curTool = tool;
						}, firstCorner);
					});
					DebugTools.curTool = tool;
				}
			});
		}
		return list;
	}

	[DebugAction("Map", "SketchGen", false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
	private static List<DebugActionNode> SketchGen()
	{
		List<DebugActionNode> list = new List<DebugActionNode>();
		foreach (SketchResolverDef item in DefDatabase<SketchResolverDef>.AllDefs.Where((SketchResolverDef x) => x.isRoot))
		{
			SketchResolverDef localResolver = item;
			DebugActionNode debugActionNode = new DebugActionNode(localResolver.defName);
			if (localResolver == SketchResolverDefOf.Monument || localResolver == SketchResolverDefOf.MonumentRuin)
			{
				new List<DebugMenuOption>();
				for (int num = 1; num <= 60; num++)
				{
					int localIndex = num;
					debugActionNode.AddChild(new DebugActionNode(localIndex.ToString(), DebugActionType.ToolMap)
					{
						action = delegate
						{
							RimWorld.SketchGen.SketchGen.Generate(parms: new SketchResolveParams
							{
								sketch = new Sketch(),
								monumentSize = new IntVec2(localIndex, localIndex)
							}, root: localResolver).Spawn(Find.CurrentMap, UI.MouseCell(), null, Sketch.SpawnPosType.Unchanged, Sketch.SpawnMode.Normal, wipeIfCollides: false, forceTerrainAffordance: false, clearEdificeWhereFloor: false, null, dormant: false, buildRoofsInstantly: true);
						}
					});
				}
			}
			else
			{
				debugActionNode.actionType = DebugActionType.ToolMap;
				debugActionNode.action = delegate
				{
					RimWorld.SketchGen.SketchGen.Generate(parms: new SketchResolveParams
					{
						sketch = new Sketch()
					}, root: localResolver).Spawn(Find.CurrentMap, UI.MouseCell(), null, Sketch.SpawnPosType.Unchanged, Sketch.SpawnMode.Normal, wipeIfCollides: false, forceTerrainAffordance: false, clearEdificeWhereFloor: false, null, dormant: false, buildRoofsInstantly: true);
				};
			}
			list.Add(debugActionNode);
		}
		return list;
	}

	[DebugAction("Map", "Set terrain (rect)", false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 100)]
	private static List<DebugActionNode> SetTerrainRect()
	{
		List<DebugActionNode> list = new List<DebugActionNode>();
		foreach (TerrainDef item in DefDatabase<TerrainDef>.AllDefs.Where((TerrainDef terr) => !terr.temporary))
		{
			TerrainDef localDef = item;
			list.Add(new DebugActionNode(localDef.defName)
			{
				action = delegate
				{
					DebugToolsGeneral.GenericRectTool(localDef.defName, delegate(CellRect rect)
					{
						foreach (IntVec3 item2 in rect)
						{
							Find.CurrentMap.terrainGrid.SetTerrain(item2, localDef);
						}
					});
				}
			});
		}
		return list;
	}

	[DebugAction("Map", "Set temp terrain (rect)", false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 100)]
	private static List<DebugActionNode> SetTempTerrainRect()
	{
		List<DebugActionNode> list = new List<DebugActionNode>();
		foreach (TerrainDef item in DefDatabase<TerrainDef>.AllDefs.Where((TerrainDef terr) => terr.temporary))
		{
			TerrainDef localDef = item;
			list.Add(new DebugActionNode(localDef.defName)
			{
				action = delegate
				{
					DebugToolsGeneral.GenericRectTool(localDef.defName, delegate(CellRect rect)
					{
						foreach (IntVec3 item2 in rect)
						{
							Find.CurrentMap.terrainGrid.SetTempTerrain(item2, localDef);
						}
					});
				}
			});
		}
		return list;
	}

	[DebugAction("Map", "Clear temp terrain (rect)", false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 100)]
	private static void ClearTempTerrainRect()
	{
		DebugToolsGeneral.GenericRectTool("Clear temp terrain", delegate(CellRect rect)
		{
			foreach (IntVec3 item in rect)
			{
				Find.CurrentMap.terrainGrid.RemoveTempTerrain(item);
			}
		});
	}

	[DebugAction("Lighting", "Log lights affecting cell", false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap, actionType = DebugActionType.ToolMap)]
	private static void LogLightsAffectinCell()
	{
		Find.CurrentMap.glowGrid.DevPrintLightIdsAffectingCell(UI.MouseCell());
	}

	[DebugAction("Lighting", "Regen glow grid cells (rect)", false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 100)]
	private static void RegenGlowGridCells()
	{
		DebugToolsGeneral.GenericRectTool("Regen glow grid cells", delegate(CellRect rect)
		{
			Find.CurrentMap.glowGrid.DevDirtyRect(rect);
		});
	}

	[DebugAction("Map", "Pollute (rect)", false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 100, requiresBiotech = true)]
	private static void PolluteRect()
	{
		DebugToolsGeneral.GenericRectTool("Pollute", delegate(CellRect rect)
		{
			foreach (IntVec3 item in rect)
			{
				Find.CurrentMap.pollutionGrid.SetPolluted(item, isPolluted: true);
			}
		});
	}

	[DebugAction("Map", "Unpollute (rect)", false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 100, requiresBiotech = true)]
	private static void UnpolluteRect()
	{
		DebugToolsGeneral.GenericRectTool("Unpollute", delegate(CellRect rect)
		{
			foreach (IntVec3 item in rect)
			{
				Find.CurrentMap.pollutionGrid.SetPolluted(item, isPolluted: false);
			}
		});
	}

	[DebugAction("Map", "Make rock (rect)", false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 100)]
	private static void MakeRock()
	{
		DebugToolsGeneral.GenericRectTool("Make rock", delegate(CellRect rect)
		{
			foreach (IntVec3 item in rect)
			{
				GenSpawn.Spawn(ThingDefOf.Granite, item, Find.CurrentMap);
			}
		});
	}

	[DebugAction("Map", "Grow pollution (x10 cell)", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = -200, requiresBiotech = true)]
	private static void PolluteCellTen()
	{
		PollutionUtility.GrowPollutionAt(UI.MouseCell(), Find.CurrentMap, 10);
	}

	[DebugAction("Map", "Grow pollution (x100 cell)", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = -200, requiresBiotech = true)]
	private static void PolluteCellHundred()
	{
		PollutionUtility.GrowPollutionAt(UI.MouseCell(), Find.CurrentMap, 100);
	}

	[DebugAction("Map", "Grow pollution (x1000 cell)", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = -200, requiresBiotech = true)]
	private static void PolluteCellThousand()
	{
		PollutionUtility.GrowPollutionAt(UI.MouseCell(), Find.CurrentMap, 1000);
	}

	[DebugAction("Map", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.Playing)]
	private static List<DebugActionNode> AddGameCondition()
	{
		List<DebugActionNode> list = new List<DebugActionNode>();
		foreach (GameConditionDef allDef in DefDatabase<GameConditionDef>.AllDefs)
		{
			GameConditionDef localDef = allDef;
			DebugActionNode debugActionNode = new DebugActionNode(localDef.LabelCap);
			debugActionNode.AddChild(new DebugActionNode("Permanent")
			{
				action = delegate
				{
					GameCondition gameCondition = GameConditionMaker.MakeCondition(localDef);
					gameCondition.Permanent = true;
					if (Find.CurrentMap != null)
					{
						Find.CurrentMap.GameConditionManager.RegisterCondition(gameCondition);
					}
					else
					{
						Find.World.GameConditionManager.RegisterCondition(gameCondition);
					}
				}
			});
			for (int num = 2500; num <= 60000; num += 2500)
			{
				int localTicks = num;
				debugActionNode.AddChild(new DebugActionNode(localTicks.ToStringTicksToPeriod() ?? "")
				{
					action = delegate
					{
						GameCondition gameCondition = GameConditionMaker.MakeCondition(localDef);
						gameCondition.Duration = localTicks;
						if (Find.CurrentMap != null)
						{
							Find.CurrentMap.GameConditionManager.RegisterCondition(gameCondition);
						}
						else
						{
							Find.World.GameConditionManager.RegisterCondition(gameCondition);
						}
					}
				});
			}
			list.Add(debugActionNode);
		}
		return list;
	}

	[DebugAction("Map", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.Playing)]
	private static List<DebugActionNode> RemoveGameCondition()
	{
		List<DebugActionNode> list = new List<DebugActionNode>();
		foreach (GameConditionDef allDef in DefDatabase<GameConditionDef>.AllDefs)
		{
			GameConditionDef localDef = allDef;
			list.Add(new DebugActionNode(localDef.LabelCap)
			{
				action = delegate
				{
					GameCondition activeCondition = Find.CurrentMap.gameConditionManager.GetActiveCondition(localDef);
					if (activeCondition != null)
					{
						activeCondition.Duration = 0;
					}
				},
				visibilityGetter = () => Find.CurrentMap != null && Find.CurrentMap.gameConditionManager.ConditionIsActive(localDef)
			});
		}
		return list;
	}

	[DebugAction("Map", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
	private static void RefogMap()
	{
		FloodFillerFog.DebugRefogMap(Find.CurrentMap);
	}

	[DebugAction("Map", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
	private static List<DebugActionNode> UseGenStep()
	{
		List<DebugActionNode> list = new List<DebugActionNode>();
		foreach (Type item in typeof(GenStep).AllSubclassesNonAbstract())
		{
			Type localGenStep = item;
			list.Add(new DebugActionNode(localGenStep.Name)
			{
				action = delegate
				{
					((GenStep)Activator.CreateInstance(localGenStep)).Generate(Find.CurrentMap, default(GenStepParams));
				}
			});
		}
		return list;
	}

	[DebugAction("Map", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
	private static void RegenSection()
	{
		Find.CurrentMap.mapDrawer.SectionAt(UI.MouseCell()).RegenerateAllLayers();
	}

	[DebugAction("Map", null, false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
	private static void RegenAllMapMeshSections()
	{
		Find.CurrentMap.mapDrawer.RegenerateEverythingNow();
	}

	[DebugAction("Map", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
	private static void AddSnow()
	{
		WeatherBuildupUtility.AddSnowRadial(UI.MouseCell(), Find.CurrentMap, 5f, 1f);
	}

	[DebugAction("Map", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
	private static void AddSand()
	{
		WeatherBuildupUtility.AddSandRadial(UI.MouseCell(), Find.CurrentMap, 5f, 1f);
	}

	[DebugAction("Map", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
	private static void RemoveSnow()
	{
		WeatherBuildupUtility.AddSnowRadial(UI.MouseCell(), Find.CurrentMap, 5f, -1f);
	}

	[DebugAction("Map", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
	private static void RemoveSand()
	{
		WeatherBuildupUtility.AddSandRadial(UI.MouseCell(), Find.CurrentMap, 5f, -1f);
	}

	[DebugAction("Map", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
	private static void ClearAllSnow()
	{
		foreach (IntVec3 allCell in Find.CurrentMap.AllCells)
		{
			Find.CurrentMap.snowGrid.SetDepth(allCell, 0f);
		}
	}

	[DebugAction("Map", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap, requiresOdyssey = true)]
	private static void ClearAllSand()
	{
		if (!ModsConfig.OdysseyActive)
		{
			return;
		}
		foreach (IntVec3 allCell in Find.CurrentMap.AllCells)
		{
			Find.CurrentMap.sandGrid.SetDepth(allCell, 0f);
		}
	}

	[DebugAction("Map", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.Playing, hideInSubMenu = true)]
	private static void GenerateMap()
	{
		PlanetTile tile = TileFinder.RandomStartingTile();
		MapParent mapParent = (MapParent)WorldObjectMaker.MakeWorldObject(tile.LayerDef.SettlementWorldObjectDef);
		mapParent.Tile = tile;
		mapParent.SetFaction(Faction.OfPlayer);
		Find.WorldObjects.Add(mapParent);
		GetOrGenerateMapUtility.GetOrGenerateMap(mapParent.Tile, new IntVec3(50, 1, 50), null);
	}

	[DebugAction("Map", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.Playing)]
	private static void DestroyMap()
	{
		List<DebugMenuOption> list = new List<DebugMenuOption>();
		List<Map> maps = Find.Maps;
		for (int i = 0; i < maps.Count; i++)
		{
			Map map = maps[i];
			list.Add(new DebugMenuOption(map.ToString(), DebugMenuOptionMode.Action, delegate
			{
				Current.Game.DeinitAndRemoveMap(map, notifyPlayer: true);
			}));
		}
		Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
	}

	[DebugAction("Map", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.Playing, hideInSubMenu = true)]
	private static void LeakMap()
	{
		List<DebugMenuOption> list = new List<DebugMenuOption>();
		List<Map> maps = Find.Maps;
		for (int i = 0; i < maps.Count; i++)
		{
			Map map = maps[i];
			list.Add(new DebugMenuOption(map.ToString(), DebugMenuOptionMode.Action, delegate
			{
				mapLeak = map;
			}));
		}
		Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
	}

	[DebugAction("Map", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.Playing, hideInSubMenu = true)]
	private static void PrintLeakedMap()
	{
		Log.Message($"Leaked map {mapLeak}");
	}

	[DebugAction("Map", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.Playing, actionType = DebugActionType.ToolMap)]
	private static void Transfer()
	{
		List<Thing> toTransfer = Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList();
		if (!toTransfer.Any())
		{
			return;
		}
		List<DebugMenuOption> list = new List<DebugMenuOption>();
		List<Map> maps = Find.Maps;
		for (int i = 0; i < maps.Count; i++)
		{
			Map map = maps[i];
			if (map == Find.CurrentMap)
			{
				continue;
			}
			list.Add(new DebugMenuOption(map.ToString(), DebugMenuOptionMode.Action, delegate
			{
				for (int j = 0; j < toTransfer.Count; j++)
				{
					if (CellFinder.TryFindRandomCellNear(map.Center, map, Mathf.Max(map.Size.x, map.Size.z), (IntVec3 x) => !x.Fogged(map) && x.Standable(map), out var result))
					{
						toTransfer[j].DeSpawn();
						GenPlace.TryPlaceThing(toTransfer[j], result, map, ThingPlaceMode.Near);
					}
					else
					{
						Log.Error("Could not find spawn cell.");
					}
				}
			}));
		}
		Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
	}

	[DebugAction("Map", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.Playing)]
	private static void ChangeMap()
	{
		List<DebugMenuOption> list = new List<DebugMenuOption>();
		List<Map> maps = Find.Maps;
		for (int i = 0; i < maps.Count; i++)
		{
			Map map = maps[i];
			if (map != Find.CurrentMap)
			{
				list.Add(new DebugMenuOption(map.ToString(), DebugMenuOptionMode.Action, delegate
				{
					Current.Game.CurrentMap = map;
				}));
			}
		}
		Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
	}

	[DebugAction("Map", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.Playing)]
	private static void RegenerateCurrentMap()
	{
		RememberedCameraPos rememberedCameraPos = Find.CurrentMap.rememberedCameraPos;
		PlanetTile tile = Find.CurrentMap.Tile;
		MapParent parent = Find.CurrentMap.Parent;
		IntVec3 size = Find.CurrentMap.Size;
		bool isPocketMap = Find.CurrentMap.IsPocketMap;
		Current.Game.DeinitAndRemoveMap(Find.CurrentMap, notifyPlayer: true);
		if (isPocketMap)
		{
			Map currentMap = PocketMapUtility.GeneratePocketMap(size, parent.MapGeneratorDef, null, Find.AnyPlayerHomeMap);
			Current.Game.CurrentMap = currentMap;
		}
		else
		{
			Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(tile, size, parent.def);
			Current.Game.CurrentMap = orGenerateMap;
		}
		Find.World.renderer.wantedMode = WorldRenderMode.None;
		Find.CameraDriver.SetRootPosAndSize(rememberedCameraPos.rootPos, rememberedCameraPos.rootSize);
	}

	[DebugAction("Map", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.Playing)]
	private static void RegenerateCurrentMapStepped()
	{
		RememberedCameraPos rememberedCameraPos = Find.CurrentMap.rememberedCameraPos;
		PlanetTile tile = Find.CurrentMap.Tile;
		MapParent parent = Find.CurrentMap.Parent;
		IntVec3 size = Find.CurrentMap.Size;
		bool isPocketMap = Find.CurrentMap.IsPocketMap;
		Current.Game.DeinitAndRemoveMap(Find.CurrentMap, notifyPlayer: true);
		if (isPocketMap)
		{
			Log.Error("Pocket maps are not supported for stepped generation (yet)");
		}
		else
		{
			Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(tile, size, parent.def, null, stepDebugger: true);
			Current.Game.CurrentMap = orGenerateMap;
		}
		Find.World.renderer.wantedMode = WorldRenderMode.None;
		Find.CameraDriver.SetRootPosAndSize(rememberedCameraPos.rootPos, rememberedCameraPos.rootSize);
	}

	[DebugAction("Map", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.Playing)]
	private static void DoNextGenStep()
	{
		if (!MapGenerator.DebugDoNextGenStep(Current.Game.CurrentMap))
		{
			Log.Message("No more gen steps to do.");
		}
	}

	[DebugAction("Map", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.Playing)]
	private static void GenerateMapWithCaves()
	{
		PlanetTile tile = TileFinder.RandomSettlementTileFor(Faction.OfPlayer, mustBeAutoChoosable: false, (PlanetTile x) => Find.World.HasCaves(x));
		if (Find.CurrentMap != null)
		{
			Find.CurrentMap.Parent.Destroy();
		}
		MapParent mapParent = (MapParent)WorldObjectMaker.MakeWorldObject(tile.LayerDef.SettlementWorldObjectDef);
		mapParent.Tile = tile;
		mapParent.SetFaction(Faction.OfPlayer);
		Find.WorldObjects.Add(mapParent);
		Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(tile, Find.World.info.initialMapSize, null);
		Current.Game.CurrentMap = orGenerateMap;
		Find.World.renderer.wantedMode = WorldRenderMode.None;
	}

	[DebugAction("Map", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.Playing)]
	private static List<DebugActionNode> RunMapGenerator()
	{
		List<DebugActionNode> list = new List<DebugActionNode>();
		foreach (MapGeneratorDef item in DefDatabase<MapGeneratorDef>.AllDefsListForReading)
		{
			MapGeneratorDef defLocal = item;
			list.Add(new DebugActionNode(defLocal.defName)
			{
				action = delegate
				{
					PlanetTile tile = Find.WorldGrid.Surface.Tiles.Where((Tile tile2) => tile2.PrimaryBiome.canBuildBase).RandomElement().tile;
					MapParent mapParent = (MapParent)WorldObjectMaker.MakeWorldObject(tile.LayerDef.SettlementWorldObjectDef);
					mapParent.Tile = tile;
					mapParent.SetFaction(Faction.OfPlayer);
					Find.WorldObjects.Add(mapParent);
					Map currentMap = MapGenerator.GenerateMap(Find.World.info.initialMapSize, mapParent, defLocal);
					Current.Game.CurrentMap = currentMap;
				}
			});
		}
		return list;
	}

	[DebugAction("Map", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.Playing)]
	private static List<DebugActionNode> GeneratePocketMap()
	{
		List<DebugActionNode> list = new List<DebugActionNode>();
		foreach (MapGeneratorDef item in DefDatabase<MapGeneratorDef>.AllDefsListForReading)
		{
			MapGeneratorDef defLocal = item;
			list.Add(new DebugActionNode(defLocal.defName)
			{
				action = delegate
				{
					Map currentMap = PocketMapUtility.GeneratePocketMap(new IntVec3(100, 1, 100), defLocal, null, Find.CurrentMap);
					Current.Game.CurrentMap = currentMap;
				}
			});
		}
		return list;
	}

	[DebugAction("Map", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.Playing)]
	private static void ForceReformInCurrentMap()
	{
		if (Find.CurrentMap == null)
		{
			return;
		}
		MapParent mapParent = Find.CurrentMap.Parent;
		List<Pawn> list = new List<Pawn>();
		if (Dialog_FormCaravan.AllSendablePawns(mapParent.Map, reform: true).Any((Pawn x) => x.IsColonist))
		{
			Messages.Message("MessageYouHaveToReformCaravanNow".Translate(), new GlobalTargetInfo(mapParent.Tile), MessageTypeDefOf.NeutralEvent);
			Current.Game.CurrentMap = mapParent.Map;
			Dialog_FormCaravan window = new Dialog_FormCaravan(mapParent.Map, reform: true, delegate
			{
				if (mapParent.HasMap)
				{
					mapParent.Destroy();
				}
			}, mapAboutToBeRemoved: true);
			Find.WindowStack.Add(window);
			return;
		}
		list.Clear();
		list.AddRange(mapParent.Map.mapPawns.AllPawns.Where((Pawn x) => x.Faction == Faction.OfPlayer || x.HostFaction == Faction.OfPlayer));
		if (list.Any((Pawn x) => CaravanUtility.IsOwner(x, Faction.OfPlayer)))
		{
			CaravanExitMapUtility.ExitMapAndCreateCaravan(list, Faction.OfPlayer, mapParent.Tile, mapParent.Tile, PlanetTile.Invalid);
		}
		list.Clear();
		mapParent.Destroy();
	}

	[DebugAction("Map", null, false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
	private static void FillMapWithTrees()
	{
		Map currentMap = Find.CurrentMap;
		foreach (IntVec3 allCell in currentMap.AllCells)
		{
			if (allCell.Standable(currentMap))
			{
				GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.Plant_TreeOak), allCell, currentMap);
			}
		}
	}

	[DebugAction("Map", null, false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap, requiresBiotech = true)]
	private static void LogMapPollution()
	{
		Log.Message("Polluted (of all possible pollutable cells): " + Find.CurrentMap.pollutionGrid.TotalPollutionPercent.ToStringPercent());
	}

	[DebugAction("Map", null, false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
	private static List<DebugActionNode> RegenerateMapWithLandmark()
	{
		List<DebugActionNode> list = new List<DebugActionNode>();
		foreach (LandmarkDef item in DefDatabase<LandmarkDef>.AllDefsListForReading)
		{
			LandmarkDef defLocal = item;
			list.Add(new DebugActionNode(defLocal.defName)
			{
				action = delegate
				{
					RememberedCameraPos rememberedCameraPos = Find.CurrentMap.rememberedCameraPos;
					PlanetTile tile = Find.CurrentMap.Tile;
					MapParent parent = Find.CurrentMap.Parent;
					IntVec3 size = Find.CurrentMap.Size;
					Current.Game.DeinitAndRemoveMap(Find.CurrentMap, notifyPlayer: true);
					Find.WorldGrid[tile].mutatorsNullable = new List<TileMutatorDef>();
					Find.World.landmarks.AddLandmark(defLocal, tile, null, forced: true);
					Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(tile, size, parent.def);
					Current.Game.CurrentMap = orGenerateMap;
					Find.World.renderer.wantedMode = WorldRenderMode.None;
					Find.CameraDriver.SetRootPosAndSize(rememberedCameraPos.rootPos, rememberedCameraPos.rootSize);
				}
			});
		}
		return list;
	}

	[DebugAction("Map", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
	private static void GenerateLandmarkScreenshots()
	{
		List<DebugMenuOption> list = new List<DebugMenuOption>();
		foreach (LandmarkDef item in DefDatabase<LandmarkDef>.AllDefsListForReading)
		{
			LandmarkDef landmarkDef = item;
			list.Add(new DebugMenuOption(landmarkDef.defName, DebugMenuOptionMode.Action, delegate
			{
				List<DebugMenuOption> list2 = new List<DebugMenuOption>();
				foreach (int item2 in Enumerable.Range(0, 10))
				{
					float localCount = item2 * 5;
					if (item2 == 0)
					{
						localCount = 1f;
					}
					list2.Add(new DebugMenuOption(localCount + " screenshots", DebugMenuOptionMode.Action, delegate
					{
						List<DebugMenuOption> list3 = new List<DebugMenuOption>();
						int[] mapSizes = Dialog_AdvancedGameConfig.MapSizes;
						foreach (int num in mapSizes)
						{
							int localSize = num;
							list3.Add(new DebugMenuOption($"{localSize} x {localSize}", DebugMenuOptionMode.Action, delegate
							{
								Find.UIRoot.screenshotMode.Active = true;
								Enumerable.Range(0, Find.WorldGrid.TilesCount - 1).ToList().Shuffle();
								WorldObject worldObject = null;
								StringBuilder log = new StringBuilder();
								log.AppendLine("Landmark info:");
								log.AppendLine("Seed: " + Find.World.info.Seed);
								int num2 = 0;
								foreach (SurfaceTile item3 in Find.WorldGrid.Tiles.InRandomOrder())
								{
									PlanetTile tile = item3.tile;
									if (!Find.WorldObjects.AnyWorldObjectAt(tile) && landmarkDef.IsValidTile(tile, tile.Layer))
									{
										if ((float)num2 >= localCount)
										{
											break;
										}
										num2++;
										LongEventHandler.QueueLongEvent(delegate
										{
											Find.World.landmarks.AddLandmark(landmarkDef, tile);
											Current.Game.DeinitAndRemoveMap(Find.CurrentMap, notifyPlayer: false);
											worldObject?.Destroy();
											Find.World.renderer.wantedMode = WorldRenderMode.None;
											worldObject = (MapParent)WorldObjectMaker.MakeWorldObject(tile.LayerDef.SettlementWorldObjectDef);
											worldObject.Tile = tile;
											worldObject.SetFaction(Faction.OfPlayer);
											Find.WorldObjects.Add(worldObject);
											Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(worldObject.Tile, new IntVec3(localSize, 1, localSize), null);
											Current.Game.CurrentMap = orGenerateMap;
											orGenerateMap.fogGrid.ClearAllFog();
										}, null, doAsynchronously: false, null, showExtraUIInfo: true, forceHideUI: true);
										int ssIndex = num2;
										LongEventHandler.QueueLongEvent(delegate
										{
											Find.CameraDriver.config.sizeRange.max = 150f;
											Find.CameraDriver.SetRootPosAndSize(Find.CurrentMap.Center.ToVector3(), 150f);
											string text = $"{landmarkDef.defName}_{Find.CurrentMap.Biome.defName}_{ssIndex}";
											log.AppendLine(text);
											log.AppendLine($"   tile ID: {tile}, mutators: {Find.CurrentMap.TileInfo.Mutators.Select((TileMutatorDef x) => x.defName).ToCommaList()}");
											ScreenshotTaker.TakeNonSteamShot(text);
										}, null, doAsynchronously: false, null, showExtraUIInfo: true, forceHideUI: true);
									}
								}
								LongEventHandler.QueueLongEvent(delegate
								{
									Log.Message(log.ToString());
									Find.UIRoot.screenshotMode.Active = false;
								}, null, doAsynchronously: false, null);
							}));
						}
						Find.WindowStack.Add(new Dialog_DebugOptionListLister(list3));
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
			}));
		}
		Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
	}

	[DebugAction("Map", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
	private static void AddMutatorToCurrentMapTile()
	{
		Tile tile = Find.CurrentMap.TileInfo;
		List<DebugMenuOption> list = new List<DebugMenuOption>();
		foreach (TileMutatorDef mutatorDef in DefDatabase<TileMutatorDef>.AllDefsListForReading)
		{
			list.Add(new DebugMenuOption(mutatorDef.defName, DebugMenuOptionMode.Action, delegate
			{
				tile.AddMutator(mutatorDef);
			}));
		}
		Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
	}

	[DebugAction("Map", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
	private static void RemoveMutatorFromCurrentMapTile()
	{
		Tile tile = Find.CurrentMap.TileInfo;
		List<DebugMenuOption> list = new List<DebugMenuOption>();
		foreach (TileMutatorDef mutatorDef in tile.Mutators)
		{
			list.Add(new DebugMenuOption(mutatorDef.defName, DebugMenuOptionMode.Action, delegate
			{
				tile.RemoveMutator(mutatorDef);
			}));
		}
		Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
	}
}
