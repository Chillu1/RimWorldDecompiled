using RimWorld;
using RimWorld.BaseGen;
using RimWorld.SketchGen;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse.AI;

namespace Verse
{
	public static class DebugToolsGeneral
	{
		[DebugAction("General", null, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void Destroy()
		{
			foreach (Thing item in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList())
			{
				item.Destroy();
			}
		}

		[DebugAction("General", null, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void Kill()
		{
			foreach (Thing item in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList())
			{
				item.Kill();
			}
		}

		[DebugAction("General", "10 damage", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void Take10Damage()
		{
			foreach (Thing item in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList())
			{
				item.TakeDamage(new DamageInfo(DamageDefOf.Crush, 10f));
			}
		}

		[DebugAction("General", "5000 damage", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void Take5000Damage()
		{
			foreach (Thing item in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList())
			{
				item.TakeDamage(new DamageInfo(DamageDefOf.Crush, 50000f));
			}
		}

		[DebugAction("General", "5000 flame damage", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void Take5000FlameDamage()
		{
			foreach (Thing item in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList())
			{
				item.TakeDamage(new DamageInfo(DamageDefOf.Flame, 5000f));
			}
		}

		[DebugAction("General", "Clear area 21x21", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void ClearArea21x21()
		{
			foreach (Thing item in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList())
			{
				_ = item;
				GenDebug.ClearArea(CellRect.CenteredOn(UI.MouseCell(), 10), Find.CurrentMap);
			}
		}

		[DebugAction("General", "Rock 21x21", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void Rock21x21()
		{
			CellRect cellRect = CellRect.CenteredOn(UI.MouseCell(), 10);
			cellRect.ClipInsideMap(Find.CurrentMap);
			foreach (IntVec3 item in cellRect)
			{
				GenSpawn.Spawn(ThingDefOf.Granite, item, Find.CurrentMap);
			}
		}

		[DebugAction("General", "Destroy trees 21x21", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void DestroyTrees21x21()
		{
			CellRect cellRect = CellRect.CenteredOn(UI.MouseCell(), 10);
			cellRect.ClipInsideMap(Find.CurrentMap);
			foreach (IntVec3 item in cellRect)
			{
				List<Thing> thingList = item.GetThingList(Find.CurrentMap);
				for (int num = thingList.Count - 1; num >= 0; num--)
				{
					if (thingList[num].def.category == ThingCategory.Plant && thingList[num].def.plant.IsTree)
					{
						thingList[num].Destroy();
					}
				}
			}
		}

		[DebugAction("General", "Explosion (bomb)", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void ExplosionBomb()
		{
			GenExplosion.DoExplosion(UI.MouseCell(), Find.CurrentMap, 3.9f, DamageDefOf.Bomb, null);
		}

		[DebugAction("General", "Explosion (flame)", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void ExplosionFlame()
		{
			GenExplosion.DoExplosion(UI.MouseCell(), Find.CurrentMap, 3.9f, DamageDefOf.Flame, null);
		}

		[DebugAction("General", "Explosion (stun)", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void ExplosionStun()
		{
			GenExplosion.DoExplosion(UI.MouseCell(), Find.CurrentMap, 3.9f, DamageDefOf.Stun, null);
		}

		[DebugAction("General", "Explosion (EMP)", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void ExplosionEMP()
		{
			GenExplosion.DoExplosion(UI.MouseCell(), Find.CurrentMap, 3.9f, DamageDefOf.EMP, null);
		}

		[DebugAction("General", "Explosion (extinguisher)", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void ExplosionExtinguisher()
		{
			GenExplosion.DoExplosion(UI.MouseCell(), Find.CurrentMap, 10f, DamageDefOf.Extinguish, null, -1, -1f, null, null, null, null, ThingDefOf.Filth_FireFoam, 1f, 3, applyDamageToExplosionCellsNeighbors: true);
		}

		[DebugAction("General", "Explosion (smoke)", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void ExplosionSmoke()
		{
			GenExplosion.DoExplosion(UI.MouseCell(), Find.CurrentMap, 10f, DamageDefOf.Smoke, null, -1, -1f, null, null, null, null, ThingDefOf.Gas_Smoke, 1f);
		}

		[DebugAction("General", null, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void LightningStrike()
		{
			Find.CurrentMap.weatherManager.eventHandler.AddEvent(new WeatherEvent_LightningStrike(Find.CurrentMap, UI.MouseCell()));
		}

		[DebugAction("General", null, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void AddSnow()
		{
			SnowUtility.AddSnowRadial(UI.MouseCell(), Find.CurrentMap, 5f, 1f);
		}

		[DebugAction("General", null, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void RemoveSnow()
		{
			SnowUtility.AddSnowRadial(UI.MouseCell(), Find.CurrentMap, 5f, -1f);
		}

		[DebugAction("General", null, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void ClearAllSnow()
		{
			foreach (IntVec3 allCell in Find.CurrentMap.AllCells)
			{
				Find.CurrentMap.snowGrid.SetDepth(allCell, 0f);
			}
		}

		[DebugAction("General", "Push heat (10)", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void PushHeat10()
		{
			foreach (IntVec3 allCell in Find.CurrentMap.AllCells)
			{
				_ = allCell;
				GenTemperature.PushHeat(UI.MouseCell(), Find.CurrentMap, 10f);
			}
		}

		[DebugAction("General", "Push heat (1000)", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void PushHeat1000()
		{
			foreach (IntVec3 allCell in Find.CurrentMap.AllCells)
			{
				_ = allCell;
				GenTemperature.PushHeat(UI.MouseCell(), Find.CurrentMap, 1000f);
			}
		}

		[DebugAction("General", "Push heat (-1000)", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void PushHeatNeg1000()
		{
			foreach (IntVec3 allCell in Find.CurrentMap.AllCells)
			{
				_ = allCell;
				GenTemperature.PushHeat(UI.MouseCell(), Find.CurrentMap, -1000f);
			}
		}

		[DebugAction("General", null, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void FinishPlantGrowth()
		{
			foreach (Thing item in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()))
			{
				Plant plant = item as Plant;
				if (plant != null)
				{
					plant.Growth = 1f;
				}
			}
		}

		[DebugAction("General", "Grow 1 day", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void Grow1Day()
		{
			IntVec3 intVec = UI.MouseCell();
			Plant plant = intVec.GetPlant(Find.CurrentMap);
			if (plant != null && plant.def.plant != null)
			{
				int num = (int)((1f - plant.Growth) * plant.def.plant.growDays);
				if (num >= 60000)
				{
					plant.Age += 60000;
				}
				else if (num > 0)
				{
					plant.Age += num;
				}
				plant.Growth += 1f / plant.def.plant.growDays;
				if ((double)plant.Growth > 1.0)
				{
					plant.Growth = 1f;
				}
				Find.CurrentMap.mapDrawer.SectionAt(intVec).RegenerateAllLayers();
			}
		}

		[DebugAction("General", null, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void GrowToMaturity()
		{
			IntVec3 intVec = UI.MouseCell();
			Plant plant = intVec.GetPlant(Find.CurrentMap);
			if (plant != null && plant.def.plant != null)
			{
				int num = (int)((1f - plant.Growth) * plant.def.plant.growDays);
				plant.Age += num;
				plant.Growth = 1f;
				Find.CurrentMap.mapDrawer.SectionAt(intVec).RegenerateAllLayers();
			}
		}

		[DebugAction("General", null, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void RegenSection()
		{
			Find.CurrentMap.mapDrawer.SectionAt(UI.MouseCell()).RegenerateAllLayers();
		}

		[DebugAction("General", null, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void RandomizeColor()
		{
			foreach (Thing item in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()))
			{
				if (item.TryGetComp<CompColorable>() != null)
				{
					item.SetColor(GenColor.RandomColorOpaque());
				}
			}
		}

		[DebugAction("General", "Rot 1 day", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void Rot1Day()
		{
			foreach (Thing item in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()))
			{
				CompRottable compRottable = item.TryGetComp<CompRottable>();
				if (compRottable != null)
				{
					compRottable.RotProgress += 60000f;
				}
			}
		}

		[DebugAction("General", "Force sleep", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void ForceSleep()
		{
			foreach (Thing item in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList())
			{
				CompCanBeDormant compCanBeDormant = item.TryGetComp<CompCanBeDormant>();
				if (compCanBeDormant != null)
				{
					compCanBeDormant.ToSleep();
				}
				else
				{
					Pawn pawn = item as Pawn;
					pawn?.jobs.StartJob(JobMaker.MakeJob(JobDefOf.LayDown, pawn.Position));
				}
			}
		}

		[DebugAction("General", "Fuel -20%", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void FuelRemove20Percent()
		{
			foreach (Thing item in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()))
			{
				CompRefuelable compRefuelable = item.TryGetComp<CompRefuelable>();
				compRefuelable?.ConsumeFuel(compRefuelable.Props.fuelCapacity * 0.2f);
			}
		}

		[DebugAction("General", "Break down...", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void BreakDown()
		{
			foreach (Thing item in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()))
			{
				CompBreakdownable compBreakdownable = item.TryGetComp<CompBreakdownable>();
				if (compBreakdownable != null && !compBreakdownable.BrokenDown)
				{
					compBreakdownable.DoBreakdown();
				}
			}
		}

		[DebugAction("General", null, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void UseScatterer()
		{
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(DebugTools_MapGen.Options_Scatterers()));
		}

		[DebugAction("General", "BaseGen", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void BaseGen()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (string item in DefDatabase<RuleDef>.AllDefs.Select((RuleDef x) => x.symbol).Distinct())
			{
				string localSymbol = item;
				list.Add(new DebugMenuOption(item, DebugMenuOptionMode.Action, delegate
				{
					DebugTool tool = null;
					IntVec3 firstCorner = default(IntVec3);
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
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("General", "SketchGen", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void SketchGen()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (SketchResolverDef item in DefDatabase<SketchResolverDef>.AllDefs.Where((SketchResolverDef x) => x.isRoot))
			{
				SketchResolverDef localResolver = item;
				if (localResolver == SketchResolverDefOf.Monument || localResolver == SketchResolverDefOf.MonumentRuin)
				{
					List<DebugMenuOption> sizeOpts = new List<DebugMenuOption>();
					for (int i = 1; i <= 60; i++)
					{
						int localIndex = i;
						sizeOpts.Add(new DebugMenuOption(localIndex.ToString(), DebugMenuOptionMode.Tool, delegate
						{
							RimWorld.SketchGen.ResolveParams parms2 = default(RimWorld.SketchGen.ResolveParams);
							parms2.sketch = new Sketch();
							parms2.monumentSize = new IntVec2(localIndex, localIndex);
							RimWorld.SketchGen.SketchGen.Generate(localResolver, parms2).Spawn(Find.CurrentMap, UI.MouseCell(), null, Sketch.SpawnPosType.Unchanged, Sketch.SpawnMode.Normal, wipeIfCollides: false, clearEdificeWhereFloor: false, null, dormant: false, buildRoofsInstantly: true);
						}));
					}
					list.Add(new DebugMenuOption(item.defName, DebugMenuOptionMode.Action, delegate
					{
						Find.WindowStack.Add(new Dialog_DebugOptionListLister(sizeOpts));
					}));
				}
				else
				{
					list.Add(new DebugMenuOption(item.defName, DebugMenuOptionMode.Tool, delegate
					{
						RimWorld.SketchGen.ResolveParams parms = default(RimWorld.SketchGen.ResolveParams);
						parms.sketch = new Sketch();
						RimWorld.SketchGen.SketchGen.Generate(localResolver, parms).Spawn(Find.CurrentMap, UI.MouseCell(), null, Sketch.SpawnPosType.Unchanged, Sketch.SpawnMode.Normal, wipeIfCollides: false, clearEdificeWhereFloor: false, null, dormant: false, buildRoofsInstantly: true);
					}));
				}
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("General", null, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void MakeRoof()
		{
			foreach (IntVec3 item in CellRect.CenteredOn(UI.MouseCell(), 1))
			{
				Find.CurrentMap.roofGrid.SetRoof(item, RoofDefOf.RoofConstructed);
			}
		}

		[DebugAction("General", null, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void DeleteRoof()
		{
			foreach (IntVec3 item in CellRect.CenteredOn(UI.MouseCell(), 1))
			{
				Find.CurrentMap.roofGrid.SetRoof(item, null);
			}
		}

		[DebugAction("General", null, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void TestFloodUnfog()
		{
			FloodFillerFog.DebugFloodUnfog(UI.MouseCell(), Find.CurrentMap);
		}

		[DebugAction("General", null, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void FlashClosewalkCell30()
		{
			IntVec3 c = CellFinder.RandomClosewalkCellNear(UI.MouseCell(), Find.CurrentMap, 30);
			Find.CurrentMap.debugDrawer.FlashCell(c);
		}

		[DebugAction("General", null, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void FlashWalkPath()
		{
			WalkPathFinder.DebugFlashWalkPath(UI.MouseCell());
		}

		[DebugAction("General", null, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void FlashSkygazeCell()
		{
			Pawn pawn = Find.CurrentMap.mapPawns.FreeColonists.First();
			RCellFinder.TryFindSkygazeCell(UI.MouseCell(), pawn, out IntVec3 result);
			Find.CurrentMap.debugDrawer.FlashCell(result);
			MoteMaker.ThrowText(result.ToVector3Shifted(), Find.CurrentMap, "for " + pawn.Label, Color.white);
		}

		[DebugAction("General", null, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void FlashDirectFleeDest()
		{
			Pawn pawn = Find.Selector.SingleSelectedThing as Pawn;
			IntVec3 result;
			if (pawn == null)
			{
				Find.CurrentMap.debugDrawer.FlashCell(UI.MouseCell(), 0f, "select a pawn");
			}
			else if (RCellFinder.TryFindDirectFleeDestination(UI.MouseCell(), 9f, pawn, out result))
			{
				Find.CurrentMap.debugDrawer.FlashCell(result, 0.5f);
			}
			else
			{
				Find.CurrentMap.debugDrawer.FlashCell(UI.MouseCell(), 0.8f, "not found");
			}
		}

		[DebugAction("General", null, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void FlashSpectatorsCells()
		{
			Action<bool> act = delegate(bool bestSideOnly)
			{
				DebugTool tool = null;
				IntVec3 firstCorner = default(IntVec3);
				tool = new DebugTool("first watch rect corner...", delegate
				{
					firstCorner = UI.MouseCell();
					DebugTools.curTool = new DebugTool("second watch rect corner...", delegate
					{
						IntVec3 second = UI.MouseCell();
						CellRect spectateRect = CellRect.FromLimits(firstCorner, second).ClipInsideMap(Find.CurrentMap);
						SpectateRectSide allowedSides = SpectateRectSide.All;
						if (bestSideOnly)
						{
							allowedSides = SpectatorCellFinder.FindSingleBestSide(spectateRect, Find.CurrentMap);
						}
						SpectatorCellFinder.DebugFlashPotentialSpectatorCells(spectateRect, Find.CurrentMap, allowedSides);
						DebugTools.curTool = tool;
					}, firstCorner);
				});
				DebugTools.curTool = tool;
			};
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			list.Add(new DebugMenuOption("All sides", DebugMenuOptionMode.Action, delegate
			{
				act(obj: false);
			}));
			list.Add(new DebugMenuOption("Best side only", DebugMenuOptionMode.Action, delegate
			{
				act(obj: true);
			}));
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("General", null, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void CheckReachability()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			TraverseMode[] array = (TraverseMode[])Enum.GetValues(typeof(TraverseMode));
			for (int i = 0; i < array.Length; i++)
			{
				TraverseMode traverseMode2 = array[i];
				TraverseMode traverseMode = traverseMode2;
				list.Add(new DebugMenuOption(traverseMode2.ToString(), DebugMenuOptionMode.Action, delegate
				{
					DebugTool tool = null;
					IntVec3 from = default(IntVec3);
					Pawn fromPawn = default(Pawn);
					tool = new DebugTool("from...", delegate
					{
						from = UI.MouseCell();
						fromPawn = from.GetFirstPawn(Find.CurrentMap);
						string text = "to...";
						if (fromPawn != null)
						{
							text = text + " (pawn=" + fromPawn.LabelShort + ")";
						}
						DebugTools.curTool = new DebugTool(text, delegate
						{
							DebugTools.curTool = tool;
						}, delegate
						{
							IntVec3 c = UI.MouseCell();
							bool flag;
							IntVec3 intVec;
							if (fromPawn != null)
							{
								flag = fromPawn.CanReach(c, PathEndMode.OnCell, Danger.Deadly, canBash: false, traverseMode);
								intVec = fromPawn.Position;
							}
							else
							{
								flag = Find.CurrentMap.reachability.CanReach(from, c, PathEndMode.OnCell, traverseMode, Danger.Deadly);
								intVec = from;
							}
							Color color = flag ? Color.green : Color.red;
							Widgets.DrawLine(intVec.ToUIPosition(), c.ToUIPosition(), color, 2f);
						});
					});
					DebugTools.curTool = tool;
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("General", "Flash TryFindRandomPawnExitCell", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void FlashTryFindRandomPawnExitCell(Pawn p)
		{
			if (CellFinder.TryFindRandomPawnExitCell(p, out IntVec3 result))
			{
				p.Map.debugDrawer.FlashCell(result, 0.5f);
				p.Map.debugDrawer.FlashLine(p.Position, result);
			}
			else
			{
				p.Map.debugDrawer.FlashCell(p.Position, 0.2f, "no exit cell");
			}
		}

		[DebugAction("General", "RandomSpotJustOutsideColony", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void RandomSpotJustOutsideColony(Pawn p)
		{
			if (RCellFinder.TryFindRandomSpotJustOutsideColony(p, out IntVec3 result))
			{
				p.Map.debugDrawer.FlashCell(result, 0.5f);
				p.Map.debugDrawer.FlashLine(p.Position, result);
			}
			else
			{
				p.Map.debugDrawer.FlashCell(p.Position, 0.2f, "no cell");
			}
		}
	}
}
