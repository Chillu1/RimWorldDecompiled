using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public static class DebugActionsMapManagement
	{
		private static Map mapLeak;

		[DebugAction("Map management", null, allowedGameStates = AllowedGameStates.Playing)]
		private static void GenerateMap()
		{
			MapParent mapParent = (MapParent)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
			mapParent.Tile = TileFinder.RandomStartingTile();
			mapParent.SetFaction(Faction.OfPlayer);
			Find.WorldObjects.Add(mapParent);
			GetOrGenerateMapUtility.GetOrGenerateMap(mapParent.Tile, new IntVec3(50, 1, 50), null);
		}

		[DebugAction("Map management", null, allowedGameStates = AllowedGameStates.Playing)]
		private static void DestroyMap()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				Map map = maps[i];
				list.Add(new DebugMenuOption(map.ToString(), DebugMenuOptionMode.Action, delegate
				{
					Current.Game.DeinitAndRemoveMap(map);
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("Map management", null, allowedGameStates = AllowedGameStates.Playing)]
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

		[DebugAction("Map management", null, allowedGameStates = AllowedGameStates.Playing)]
		private static void PrintLeakedMap()
		{
			Log.Message($"Leaked map {mapLeak}");
		}

		[DebugAction("Map management", null, allowedGameStates = AllowedGameStates.Playing)]
		private static void AddGameCondition()
		{
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(Options_Add_GameCondition()));
		}

		[DebugAction("Map management", null, allowedGameStates = AllowedGameStates.Playing)]
		private static void RemoveGameCondition()
		{
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(Options_Remove_GameCondition()));
		}

		[DebugAction("Map management", null, allowedGameStates = AllowedGameStates.Playing, actionType = DebugActionType.ToolMap)]
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
						if (CellFinder.TryFindRandomCellNear(map.Center, map, Mathf.Max(map.Size.x, map.Size.z), (IntVec3 x) => !x.Fogged(map) && x.Standable(map), out IntVec3 result))
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

		[DebugAction("Map management", null, allowedGameStates = AllowedGameStates.Playing)]
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

		[DebugAction("Map management", null, allowedGameStates = AllowedGameStates.Playing)]
		private static void RegenerateCurrentMap()
		{
			RememberedCameraPos rememberedCameraPos = Find.CurrentMap.rememberedCameraPos;
			int tile = Find.CurrentMap.Tile;
			MapParent parent = Find.CurrentMap.Parent;
			IntVec3 size = Find.CurrentMap.Size;
			Current.Game.DeinitAndRemoveMap(Find.CurrentMap);
			Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(tile, size, parent.def);
			Current.Game.CurrentMap = orGenerateMap;
			Find.World.renderer.wantedMode = WorldRenderMode.None;
			Find.CameraDriver.SetRootPosAndSize(rememberedCameraPos.rootPos, rememberedCameraPos.rootSize);
		}

		[DebugAction("Map management", null, allowedGameStates = AllowedGameStates.Playing)]
		private static void GenerateMapWithCaves()
		{
			int tile = TileFinder.RandomSettlementTileFor(Faction.OfPlayer, mustBeAutoChoosable: false, (int x) => Find.World.HasCaves(x));
			if (Find.CurrentMap != null)
			{
				Find.CurrentMap.Parent.Destroy();
			}
			MapParent mapParent = (MapParent)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
			mapParent.Tile = tile;
			mapParent.SetFaction(Faction.OfPlayer);
			Find.WorldObjects.Add(mapParent);
			Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(tile, Find.World.info.initialMapSize, null);
			Current.Game.CurrentMap = orGenerateMap;
			Find.World.renderer.wantedMode = WorldRenderMode.None;
		}

		[DebugAction("Map management", null, allowedGameStates = AllowedGameStates.Playing)]
		private static void RunMapGenerator()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (MapGeneratorDef mapgen in DefDatabase<MapGeneratorDef>.AllDefsListForReading)
			{
				list.Add(new DebugMenuOption(mapgen.defName, DebugMenuOptionMode.Action, delegate
				{
					MapParent mapParent = (MapParent)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
					mapParent.Tile = (from tile in Enumerable.Range(0, Find.WorldGrid.TilesCount)
						where Find.WorldGrid[tile].biome.canBuildBase
						select tile).RandomElement();
					mapParent.SetFaction(Faction.OfPlayer);
					Find.WorldObjects.Add(mapParent);
					Map currentMap = MapGenerator.GenerateMap(Find.World.info.initialMapSize, mapParent, mapgen);
					Current.Game.CurrentMap = currentMap;
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("Map management", null, allowedGameStates = AllowedGameStates.Playing)]
		private static void ForceReformInCurrentMap()
		{
			if (Find.CurrentMap != null)
			{
				TimedForcedExit.ForceReform(Find.CurrentMap.Parent);
			}
		}

		public static List<DebugMenuOption> Options_Add_GameCondition()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (GameConditionDef allDef in DefDatabase<GameConditionDef>.AllDefs)
			{
				GameConditionDef localDef = allDef;
				list.Add(new DebugMenuOption(localDef.LabelCap, DebugMenuOptionMode.Tool, delegate
				{
					Find.CurrentMap.GameConditionManager.RegisterCondition(GameConditionMaker.MakeCondition(localDef));
				}));
			}
			return list;
		}

		public static List<DebugMenuOption> Options_Remove_GameCondition()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (GameCondition activeCondition in Find.CurrentMap.gameConditionManager.ActiveConditions)
			{
				GameCondition localCondition = activeCondition;
				list.Add(new DebugMenuOption(localCondition.def.LabelCap, DebugMenuOptionMode.Tool, delegate
				{
					localCondition.Duration = 0;
				}));
			}
			return list;
		}
	}
}
