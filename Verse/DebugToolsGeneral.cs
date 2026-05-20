using System;
using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using RimWorld;
using UnityEngine;
using Verse.AI;

namespace Verse
{
	public static class DebugToolsGeneral
	{
		private struct ExplosionData
		{
			public DamageDef damageDef;

			public GasType? gasType;

			public ThingDef thingDef;

			public float thingChance;

			public int thingCount;

			public bool applyToNeighbors;
		}

		private static List<ExplosionData> explosionDatas;

		private static readonly int[] HeatPushOptions = new int[7] { 10, 50, 100, 1000, -10, -50, -1000 };

		private static List<ThingStyleDef> tmpStyleDefs = new List<ThingStyleDef>();

		[DebugAction("General", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
		private static void Destroy()
		{
			Thing.allowDestroyNonDestroyable = true;
			try
			{
				foreach (Thing item in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList())
				{
					item.Destroy();
				}
			}
			finally
			{
				Thing.allowDestroyNonDestroyable = false;
			}
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
		private static void Kill()
		{
			foreach (Thing item in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList())
			{
				item.Kill();
			}
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 999)]
		private static void SetFaction()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			List<Thing> things = Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList();
			foreach (Faction item2 in Find.FactionManager.AllFactionsInViewOrder)
			{
				Faction localFac = item2;
				FloatMenuOption item = new FloatMenuOption(localFac.Name, delegate
				{
					foreach (Thing item3 in things)
					{
						if (item3.def.CanHaveFaction)
						{
							item3.SetFaction(localFac);
						}
					}
				});
				list.Add(item);
			}
			list.Add(new FloatMenuOption("None", delegate
			{
				foreach (Thing item4 in things)
				{
					if (item4.def.CanHaveFaction)
					{
						item4.SetFaction(null);
					}
				}
			}));
			Find.WindowStack.Add(new FloatMenu(list));
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 999)]
		private static void SetFactionRect()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (Faction item2 in Find.FactionManager.AllFactionsInViewOrder)
			{
				Faction localFac = item2;
				FloatMenuOption item = new FloatMenuOption(localFac.Name, delegate
				{
					GenericRectTool(localFac.Name, delegate(CellRect rect)
					{
						foreach (IntVec3 cell in rect.Cells)
						{
							foreach (Thing item3 in Find.CurrentMap.thingGrid.ThingsAt(cell).ToList())
							{
								if (item3.def.CanHaveFaction)
								{
									item3.SetFaction(localFac);
								}
							}
						}
					});
				});
				list.Add(item);
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void Teleport()
		{
			if (UI.MouseCell().TryGetFirstThing<Pawn>(Find.CurrentMap, out var pawn))
			{
				MoteMaker.ThrowText(pawn.DrawPos, Find.CurrentMap, "Selected " + pawn.Label);
				DebugTools.curTool = new DebugTool("Destination", delegate
				{
					IntVec3 intVec = UI.MouseCell();
					pawn.Position = intVec.ClampInsideMap(Find.CurrentMap);
					pawn.Notify_Teleported();
					DebugTools.curTool = null;
				});
			}
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void PickCopy()
		{
			if (!UI.MouseCell().TryGetFirstThing<Thing>(Find.CurrentMap, out var thing))
			{
				return;
			}
			MoteMaker.ThrowText(thing.DrawPos, Find.CurrentMap, "Copied " + thing.Label);
			DebugTools.curTool = new DebugTool("Place", delegate
			{
				IntVec3 loc = UI.MouseCell();
				Thing thing2 = GenSpawn.Spawn(thing.def, loc, Find.CurrentMap, thing.Rotation);
				if (thing.TryGetComp(out CompQuality comp) && thing2.TryGetComp(out CompQuality comp2))
				{
					comp2.SetQuality(comp.Quality, ArtGenerationContext.Outsider);
				}
				if (thing.def.CanHaveFaction)
				{
					thing2.SetFactionDirect(thing.Faction);
				}
			});
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void Discard()
		{
			foreach (Thing item in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList())
			{
				item.Destroy();
				if (item is Pawn p)
				{
					Find.WorldPawns.RemoveAndDiscardPawnViaGC(p);
				}
				else
				{
					item.Discard();
				}
			}
		}

		[DebugAction("General", "10 damage", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void Take10Damage()
		{
			foreach (Thing item in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList())
			{
				item.TakeDamage(new DamageInfo(DamageDefOf.Crush, 10f));
			}
		}

		[DebugAction("General", "300 damage", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void Take300Damage()
		{
			foreach (Thing item in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList())
			{
				item.TakeDamage(new DamageInfo(DamageDefOf.Crush, 300f));
			}
		}

		[DebugAction("General", "5000 damage", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void Take5000Damage()
		{
			foreach (Thing item in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList())
			{
				item.TakeDamage(new DamageInfo(DamageDefOf.Crush, 50000f));
			}
		}

		[DebugAction("General", "Clear area (rect)", false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 100)]
		private static void ClearArea()
		{
			GenericRectTool("Clear", delegate(CellRect rect)
			{
				GenDebug.ClearArea(rect, Find.CurrentMap);
			});
		}

		[DebugAction("Spawning", "Spawn fill area (rect)", false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 100)]
		private static List<DebugActionNode> SpawnFillArea()
		{
			List<DebugActionNode> list = new List<DebugActionNode>();
			foreach (ThingDef item in DefDatabase<ThingDef>.AllDefs.OrderBy((ThingDef d) => d.defName))
			{
				ThingDef localDef = item;
				list.Add(new DebugActionNode(localDef.defName, DebugActionType.Action, delegate
				{
					GenericRectTool("Spawn", delegate(CellRect rect)
					{
						GenDebug.SpawnArea(rect, Find.CurrentMap, localDef);
					});
				}));
			}
			return list;
		}

		[DebugAction("General", "Make empty room (rect)", false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 100)]
		private static void MakeEmptyRoom()
		{
			GenericRectTool("Make room", delegate(CellRect rect)
			{
				GenDebug.ClearArea(rect, Find.CurrentMap);
				IEnumerable<IntVec3> edgeCells = rect.EdgeCells;
				IntVec3 result = IntVec3.Invalid;
				edgeCells.Where((IntVec3 x) => !rect.IsCorner(x)).TryRandomElement(out result);
				foreach (IntVec3 item in edgeCells)
				{
					Thing thing = ThingMaker.MakeThing((item == result) ? ThingDefOf.Door : ThingDefOf.Wall, ThingDefOf.WoodLog);
					thing.SetFaction(Faction.OfPlayer);
					GenPlace.TryPlaceThing(thing, item, Find.CurrentMap, ThingPlaceMode.Direct);
				}
				foreach (IntVec3 item2 in rect)
				{
					Find.CurrentMap.roofGrid.SetRoof(item2, RoofDefOf.RoofConstructed);
					Find.CurrentMap.terrainGrid.SetTerrain(item2, TerrainDefOf.WoodPlankFloor);
				}
			});
		}

		[DebugAction("General", "Edit roof (rect)", false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 100)]
		private static List<DebugActionNode> MakeRoof()
		{
			List<DebugActionNode> list = new List<DebugActionNode>();
			list.Add(new DebugActionNode("Clear", DebugActionType.Action, delegate
			{
				GenericRectTool("Clear roof", delegate(CellRect rect)
				{
					foreach (IntVec3 item in rect)
					{
						Find.CurrentMap.roofGrid.SetRoof(item, null);
					}
				});
			}));
			foreach (RoofDef allDef in DefDatabase<RoofDef>.AllDefs)
			{
				RoofDef localDef = allDef;
				list.Add(new DebugActionNode(localDef.LabelCap, DebugActionType.Action, delegate
				{
					GenericRectTool("Make roof (" + localDef.label + ")", delegate(CellRect rect)
					{
						foreach (IntVec3 item2 in rect)
						{
							Find.CurrentMap.roofGrid.SetRoof(item2, localDef);
						}
					});
				}));
			}
			return list;
		}

		[DebugAction("General", "Fog (rect)", false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 100)]
		private static void FogRect()
		{
			GenericRectTool("Fog", delegate(CellRect rect)
			{
				Find.CurrentMap.fogGrid.Refog(rect.ClipInsideMap(Find.CurrentMap));
			});
		}

		[DebugAction("General", "Unfog (rect)", false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 100)]
		private static void UnfogRect()
		{
			GenericRectTool("Clear", delegate(CellRect rect)
			{
				foreach (IntVec3 item in rect.ClipInsideMap(Find.CurrentMap))
				{
					Find.CurrentMap.fogGrid.Unfog(item);
				}
			});
		}

		public static void GenericRectTool(string label, Action<CellRect> rectAction, bool closeOnComplete = false)
		{
			DebugTool tool = null;
			IntVec3 firstCorner;
			tool = new DebugTool(label + ": First corner...", delegate
			{
				firstCorner = UI.MouseCell();
				DebugTools.curTool = new DebugTool(label + ": Second corner...", delegate
				{
					IntVec3 second = UI.MouseCell();
					CellRect obj = CellRect.FromLimits(firstCorner, second).ClipInsideMap(Find.CurrentMap);
					rectAction(obj);
					DebugTools.curTool = (closeOnComplete ? null : tool);
				}, firstCorner);
			});
			DebugTools.curTool = tool;
		}

		[DebugAction("General", "Explosion...", false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static List<DebugActionNode> Explosion()
		{
			if (explosionDatas.NullOrEmpty())
			{
				explosionDatas = new List<ExplosionData>
				{
					new ExplosionData
					{
						damageDef = DamageDefOf.Bomb
					},
					new ExplosionData
					{
						damageDef = DamageDefOf.Flame
					},
					new ExplosionData
					{
						damageDef = DamageDefOf.Stun
					},
					new ExplosionData
					{
						damageDef = DamageDefOf.EMP
					},
					new ExplosionData
					{
						damageDef = DamageDefOf.Extinguish,
						thingDef = ThingDefOf.Filth_FireFoam,
						thingChance = 1f,
						thingCount = 3,
						applyToNeighbors = true
					},
					new ExplosionData
					{
						damageDef = DamageDefOf.Smoke,
						gasType = GasType.BlindSmoke
					}
				};
				if (ModsConfig.BiotechActive)
				{
					explosionDatas.Add(new ExplosionData
					{
						damageDef = DamageDefOf.ToxGas,
						gasType = GasType.ToxGas
					});
					explosionDatas.Add(new ExplosionData
					{
						damageDef = DamageDefOf.Vaporize
					});
				}
				if (ModsConfig.OdysseyActive)
				{
					explosionDatas.Add(new ExplosionData
					{
						damageDef = DamageDefOf.MiningBomb
					});
				}
			}
			List<DebugActionNode> list = new List<DebugActionNode>();
			for (int i = 0; i < explosionDatas.Count; i++)
			{
				ExplosionData data = explosionDatas[i];
				list.Add(new DebugActionNode(data.damageDef.LabelCap, DebugActionType.ToolMap, delegate
				{
					IntVec3 center = UI.MouseCell();
					Map currentMap = Find.CurrentMap;
					DamageDef damageDef = data.damageDef;
					ThingDef thingDef = data.thingDef;
					float thingChance = data.thingChance;
					int thingCount = data.thingCount;
					GasType? gasType = data.gasType;
					bool applyToNeighbors = data.applyToNeighbors;
					GenExplosion.DoExplosion(center, currentMap, 4.9f, damageDef, null, -1, -1f, null, null, null, null, thingDef, thingChance, thingCount, gasType, null, 255, applyToNeighbors);
				}));
			}
			return list;
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = -1000)]
		private static void LightningStrike()
		{
			Find.CurrentMap.weatherManager.eventHandler.AddEvent(new WeatherEvent_LightningStrike(Find.CurrentMap, UI.MouseCell()));
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = -1000)]
		private static List<DebugActionNode> LightningStrikeDelayed()
		{
			List<DebugActionNode> list = new List<DebugActionNode>();
			for (int i = 0; i < 41; i++)
			{
				int delay = i * 30;
				list.Add(new DebugActionNode(((float)delay / 60f).ToString("F1") + " seconds", DebugActionType.ToolMap, delegate
				{
					Find.CurrentMap.weatherManager.eventHandler.AddEvent(new WeatherEvent_LightningStrikeDelayed(Find.CurrentMap, UI.MouseCell(), delay));
				}));
			}
			return list;
		}

		[DebugAction("General", "Add gas...", false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = -200)]
		private static List<DebugActionNode> PushGas()
		{
			List<DebugActionNode> list = new List<DebugActionNode>();
			foreach (GasType value in Enum.GetValues(typeof(GasType)))
			{
				GasType gasType2 = value;
				if ((gasType2 != GasType.ToxGas || ModsConfig.BiotechActive) && (gasType2 != GasType.DeadlifeDust || ModsConfig.AnomalyActive))
				{
					list.Add(new DebugActionNode(gasType2.GetLabel().CapitalizeFirst(), DebugActionType.ToolMap, delegate
					{
						GasUtility.AddGas(UI.MouseCell(), Find.CurrentMap, gasType2, 5f);
					}));
				}
			}
			return list;
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = -200)]
		private static void ClearAllGas()
		{
			Find.CurrentMap.gasGrid.Debug_ClearAll();
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = -1000)]
		private static void FillAllGas()
		{
			Find.CurrentMap.gasGrid.Debug_FillAll();
		}

		[DebugAction("General", "Push heat...", false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = -200)]
		private static List<DebugActionNode> PushHeat()
		{
			List<DebugActionNode> list = new List<DebugActionNode>();
			for (int i = 0; i < HeatPushOptions.Length; i++)
			{
				int t = HeatPushOptions[i];
				list.Add(new DebugActionNode(t.ToString(), DebugActionType.ToolMap, delegate
				{
					Room room = UI.MouseCell().GetRoom(Find.CurrentMap);
					if (room != null && !room.UsesOutdoorTemperature)
					{
						foreach (IntVec3 cell in room.Cells)
						{
							GenTemperature.PushHeat(cell, Find.CurrentMap, t);
						}
					}
				}));
			}
			return list;
		}

		[DebugAction("General", "Grow plant 1 day", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
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

		[DebugAction("General", "Grow plant to maturity", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void GrowPlantToMaturity()
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

		[DebugAction("General", "Make plant leafless", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void MakePlantLeafless()
		{
			IntVec3 intVec = UI.MouseCell();
			Plant plant = intVec.GetPlant(Find.CurrentMap);
			if (plant != null && plant.def.plant != null)
			{
				plant.MakeLeafless(Plant.LeaflessCause.Cold, sendMessage: false);
				Find.CurrentMap.mapDrawer.SectionAt(intVec).RegenerateAllLayers();
			}
		}

		[DebugAction("General", "Rotate", false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static DebugActionNode Rotate()
		{
			DebugActionNode debugActionNode = new DebugActionNode();
			debugActionNode.AddChild(new DebugActionNode("Clockwise", DebugActionType.ToolMap, delegate
			{
				foreach (Thing item in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList())
				{
					item.Rotation = item.Rotation.Rotated(RotationDirection.Clockwise);
					item.DirtyMapMesh(item.Map);
				}
			}));
			debugActionNode.AddChild(new DebugActionNode("Counter clockwise", DebugActionType.ToolMap, delegate
			{
				foreach (Thing item2 in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList())
				{
					item2.Rotation = item2.Rotation.Rotated(RotationDirection.Counterclockwise);
					item2.DirtyMapMesh(item2.Map);
				}
			}));
			return debugActionNode;
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void SetColor()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			IntVec3 cell = UI.MouseCell();
			list.Add(new FloatMenuOption("Random", delegate
			{
				SetColor_All(GenColor.RandomColorOpaque());
			}));
			foreach (Ideo i in Find.IdeoManager.IdeosListForReading)
			{
				if (!i.classicMode && i.Icon != BaseContent.BadTex)
				{
					list.Add(new FloatMenuOption(i.name, delegate
					{
						SetColor_All(i.Color);
					}, i.Icon, i.Color));
				}
			}
			foreach (ColorDef c in DefDatabase<ColorDef>.AllDefs)
			{
				list.Add(new FloatMenuOption(c.defName, delegate
				{
					SetColor_All(c.color);
				}, BaseContent.WhiteTex, c.color));
			}
			Find.WindowStack.Add(new FloatMenu(list));
			void SetColor_All(Color color)
			{
				foreach (Thing item in Find.CurrentMap.thingGrid.ThingsAt(cell).ToList())
				{
					if (item is Pawn { apparel: not null } pawn)
					{
						foreach (Apparel item2 in pawn.apparel.WornApparel)
						{
							item2.SetColor(color, reportFailure: false);
						}
					}
					else
					{
						item.SetColor(color, reportFailure: false);
					}
				}
			}
		}

		[DebugAction("General", "Rot 1 day", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
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

		[DebugAction("General", "Force sleep", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void ForceSleep()
		{
			foreach (Thing item in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList())
			{
				CompCanBeDormant compCanBeDormant = item.TryGetComp<CompCanBeDormant>();
				if (compCanBeDormant != null)
				{
					compCanBeDormant.ToSleep();
				}
				else if (item is Pawn pawn)
				{
					pawn.jobs.StartJob(JobMaker.MakeJob(JobDefOf.LayDown, pawn.Position));
				}
			}
		}

		[DebugAction("General", "Break down...", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
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

		[DebugAction("General", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
		private static void TestFloodUnfog()
		{
			FloodFillerFog.DebugFloodUnfog(UI.MouseCell(), Find.CurrentMap);
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
		private static void FlashClosewalkCell30()
		{
			IntVec3 c = CellFinder.RandomClosewalkCellNear(UI.MouseCell(), Find.CurrentMap, 30);
			Find.CurrentMap.debugDrawer.FlashCell(c);
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
		private static void FlashWalkPath()
		{
			WalkPathFinder.DebugFlashWalkPath(UI.MouseCell());
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
		private static void FlashSkygazeCell()
		{
			Pawn pawn = Find.CurrentMap.mapPawns.FreeColonists.First();
			RCellFinder.TryFindSkygazeCell(UI.MouseCell(), pawn, out var result);
			Find.CurrentMap.debugDrawer.FlashCell(result);
			MoteMaker.ThrowText(result.ToVector3Shifted(), Find.CurrentMap, "for " + pawn.Label, Color.white);
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
		private static void FlashDirectFleeDest()
		{
			IntVec3 result;
			if (!(Find.Selector.SingleSelectedThing is Pawn pawn))
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

		[DebugAction("General", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.IsCurrentlyOnMap, hideInSubMenu = true)]
		private static void FlashShuttleDropCellsNear()
		{
			IntVec3 center = UI.MouseCell();
			Map currentMap = Find.CurrentMap;
			for (int i = 0; i < 100; i++)
			{
				DropCellFinder.TryFindDropSpotNear(center, currentMap, out var result, allowFogged: false, canRoofPunch: false, allowIndoors: false, ThingDefOf.Shuttle.Size + new IntVec2(2, 2), mustBeReachableFromCenter: false);
				currentMap.debugDrawer.FlashCell(result, 0.2f);
			}
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
		private static void FlashSpectatorsCells()
		{
			Action<bool> act = delegate(bool bestSideOnly)
			{
				DebugTool tool = null;
				IntVec3 firstCorner;
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

		[DebugAction("General", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static List<DebugActionNode> CheckReachability()
		{
			List<DebugActionNode> list = new List<DebugActionNode>();
			TraverseMode[] array = (TraverseMode[])Enum.GetValues(typeof(TraverseMode));
			for (int i = 0; i < array.Length; i++)
			{
				TraverseMode traverseMode = array[i];
				TraverseMode traverseMode2 = traverseMode;
				list.Add(new DebugActionNode(traverseMode.ToString(), DebugActionType.Action, delegate
				{
					DebugTool tool = null;
					IntVec3 from;
					Pawn fromPawn;
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
							IntVec3 intVec = UI.MouseCell();
							bool flag;
							IntVec3 intVec2;
							if (fromPawn != null)
							{
								flag = fromPawn.CanReach(intVec, PathEndMode.OnCell, Danger.Deadly, canBashDoors: false, canBashFences: false, traverseMode2);
								intVec2 = fromPawn.Position;
							}
							else
							{
								flag = Find.CurrentMap.reachability.CanReach(from, intVec, PathEndMode.OnCell, traverseMode2, Danger.Deadly);
								intVec2 = from;
							}
							Color color = (flag ? Color.green : Color.red);
							Widgets.DrawLine(intVec2.ToUIPosition(), intVec.ToUIPosition(), color, 2f);
						});
					});
					DebugTools.curTool = tool;
				}));
			}
			return list;
		}

		[DebugAction("General", "Flash TryFindRandomPawnExitCell", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
		private static void FlashTryFindRandomPawnExitCell(Pawn p)
		{
			if (CellFinder.TryFindRandomPawnExitCell(p, out var result))
			{
				p.Map.debugDrawer.FlashCell(result, 0.5f);
				p.Map.debugDrawer.FlashLine(p.Position, result);
			}
			else
			{
				p.Map.debugDrawer.FlashCell(p.Position, 0.2f, "no exit cell");
			}
		}

		[DebugAction("General", "RandomSpotJustOutsideColony", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
		private static void RandomSpotJustOutsideColony(Pawn p)
		{
			if (RCellFinder.TryFindRandomSpotJustOutsideColony(p, out var result))
			{
				p.Map.debugDrawer.FlashCell(result, 0.5f);
				p.Map.debugDrawer.FlashLine(p.Position, result);
			}
			else
			{
				p.Map.debugDrawer.FlashCell(p.Position, 0.2f, "no cell");
			}
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
		private static void RandomSpotNearThingAvoidingHostiles()
		{
			List<Thing> list = Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList();
			if (list.Count != 0)
			{
				Thing thing = list.Where((Thing t) => t is Pawn && t.Faction != null).FirstOrDefault();
				if (thing == null)
				{
					thing = list.First();
				}
				if (RCellFinder.TryFindRandomSpotNearAvoidingHostilePawns(thing, thing.Map, (IntVec3 s) => true, out var result))
				{
					thing.Map.debugDrawer.FlashCell(result, 0.5f);
					thing.Map.debugDrawer.FlashLine(thing.Position, result);
				}
				else
				{
					thing.Map.debugDrawer.FlashCell(thing.Position, 0.2f, "no cell");
				}
			}
		}

		[DebugAction("Map", null, false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void ClearAllFog()
		{
			Find.CurrentMap.fogGrid.ClearAllFog();
		}

		[DebugAction("General", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void ChangeThingStyle()
		{
			Thing thing = Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).FirstOrDefault((Thing x) => x.def.CanBeStyled());
			if (thing == null)
			{
				return;
			}
			tmpStyleDefs.Clear();
			if (!thing.def.randomStyle.NullOrEmpty())
			{
				foreach (ThingStyleChance item in thing.def.randomStyle)
				{
					if (item.StyleDef.graphicData != null)
					{
						tmpStyleDefs.Add(item.StyleDef);
					}
				}
			}
			foreach (StyleCategoryDef item2 in DefDatabase<StyleCategoryDef>.AllDefs.Where((StyleCategoryDef x) => x.thingDefStyles.Any((ThingDefStyle y) => y.ThingDef == thing.def)))
			{
				tmpStyleDefs.Add(item2.GetStyleForThingDef(thing.def));
			}
			if (!tmpStyleDefs.Any())
			{
				return;
			}
			List<DebugMenuOption> opts = new List<DebugMenuOption>();
			AddOption(thing, () => (ThingStyleDef)null, "Standard");
			AddOption(thing, () => tmpStyleDefs.RandomElementByWeight((ThingStyleDef x) => (x != thing.StyleDef) ? 1f : 0.01f), "Random");
			foreach (ThingStyleDef s in tmpStyleDefs)
			{
				AddOption(thing, () => s, s.defName);
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(opts));
			void AddOption(Thing t, Func<ThingStyleDef> styleSelector, string label)
			{
				opts.Add(new DebugMenuOption(label, DebugMenuOptionMode.Action, delegate
				{
					t.StyleDef = styleSelector();
					t.DirtyMapMesh(t.Map);
				}));
			}
		}

		[DebugAction("General", "Destroy fire", false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void DestroyAllFire()
		{
			Thing[] array = Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.Fire).ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Destroy();
			}
		}
	}
}
