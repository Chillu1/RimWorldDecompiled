using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse.AI.Group;

namespace Verse
{
	public static class DebugToolsSpawning
	{
		private static IEnumerable<float> PointsMechCluster()
		{
			for (float points = 50f; points <= 10000f; points += 50f)
			{
				yield return points;
			}
		}

		[DebugAction("Spawning", null, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void SpawnPawn()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (PawnKindDef item in DefDatabase<PawnKindDef>.AllDefs.OrderBy((PawnKindDef kd) => kd.defName))
			{
				PawnKindDef localKindDef = item;
				list.Add(new DebugMenuOption(localKindDef.defName, DebugMenuOptionMode.Tool, delegate
				{
					Faction faction = FactionUtility.DefaultFactionFrom(localKindDef.defaultFactionType);
					Pawn newPawn = PawnGenerator.GeneratePawn(localKindDef, faction);
					GenSpawn.Spawn(newPawn, UI.MouseCell(), Find.CurrentMap);
					if (faction != null && faction != Faction.OfPlayer)
					{
						Lord lord = null;
						if (newPawn.Map.mapPawns.SpawnedPawnsInFaction(faction).Any((Pawn p) => p != newPawn))
						{
							lord = ((Pawn)GenClosest.ClosestThing_Global(newPawn.Position, newPawn.Map.mapPawns.SpawnedPawnsInFaction(faction), 99999f, (Thing p) => p != newPawn && ((Pawn)p).GetLord() != null)).GetLord();
						}
						if (lord == null)
						{
							LordJob_DefendPoint lordJob = new LordJob_DefendPoint(newPawn.Position);
							lord = LordMaker.MakeNewLord(faction, lordJob, Find.CurrentMap);
						}
						lord.AddPawn(newPawn);
					}
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("Spawning", null, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void SpawnWeapon()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (ThingDef item in from def in DefDatabase<ThingDef>.AllDefs
				where def.equipmentType == EquipmentType.Primary
				select def into d
				orderby d.defName
				select d)
			{
				ThingDef localDef = item;
				list.Add(new DebugMenuOption(localDef.defName, DebugMenuOptionMode.Tool, delegate
				{
					DebugThingPlaceHelper.DebugSpawn(localDef, UI.MouseCell());
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("Spawning", "Spawn apparel...", allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void SpawnApparel()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (ThingDef item in from def in DefDatabase<ThingDef>.AllDefs
				where def.IsApparel
				select def into d
				orderby d.defName
				select d)
			{
				ThingDef localDef = item;
				list.Add(new DebugMenuOption(localDef.defName, DebugMenuOptionMode.Tool, delegate
				{
					DebugThingPlaceHelper.DebugSpawn(localDef, UI.MouseCell());
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("Spawning", "Try place near thing...", allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void TryPlaceNearThing()
		{
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(DebugThingPlaceHelper.TryPlaceOptionsForStackCount(1, direct: false)));
		}

		[DebugAction("Spawning", "Try place near stack of 25...", allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void TryPlaceNearStacksOf25()
		{
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(DebugThingPlaceHelper.TryPlaceOptionsForStackCount(25, direct: false)));
		}

		[DebugAction("Spawning", "Try place near stack of 75...", allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void TryPlaceNearStacksOf75()
		{
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(DebugThingPlaceHelper.TryPlaceOptionsForStackCount(75, direct: false)));
		}

		[DebugAction("Spawning", "Try place direct thing...", allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void TryPlaceDirectThing()
		{
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(DebugThingPlaceHelper.TryPlaceOptionsForStackCount(1, direct: true)));
		}

		[DebugAction("Spawning", "Try place direct stack of 25...", allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void TryPlaceDirectStackOf25()
		{
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(DebugThingPlaceHelper.TryPlaceOptionsForStackCount(25, direct: true)));
		}

		[DebugAction("Spawning", "Try add to inventory...", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void TryAddToInventory(Pawn p)
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs)
			{
				if (def.category == ThingCategory.Item)
				{
					list.Add(new DebugMenuOption(def.label, DebugMenuOptionMode.Action, delegate
					{
						p.inventory.TryAddItemNotForSale(ThingMaker.MakeThing(def));
					}));
				}
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("Spawning", "Spawn thing with wipe mode...", allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void SpawnThingWithWipeMode()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			WipeMode[] array = (WipeMode[])Enum.GetValues(typeof(WipeMode));
			for (int i = 0; i < array.Length; i++)
			{
				WipeMode wipeMode = array[i];
				WipeMode localWipeMode = wipeMode;
				list.Add(new DebugMenuOption(wipeMode.ToString(), DebugMenuOptionMode.Action, delegate
				{
					Find.WindowStack.Add(new Dialog_DebugOptionListLister(DebugThingPlaceHelper.SpawnOptions(localWipeMode)));
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("Spawning", "Set terrain...", allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void SetTerrain()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (TerrainDef allDef in DefDatabase<TerrainDef>.AllDefs)
			{
				TerrainDef localDef = allDef;
				list.Add(new DebugMenuOption(localDef.LabelCap, DebugMenuOptionMode.Tool, delegate
				{
					if (UI.MouseCell().InBounds(Find.CurrentMap))
					{
						Find.CurrentMap.terrainGrid.SetTerrain(UI.MouseCell(), localDef);
					}
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("Spawning", null, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void SpawnMechCluster()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (float item in PointsMechCluster())
			{
				float localPoints = item;
				list.Add(new DebugMenuOption(item + " points", DebugMenuOptionMode.Action, delegate
				{
					List<DebugMenuOption> options = new List<DebugMenuOption>
					{
						new DebugMenuOption("In pods, click place", DebugMenuOptionMode.Tool, delegate
						{
							MechClusterSketch sketch4 = MechClusterGenerator.GenerateClusterSketch_NewTemp(localPoints, Find.CurrentMap);
							MechClusterUtility.SpawnCluster(UI.MouseCell(), Find.CurrentMap, sketch4);
						}),
						new DebugMenuOption("In pods, autoplace", DebugMenuOptionMode.Action, delegate
						{
							MechClusterSketch sketch3 = MechClusterGenerator.GenerateClusterSketch_NewTemp(localPoints, Find.CurrentMap);
							MechClusterUtility.SpawnCluster(MechClusterUtility.FindClusterPosition(Find.CurrentMap, sketch3), Find.CurrentMap, sketch3);
						}),
						new DebugMenuOption("Direct spawn, click place", DebugMenuOptionMode.Tool, delegate
						{
							MechClusterSketch sketch2 = MechClusterGenerator.GenerateClusterSketch_NewTemp(localPoints, Find.CurrentMap);
							MechClusterUtility.SpawnCluster(UI.MouseCell(), Find.CurrentMap, sketch2, dropInPods: false);
						}),
						new DebugMenuOption("Direct spawn, autoplace", DebugMenuOptionMode.Action, delegate
						{
							MechClusterSketch sketch = MechClusterGenerator.GenerateClusterSketch_NewTemp(localPoints, Find.CurrentMap);
							MechClusterUtility.SpawnCluster(MechClusterUtility.FindClusterPosition(Find.CurrentMap, sketch), Find.CurrentMap, sketch, dropInPods: false);
						})
					};
					Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("Spawning", "Make filth x100", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void MakeFilthx100()
		{
			for (int i = 0; i < 100; i++)
			{
				IntVec3 c = UI.MouseCell() + GenRadial.RadialPattern[i];
				if (c.InBounds(Find.CurrentMap) && c.Walkable(Find.CurrentMap))
				{
					FilthMaker.TryMakeFilth(c, Find.CurrentMap, ThingDefOf.Filth_Dirt, 2);
					MoteMaker.ThrowMetaPuff(c.ToVector3Shifted(), Find.CurrentMap);
				}
			}
		}

		[DebugAction("Spawning", null, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void SpawnFactionLeader()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (Faction allFaction in Find.FactionManager.AllFactions)
			{
				Faction localFac = allFaction;
				if (localFac.leader != null)
				{
					list.Add(new FloatMenuOption(localFac.Name + " - " + localFac.leader.Name.ToStringFull, delegate
					{
						GenSpawn.Spawn(localFac.leader, UI.MouseCell(), Find.CurrentMap);
					}));
				}
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		[DebugAction("Spawning", "Spawn world pawn...", allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void SpawnWorldPawn()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			Action<Pawn> act = delegate(Pawn p)
			{
				List<DebugMenuOption> list2 = new List<DebugMenuOption>();
				PawnKindDef kLocal = default(PawnKindDef);
				foreach (PawnKindDef item in DefDatabase<PawnKindDef>.AllDefs.Where((PawnKindDef x) => x.race == p.def))
				{
					kLocal = item;
					list2.Add(new DebugMenuOption(kLocal.defName, DebugMenuOptionMode.Tool, delegate
					{
						PawnGenerationRequest request = new PawnGenerationRequest(kLocal, p.Faction);
						PawnGenerator.RedressPawn(p, request);
						GenSpawn.Spawn(p, UI.MouseCell(), Find.CurrentMap);
						DebugTools.curTool = null;
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
			};
			foreach (Pawn item2 in Find.WorldPawns.AllPawnsAlive)
			{
				Pawn pLocal = item2;
				list.Add(new DebugMenuOption(item2.LabelShort, DebugMenuOptionMode.Action, delegate
				{
					act(pLocal);
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("Spawning", "Spawn thing set...", allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void SpawnThingSet()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			List<ThingSetMakerDef> allDefsListForReading = DefDatabase<ThingSetMakerDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				ThingSetMakerDef localGenerator = allDefsListForReading[i];
				list.Add(new DebugMenuOption(localGenerator.defName, DebugMenuOptionMode.Tool, delegate
				{
					if (UI.MouseCell().InBounds(Find.CurrentMap))
					{
						StringBuilder stringBuilder = new StringBuilder();
						string nonNullFieldsDebugInfo = Gen.GetNonNullFieldsDebugInfo(localGenerator.debugParams);
						List<Thing> list2 = localGenerator.root.Generate(localGenerator.debugParams);
						stringBuilder.Append(localGenerator.defName + " generated " + list2.Count + " things");
						if (!nonNullFieldsDebugInfo.NullOrEmpty())
						{
							stringBuilder.Append(" (used custom debug params: " + nonNullFieldsDebugInfo + ")");
						}
						stringBuilder.AppendLine(":");
						float num = 0f;
						float num2 = 0f;
						for (int j = 0; j < list2.Count; j++)
						{
							stringBuilder.AppendLine("   - " + list2[j].LabelCap);
							num += list2[j].MarketValue * (float)list2[j].stackCount;
							if (!(list2[j] is Pawn))
							{
								num2 += list2[j].GetStatValue(StatDefOf.Mass) * (float)list2[j].stackCount;
							}
							if (!GenPlace.TryPlaceThing(list2[j], UI.MouseCell(), Find.CurrentMap, ThingPlaceMode.Near))
							{
								list2[j].Destroy();
							}
						}
						stringBuilder.AppendLine("Total market value: " + num.ToString("0.##"));
						stringBuilder.AppendLine("Total mass: " + num2.ToStringMass());
						Log.Message(stringBuilder.ToString());
					}
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("Spawning", "Trigger effecter...", allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void TriggerEffecter()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			List<EffecterDef> allDefsListForReading = DefDatabase<EffecterDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				EffecterDef localDef = allDefsListForReading[i];
				list.Add(new DebugMenuOption(localDef.defName, DebugMenuOptionMode.Tool, delegate
				{
					Effecter effecter = localDef.Spawn();
					effecter.Trigger(new TargetInfo(UI.MouseCell(), Find.CurrentMap), new TargetInfo(UI.MouseCell(), Find.CurrentMap));
					effecter.Cleanup();
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("Spawning", null, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void SpawnShuttle()
		{
			List<DebugMenuOption> options = new List<DebugMenuOption>
			{
				new DebugMenuOption("Incoming", DebugMenuOptionMode.Tool, delegate
				{
					GenPlace.TryPlaceThing(SkyfallerMaker.MakeSkyfaller(ThingDefOf.ShuttleIncoming, ThingMaker.MakeThing(ThingDefOf.Shuttle)), UI.MouseCell(), Find.CurrentMap, ThingPlaceMode.Near);
				}),
				new DebugMenuOption("Crashing", DebugMenuOptionMode.Tool, delegate
				{
					GenPlace.TryPlaceThing(SkyfallerMaker.MakeSkyfaller(ThingDefOf.ShuttleCrashing, ThingMaker.MakeThing(ThingDefOf.ShuttleCrashed)), UI.MouseCell(), Find.CurrentMap, ThingPlaceMode.Near);
				}),
				new DebugMenuOption("Stationary", DebugMenuOptionMode.Tool, delegate
				{
					GenPlace.TryPlaceThing(ThingMaker.MakeThing(ThingDefOf.Shuttle), UI.MouseCell(), Find.CurrentMap, ThingPlaceMode.Near);
				})
			};
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
		}

		[DebugAction("Spawning", null, actionType = DebugActionType.ToolWorld, allowedGameStates = AllowedGameStates.PlayingOnWorld)]
		private static void SpawnRandomCaravan()
		{
			int num = GenWorld.MouseTile();
			if (Find.WorldGrid[num].biome.impassable)
			{
				return;
			}
			List<Pawn> list = new List<Pawn>();
			int num2 = Rand.RangeInclusive(1, 10);
			for (int i = 0; i < num2; i++)
			{
				Pawn pawn = PawnGenerator.GeneratePawn(Faction.OfPlayer.def.basicMemberKind, Faction.OfPlayer);
				list.Add(pawn);
				if (!pawn.WorkTagIsDisabled(WorkTags.Violent) && Rand.Value < 0.9f)
				{
					ThingDef thingDef = DefDatabase<ThingDef>.AllDefs.Where((ThingDef def) => def.IsWeapon && def.PlayerAcquirable).RandomElementWithFallback();
					pawn.equipment.AddEquipment((ThingWithComps)ThingMaker.MakeThing(thingDef, GenStuff.RandomStuffFor(thingDef)));
				}
			}
			int num3 = Rand.RangeInclusive(-4, 10);
			for (int j = 0; j < num3; j++)
			{
				Pawn item = PawnGenerator.GeneratePawn(DefDatabase<PawnKindDef>.AllDefs.Where((PawnKindDef d) => d.RaceProps.Animal && d.RaceProps.wildness < 1f).RandomElement(), Faction.OfPlayer);
				list.Add(item);
			}
			Caravan caravan = CaravanMaker.MakeCaravan(list, Faction.OfPlayer, num, addToWorldPawnsIfNotAlready: true);
			List<Thing> list2 = ThingSetMakerDefOf.DebugCaravanInventory.root.Generate();
			for (int k = 0; k < list2.Count; k++)
			{
				Thing thing = list2[k];
				if (!(thing.GetStatValue(StatDefOf.Mass) * (float)thing.stackCount > caravan.MassCapacity - caravan.MassUsage))
				{
					CaravanInventoryUtility.GiveThing(caravan, thing);
					continue;
				}
				break;
			}
		}

		[DebugAction("Spawning", null, actionType = DebugActionType.ToolWorld, allowedGameStates = AllowedGameStates.PlayingOnWorld)]
		private static void SpawnRandomFactionBase()
		{
			if (Find.FactionManager.AllFactions.Where((Faction x) => !x.IsPlayer && !x.Hidden).TryRandomElement(out var result))
			{
				int num = GenWorld.MouseTile();
				if (!Find.WorldGrid[num].biome.impassable)
				{
					Settlement settlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
					settlement.SetFaction(result);
					settlement.Tile = num;
					settlement.Name = SettlementNameGenerator.GenerateSettlementName(settlement);
					Find.WorldObjects.Add(settlement);
				}
			}
		}

		[DebugAction("Spawning", null, actionType = DebugActionType.ToolWorld, allowedGameStates = AllowedGameStates.PlayingOnWorld)]
		private static void SpawnSite()
		{
			int tile = GenWorld.MouseTile();
			if (tile < 0 || Find.World.Impassable(tile))
			{
				Messages.Message("Impassable", MessageTypeDefOf.RejectInput, historical: false);
				return;
			}
			List<SitePartDef> parts = new List<SitePartDef>();
			Action addPart = null;
			addPart = delegate
			{
				List<DebugMenuOption> list = new List<DebugMenuOption>
				{
					new DebugMenuOption("-Done (" + parts.Count + " parts)-", DebugMenuOptionMode.Action, delegate
					{
						Site site = SiteMaker.TryMakeSite(parts, tile);
						if (site == null)
						{
							Messages.Message("Could not find any valid faction for this site.", MessageTypeDefOf.RejectInput, historical: false);
						}
						else
						{
							Find.WorldObjects.Add(site);
						}
					})
				};
				SitePartDef localPart = default(SitePartDef);
				foreach (SitePartDef allDef in DefDatabase<SitePartDef>.AllDefs)
				{
					localPart = allDef;
					list.Add(new DebugMenuOption(allDef.defName, DebugMenuOptionMode.Action, delegate
					{
						parts.Add(localPart);
						addPart();
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			};
			addPart();
		}

		[DebugAction("Spawning", null, actionType = DebugActionType.ToolWorld, allowedGameStates = AllowedGameStates.PlayingOnWorld)]
		private static void DestroySite()
		{
			int tileID = GenWorld.MouseTile();
			foreach (WorldObject item in Find.WorldObjects.ObjectsAt(tileID).ToList())
			{
				item.Destroy();
			}
		}

		[DebugAction("Spawning", null, actionType = DebugActionType.ToolWorld, allowedGameStates = AllowedGameStates.PlayingOnWorld)]
		private static void SpawnSiteWithPoints()
		{
			int tile = GenWorld.MouseTile();
			if (tile < 0 || Find.World.Impassable(tile))
			{
				Messages.Message("Impassable", MessageTypeDefOf.RejectInput, historical: false);
				return;
			}
			List<SitePartDef> parts = new List<SitePartDef>();
			Action addPart = null;
			addPart = delegate
			{
				List<DebugMenuOption> list = new List<DebugMenuOption>
				{
					new DebugMenuOption("-Done (" + parts.Count + " parts)-", DebugMenuOptionMode.Action, delegate
					{
						List<DebugMenuOption> list2 = new List<DebugMenuOption>();
						float localPoints = default(float);
						foreach (float item in DebugActionsUtility.PointsOptions(extended: true))
						{
							localPoints = item;
							list2.Add(new DebugMenuOption(item.ToString("F0"), DebugMenuOptionMode.Action, delegate
							{
								Site site = SiteMaker.TryMakeSite(parts, tile, disallowNonHostileFactions: true, null, ifHostileThenMustRemainHostile: true, localPoints);
								if (site == null)
								{
									Messages.Message("Could not find any valid faction for this site.", MessageTypeDefOf.RejectInput, historical: false);
								}
								else
								{
									Find.WorldObjects.Add(site);
								}
							}));
						}
						Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
					})
				};
				SitePartDef localPart = default(SitePartDef);
				foreach (SitePartDef allDef in DefDatabase<SitePartDef>.AllDefs)
				{
					localPart = allDef;
					list.Add(new DebugMenuOption(allDef.defName, DebugMenuOptionMode.Action, delegate
					{
						parts.Add(localPart);
						addPart();
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			};
			addPart();
		}

		[DebugAction("Spawning", null, actionType = DebugActionType.ToolWorld, allowedGameStates = AllowedGameStates.PlayingOnWorld)]
		private static void SpawnWorldObject()
		{
			int tile = GenWorld.MouseTile();
			if (tile < 0 || Find.World.Impassable(tile))
			{
				Messages.Message("Impassable", MessageTypeDefOf.RejectInput, historical: false);
				return;
			}
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (WorldObjectDef allDef in DefDatabase<WorldObjectDef>.AllDefs)
			{
				WorldObjectDef localDef = allDef;
				Action action = null;
				action = delegate
				{
					WorldObject worldObject = WorldObjectMaker.MakeWorldObject(localDef);
					worldObject.Tile = tile;
					Find.WorldObjects.Add(worldObject);
				};
				list.Add(new DebugMenuOption(localDef.defName, DebugMenuOptionMode.Action, action));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("General", "Change camera config...", allowedGameStates = AllowedGameStates.PlayingOnWorld)]
		private static void ChangeCameraConfigWorld()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (Type item in typeof(WorldCameraConfig).AllSubclasses())
			{
				Type localType = item;
				string text = localType.Name;
				if (text.StartsWith("WorldCameraConfig_"))
				{
					text = text.Substring("WorldCameraConfig_".Length);
				}
				list.Add(new DebugMenuOption(text, DebugMenuOptionMode.Action, delegate
				{
					Find.WorldCameraDriver.config = (WorldCameraConfig)Activator.CreateInstance(localType);
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}
	}
}
