using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Unity.Collections;
using Unity.Profiling;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class GravshipUtility
	{
		private static List<ThingDef> allShipComponents;

		public static bool generatingGravship = false;

		public static readonly Color ConnectedSubstructureColor = ColorLibrary.Cyan;

		public static readonly Color DisconnectedSubstructureColor = ColorLibrary.Red;

		private const float MinFuelCost = 50f;

		private static readonly ProfilerMarker PlayerHasGravEngineMarker = new ProfilerMarker("PlayerHasGravEngine()");

		private static int lastCachedEngineTick = -1;

		private static int lastCachedEngineMapID = -1;

		private static Building_GravEngine cachedGravEngine;

		private static readonly List<PlanetLayerConnection> connections = new List<PlanetLayerConnection>();

		private static PlanetTile cachedOrigin;

		private static PlanetTile cachedDest;

		private static float cachedCost;

		private static float cachedLayerCost;

		private static PlanetLayer cachedOriginLayer;

		private static PlanetLayer cachedDestLayer;

		private static bool cachedResult;

		private static int cachedDistance;

		private static readonly SimpleCurve LaunchCooldownFromQualityCurve = new SimpleCurve
		{
			new CurvePoint(0f, 600000f),
			new CurvePoint(0.25f, 300000f),
			new CurvePoint(0.5f, 120000f),
			new CurvePoint(0.8f, 30000f),
			new CurvePoint(1f, 20000f)
		};

		private static readonly SimpleCurve NegativeLandingOutcomeFromQualityCurve = new SimpleCurve
		{
			new CurvePoint(0f, 1f),
			new CurvePoint(0.5f, 0.25f),
			new CurvePoint(1f, 0.02f)
		};

		public static bool ShowConnectedSubstructure
		{
			get
			{
				if (!ModsConfig.OdysseyActive)
				{
					return false;
				}
				Designator selectedDesignator = Find.DesignatorManager.SelectedDesignator;
				if (selectedDesignator is Designator_Install)
				{
					return false;
				}
				if (selectedDesignator is Designator_Place designator_Place)
				{
					if (designator_Place.PlacingDef is TerrainDef { IsSubstructure: not false })
					{
						return true;
					}
					if (designator_Place.PlacingDef is ThingDef thingDef && thingDef.GetCompProperties<CompProperties_SubstructureFootprint>() != null)
					{
						return true;
					}
				}
				foreach (object selectedObject in Find.Selector.SelectedObjects)
				{
					if (selectedObject is Thing thing && thing.TryGetComp(out CompSubstructureFootprint comp) && comp.Valid && comp.DisplaySubstructureOverlay)
					{
						return true;
					}
				}
				return false;
			}
		}

		public static bool PlayerHasGravEngine()
		{
			if (!ModsConfig.OdysseyActive)
			{
				return false;
			}
			if (Find.CurrentGravship != null)
			{
				return true;
			}
			using (PlayerHasGravEngineMarker.Auto())
			{
				foreach (Map map in Find.Maps)
				{
					if (PlayerHasGravEngine(map))
					{
						return true;
					}
				}
				foreach (WorldObject allWorldObject in Find.WorldObjects.AllWorldObjects)
				{
					if (allWorldObject.Faction == null || !allWorldObject.Faction.IsPlayer || allWorldObject is MapParent || !(allWorldObject is IThingHolder holder))
					{
						continue;
					}
					foreach (Thing item in ThingOwnerUtility.GetAllThingsRecursively(holder))
					{
						if (item.GetInnerIfMinified().def == ThingDefOf.GravEngine)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		[Obsolete]
		public static Thing GetPlayerGravEngine(Map map)
		{
			return GetPlayerGravEngine_NewTemp(map);
		}

		public static Building_GravEngine GetPlayerGravEngine_NewTemp(Map map)
		{
			if (!ModsConfig.OdysseyActive)
			{
				return null;
			}
			if (Find.TickManager.TicksGame == lastCachedEngineTick && lastCachedEngineMapID == map.uniqueID)
			{
				return cachedGravEngine;
			}
			ThingDef def = ThingDefOf.GravEngine;
			Building_GravEngine building_GravEngine = map.listerThings.ThingsOfDef(def).FirstOrDefault() as Building_GravEngine;
			if (building_GravEngine == null)
			{
				building_GravEngine = map.listerThings.ThingsOfDef(def.minifiedDef).FirstOrDefault().GetInnerIfMinified() as Building_GravEngine;
			}
			if (building_GravEngine == null)
			{
				List<Thing> list = new List<Thing>();
				ThingOwnerUtility.GetAllThingsRecursively(map, ThingRequest.ForDef(def.minifiedDef), list, allowUnreal: true, null, alsoGetSpawnedThings: false);
				Thing thing = list.Find((Thing t) => t.GetInnerIfMinified()?.def == def);
				if (thing != null)
				{
					building_GravEngine = thing.GetInnerIfMinified() as Building_GravEngine;
				}
			}
			lastCachedEngineMapID = map.uniqueID;
			lastCachedEngineTick = Find.TickManager.TicksGame;
			cachedGravEngine = building_GravEngine;
			return building_GravEngine;
		}

		public static bool PlayerHasGravEngine(Map map)
		{
			if (!ModsConfig.OdysseyActive)
			{
				return false;
			}
			return GetPlayerGravEngine_NewTemp(map) != null;
		}

		public static bool TryGetNameOfGravshipOnMap(Map map, out string name)
		{
			name = null;
			if (!ModsConfig.OdysseyActive || map == null || !PlayerHasGravEngine(map))
			{
				return false;
			}
			Building_GravEngine playerGravEngine_NewTemp = GetPlayerGravEngine_NewTemp(map);
			if (playerGravEngine_NewTemp == null)
			{
				return false;
			}
			if (playerGravEngine_NewTemp.nameHidden)
			{
				return false;
			}
			name = playerGravEngine_NewTemp.RenamableLabel;
			return true;
		}

		public static void GetConnectedSubstructure(Building_GravEngine engine, HashSet<IntVec3> cells, int maxCells, bool requireInsideFootprint = true)
		{
			cells.Clear();
			if (!ModsConfig.OdysseyActive)
			{
				return;
			}
			if (!engine.Spawned)
			{
				Log.Error("Tried to get connected substructure for an unspawned engine.");
				return;
			}
			List<Thing> footprintMakers = engine.Map.listerThings.ThingsInGroup(ThingRequestGroup.SubstructureFootprint);
			engine.Map.floodFiller.FloodFill(engine.Position, delegate(IntVec3 x)
			{
				if (x.InBounds(engine.Map))
				{
					TerrainDef terrainDef = engine.Map.terrainGrid.FoundationAt(x);
					if (terrainDef != null && terrainDef.IsSubstructure)
					{
						if (requireInsideFootprint)
						{
							return InsideFootprint(x, engine.Map, footprintMakers);
						}
						return true;
					}
				}
				return false;
			}, delegate(IntVec3 x)
			{
				cells.Add(x);
				return false;
			}, maxCells);
		}

		public static bool InsideFootprint(IntVec3 loc, Map map, List<Thing> footprintMakers = null, Thing thingToExclude = null)
		{
			if (!ModsConfig.OdysseyActive)
			{
				return false;
			}
			if (footprintMakers == null)
			{
				footprintMakers = map.listerThings.ThingsInGroup(ThingRequestGroup.SubstructureFootprint);
			}
			foreach (Thing footprintMaker in footprintMakers)
			{
				if (footprintMaker.Spawned && footprintMaker.TryGetComp(out CompSubstructureFootprint comp) && comp.Valid && loc.InHorDistOf(footprintMaker.Position, comp.Props.radius) && (thingToExclude == null || footprintMaker != thingToExclude))
				{
					return true;
				}
			}
			return false;
		}

		public static void PreLaunchConfirmation(Building_GravEngine engine, Action launchAction)
		{
			if (!ModsConfig.OdysseyActive)
			{
				return;
			}
			TaggedString text = "ConfirmGravEngineLaunch".Translate();
			List<Pawn> list = new List<Pawn>();
			List<Pawn> list2 = new List<Pawn>();
			List<Thing> list3 = new List<Thing>();
			List<Pawn> list4 = new List<Pawn>();
			foreach (Pawn allPawn in engine.Map.mapPawns.AllPawns)
			{
				if (allPawn.Faction == Faction.OfPlayer && (!engine.ValidSubstructureAt(allPawn.PositionHeld) || allPawn.ParentHolder is ActiveTransporterInfo))
				{
					if (allPawn.RaceProps.Humanlike)
					{
						list.Add(allPawn);
					}
					else
					{
						list2.Add(allPawn);
					}
				}
				if (ConsideredKidnapped(engine, allPawn))
				{
					list4.Add(allPawn);
				}
			}
			foreach (Pawn item in engine.Map.mapPawns.PrisonersOfColony)
			{
				if (!engine.ValidSubstructureAt(item.PositionHeld) && !list.Contains(item))
				{
					list.Add(item);
				}
			}
			if (list.Count > 0)
			{
				text += "\n\n" + ("GravEngineWarning".Translate() + ": ").Colorize(ColorLibrary.RedReadable) + "PeopleWillBeLeftBehind".Translate().Resolve() + ":\n" + list.Select((Pawn p) => p.NameFullColored.Resolve()).ToLineList("  - ", capitalizeItems: true);
			}
			if (list2.Count > 0)
			{
				text += "\n\n" + ("GravEngineWarning".Translate() + ": ").Colorize(ColorLibrary.RedReadable) + "AnimalsWillBeLeftBehind".Translate().Resolve() + ":\n" + list2.Select((Pawn p) => p.NameFullColored.Resolve()).ToLineList("  - ", capitalizeItems: true);
			}
			if (list4.Count > 0)
			{
				text += "\n\n" + ("GravEngineWarning".Translate() + ": ").Colorize(ColorLibrary.RedReadable) + "PawnsKidnappedOnGravship".Translate().Resolve() + ":\n" + list4.Select((Pawn p) => p.NameFullColored.Resolve()).ToLineList("  - ", capitalizeItems: true);
			}
			foreach (IntVec3 item2 in engine.AllConnectedSubstructure)
			{
				Building edifice = item2.GetEdifice(engine.Map);
				if (edifice != null && edifice.def.Size != IntVec2.One && !engine.OnValidSubstructure(edifice) && !list3.Contains(edifice))
				{
					list3.Add(edifice);
				}
			}
			if (list3.Count > 0)
			{
				text += "\n\n" + ("GravEngineWarning".Translate() + ": ").Colorize(ColorLibrary.RedReadable) + "BuildingsWillBeLeftBehind".Translate().Resolve() + ":\n" + list3.Select((Thing b) => b.LabelShort).ToLineList("  - ", capitalizeItems: true);
			}
			HashSet<string> hashSet = new HashSet<string>();
			foreach (IntVec3 allCell in engine.Map.AllCells)
			{
				TerrainDef terrainDef = engine.Map.terrainGrid.FoundationAt(allCell);
				if (terrainDef != null && terrainDef.IsSubstructure)
				{
					if (!InsideFootprint(allCell, engine.Map))
					{
						hashSet.Add("SubstructureOutsideFootprint".Translate());
					}
					if (!engine.ValidSubstructureAt(allCell))
					{
						hashSet.Add("DisconnectedSubstructure".Translate());
					}
					RoofDef roofDef = engine.Map.roofGrid.RoofAt(allCell);
					if (roofDef != null && roofDef.isThickRoof)
					{
						hashSet.Add("SubstructureUnderRockRoof".Translate());
					}
				}
			}
			if (hashSet.Count > 0)
			{
				text += "\n\n" + ("GravEngineWarning".Translate() + ": ").Colorize(ColorLibrary.RedReadable) + "SubstructureWillBeLeftBehind".Translate() + ":\n" + hashSet.ToLineList("  - ", capitalizeItems: true);
			}
			Find.WindowStack.Add(new Dialog_MessageBox(text, null, launchAction, "Cancel".Translate(), delegate
			{
			}, null, buttonADestructive: true, launchAction));
		}

		public static void TravelTo(Gravship gravship, PlanetTile oldTile, PlanetTile newTile)
		{
			if (ModsConfig.OdysseyActive)
			{
				gravship.SetFaction(gravship.Engine.Faction);
				if (oldTile.Layer != newTile.Layer)
				{
					oldTile = newTile.Layer.GetClosestTile_NewTemp(oldTile);
				}
				gravship.Tile = oldTile;
				gravship.destinationTile = newTile;
				Find.WorldObjects.Add(gravship);
				CameraJumper.TryJump(gravship);
			}
		}

		public static Gravship GenerateGravship(Building_GravEngine engine)
		{
			if (!ModsConfig.OdysseyActive)
			{
				return null;
			}
			generatingGravship = true;
			Map map = engine.Map;
			IntVec3[] array = engine.ValidSubstructure.ToArray();
			Gravship obj = new Gravship(engine);
			obj = WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Gravship, obj) as Gravship;
			Current.Game.Gravship = obj;
			List<Thing> list = obj.Things.ToList();
			list.SortByDescending(ThingSpawnPriority);
			foreach (Thing item in list)
			{
				item.PreSwapMap();
			}
			foreach (Thing item2 in list)
			{
				if (item2.Spawned)
				{
					item2.DeSpawn(DestroyMode.WillReplace);
				}
			}
			foreach (Pawn pawn in obj.Pawns)
			{
				pawn.PreSwapMap();
				if (pawn.Spawned)
				{
					pawn.DeSpawn(DestroyMode.WillReplace);
				}
			}
			ulong dirtyFlags = (ulong)MapMeshFlagDefOf.Terrain | (ulong)MapMeshFlagDefOf.Roofs | (ulong)MapMeshFlagDefOf.Snow | (ulong)MapMeshFlagDefOf.Sand | (ulong)MapMeshFlagDefOf.Gas;
			for (int i = 0; i < array.Length; i++)
			{
				IntVec3 intVec = array[i];
				int index = map.cellIndices.CellToIndex(intVec);
				map.terrainGrid.RemoveGravshipTerrainUnsafe(intVec, index);
				map.roofGrid.RemoveRoofUnsafe(index);
				map.areaManager.BuildRoof[index] = false;
				map.areaManager.NoRoof[index] = false;
				map.glowGrid.DirtyCell(intVec);
				NativeArray<float> depthGrid_Unsafe = map.snowGrid.DepthGrid_Unsafe;
				depthGrid_Unsafe[index] = 0f;
				NativeArray<float> depthGrid_Unsafe2 = map.sandGrid.DepthGrid_Unsafe;
				depthGrid_Unsafe2[index] = 0f;
				map.gasGrid.ClearCellUnsafe(intVec);
				foreach (Designation item3 in map.designationManager.AllDesignationsAt(intVec))
				{
					map.designationManager.RemoveDesignation(item3);
				}
				IntVec2 cell2D = intVec.ToIntVec2;
				foreach (FleckSystem system in map.flecks.Systems)
				{
					system.RemoveAllFlecks((IFleck fleck) => fleck.GetPosition().ToIntVec2() == cell2D);
				}
				map.mapDrawer.MapMeshDirty(intVec, dirtyFlags, regenAdjacentCells: true, regenAdjacentSections: false);
				map.pathing.RecalculatePerceivedPathCostAt(intVec);
			}
			map.roofGrid.Drawer.SetDirty();
			generatingGravship = false;
			return obj;
		}

		public static void AbandonMap(Map map)
		{
			if (!ModLister.CheckOdyssey("Gravship"))
			{
				return;
			}
			List<Pawn> list = map.mapPawns.AllPawnsSpawned.ToList();
			PawnDiedOrDownedThoughtsUtility.TryGiveThoughts(list, PawnDiedOrDownedThoughtsKind.Lost);
			for (int num = list.Count - 1; num >= 0; num--)
			{
				list[num].Notify_LeftBehind();
				if (list[num].Spawned)
				{
					list[num].DeSpawn();
				}
				if (!list[num].Destroyed)
				{
					list[num].DestroyOrPassToWorld();
				}
			}
			map.Parent.Abandon(wasGravshipLaunch: true);
		}

		public static void ArriveExistingMap(Gravship gravship)
		{
			if (!ModsConfig.OdysseyActive)
			{
				return;
			}
			Map map = Find.WorldObjects.MapParentAt(gravship.destinationTile)?.Map;
			if (map == null)
			{
				Log.Error($"Tried to arrive gravship {gravship.Label} at tile {gravship.destinationTile} but no map exists there.");
				return;
			}
			Find.GravshipController.landingMap = map;
			GravshipLandingMarker gravshipLandingMarker = ThingMaker.MakeThing(ThingDefOf.GravshipLandingMarker) as GravshipLandingMarker;
			gravshipLandingMarker.gravship = gravship;
			gravshipLandingMarker.CacheCells();
			Find.GravshipController.Notify_LandingAreaConfirmationStarted(gravshipLandingMarker);
			Find.WorldObjects.Remove(gravship);
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				if (Prefs.PauseOnLoad)
				{
					Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
				}
				CameraJumper.TryJump(map.Center, map, CameraJumper.MovementMode.Cut);
				Find.DesignatorManager.Select(Find.GravshipController.MoveDesignator());
			});
		}

		public static void ArriveNewMap(Gravship gravship)
		{
			if (!ModsConfig.OdysseyActive)
			{
				return;
			}
			PlanetTile destinationTile = gravship.destinationTile;
			MapParent mapParent = Find.WorldObjects.MapParentAt(destinationTile);
			if (mapParent == null)
			{
				if (destinationTile.LayerDef.DefaultWorldObject == destinationTile.LayerDef.SettlementWorldObjectDef)
				{
					mapParent = SettleUtility.AddNewHome(destinationTile, Faction.OfPlayer);
				}
				else
				{
					mapParent = (MapParent)WorldObjectMaker.MakeWorldObject(destinationTile.LayerDef.DefaultWorldObject);
					mapParent.Tile = destinationTile;
					Find.WorldObjects.Add(mapParent);
				}
				if (mapParent.def.canHaveFaction && mapParent.Faction == null)
				{
					mapParent.SetFaction(Faction.OfPlayer);
				}
				mapParent.Tile = destinationTile;
				if (mapParent is Settlement settlement)
				{
					settlement.Name = "GravshipLandingSite".Translate(gravship.Label).CapitalizeFirst();
					settlement.namedByPlayer = true;
				}
			}
			else if (mapParent.def.canHaveFaction && mapParent.Faction == null)
			{
				mapParent.SetFaction(Faction.OfPlayer);
			}
			IntVec3 size = Find.World.info.initialMapSize;
			if (mapParent.def.overrideMapSize.HasValue)
			{
				size = mapParent.def.overrideMapSize.Value;
			}
			else if (mapParent.def.canResizeToGravship)
			{
				size.x = Mathf.RoundToInt(Mathf.Ceil((float)gravship.Bounds.maxX / 100f) * 100f);
				size.z = Mathf.RoundToInt(Mathf.Ceil((float)gravship.Bounds.maxZ / 100f) * 100f);
				size.y = 1;
			}
			if (mapParent is Site { PreferredMapSize: var preferredMapSize } site)
			{
				size = new IntVec3(Mathf.Max(size.x, preferredMapSize.x), Mathf.Max(size.y, preferredMapSize.y), Mathf.Max(size.z, preferredMapSize.z));
				if (site.MainSitePartDef.minMapSize.HasValue)
				{
					IntVec3 value = site.MainSitePartDef.minMapSize.Value;
					size = new IntVec3(Mathf.Max(size.x, value.x), Mathf.Max(size.y, value.y), Mathf.Max(size.z, value.z));
				}
			}
			Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(destinationTile, size, mapParent.def, GetGenSteps(gravship));
			if ((orGenerateMap.Parent is Settlement || (orGenerateMap.Parent is Site site2 && site2.parts.All((SitePart part) => part.def.considerEnteringAsAttack))) && orGenerateMap.Parent.Faction != null && orGenerateMap.Parent.Faction != Faction.OfPlayer)
			{
				Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
				TaggedString letterLabel = "LetterLabelGravshipEnteredEnemyBase".Translate();
				TaggedString letterText = "LetterGravshipEnteredEnemyBase".Translate(orGenerateMap.Parent.Label.ApplyTag(TagType.Settlement, orGenerateMap.Parent.Faction.GetUniqueLoadID())).CapitalizeFirst();
				SettlementUtility.AffectRelationsOnAttacked(orGenerateMap.Parent, ref letterText);
				PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(orGenerateMap.mapPawns.AllPawns.Where((Pawn p) => !gravship.ContainsPawn(p)), ref letterLabel, ref letterText, "LetterRelatedPawnsSettlement".Translate(Faction.OfPlayer.def.pawnsPlural), informEvenIfSeenBefore: true);
				Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.NeutralEvent);
				Find.GoodwillSituationManager.RecalculateAll(canSendHostilityChangedLetter: true);
			}
			if (gravship.Spawned)
			{
				Find.WorldObjects.Remove(gravship);
			}
			TaleRecorder.RecordTale(TaleDefOf.TileSettled).customLabel = "NewSettlement".Translate();
			if (Find.IdeoManager != null)
			{
				Find.IdeoManager.lastResettledTick = GenTicks.TicksGame;
			}
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				if (Prefs.PauseOnLoad)
				{
					Find.TickManager.DoSingleTick();
					Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
				}
				CameraJumper.TryJump(gravship.Engine, CameraJumper.MovementMode.Cut);
				Find.CameraDriver.shaker.DoShake(0.1f, 180);
			});
		}

		public static int ThingSpawnPriority(Thing a)
		{
			if (!ModLister.CheckOdyssey("Gravship thing spawn priority"))
			{
				return 0;
			}
			if (a is Building_Bed)
			{
				return 20;
			}
			BuildingProperties building = a.def.building;
			if (building != null && building.isAttachment)
			{
				return 10;
			}
			if (a.def.category == ThingCategory.Item)
			{
				return 20;
			}
			return a.def.gravshipSpawnPriority;
		}

		public static bool TryGetPathFuelCost(PlanetTile from, PlanetTile to, out float cost, out int distance, float fuelPerTile = 10f, float fuelFactor = 1f)
		{
			cost = 0f;
			distance = 0;
			if (!ModLister.CheckOdyssey("Gravship path fuel cost"))
			{
				return false;
			}
			if (cachedOrigin == from && cachedDest == to)
			{
				cost = cachedCost;
				distance = cachedDistance;
				return cachedResult;
			}
			cachedOrigin = from;
			cachedDest = to;
			cachedCost = (cost = 0f);
			cachedDistance = (distance = 0);
			if (from.Layer != to.Layer)
			{
				if (cachedOriginLayer == from.Layer && cachedDestLayer == to.Layer)
				{
					cost += cachedLayerCost;
				}
				else
				{
					if (!from.Layer.TryGetPath(to.Layer, connections, out var cost2))
					{
						return cachedResult = false;
					}
					cachedOriginLayer = to.Layer;
					cachedDestLayer = from.Layer;
					cost += (cachedLayerCost = cost2);
					connections.Clear();
				}
				from = to.Layer.GetClosestTile_NewTemp(from);
			}
			cachedDistance = (distance = Find.WorldGrid.TraversalDistanceBetween(from, to));
			cost += (float)distance * to.LayerDef.rangeDistanceFactor * (fuelPerTile * fuelFactor);
			cachedCost = (cost = Mathf.Max(cost, 50f));
			return cachedResult = true;
		}

		public static int MaxDistForFuel(float fuel, PlanetLayer fromLayer, PlanetLayer toLayer, float fuelPerTile = 10f, float fuelFactor = 1f)
		{
			if (fromLayer != toLayer)
			{
				if (cachedOriginLayer == fromLayer && cachedDestLayer == toLayer)
				{
					fuel -= cachedLayerCost;
				}
				else
				{
					if (!fromLayer.TryGetPath(toLayer, connections, out var cost))
					{
						return 0;
					}
					cachedOriginLayer = toLayer;
					cachedDestLayer = fromLayer;
					fuel -= (cachedLayerCost = cost);
					connections.Clear();
				}
			}
			return Mathf.FloorToInt(fuel / (toLayer.Def.rangeDistanceFactor * (fuelPerTile * fuelFactor)));
		}

		private static IEnumerable<GenStepWithParams> GetGenSteps(Gravship gravship)
		{
			yield return new GenStepWithParams
			{
				def = GenStepDefOf.ReserveGravshipArea,
				parms = new GenStepParams
				{
					gravship = gravship
				}
			};
			yield return new GenStepWithParams
			{
				def = GenStepDefOf.GravshipMarker,
				parms = new GenStepParams
				{
					gravship = gravship
				}
			};
		}

		public static void SettleTile(Map map)
		{
			if (ModLister.CheckOdyssey("Gravship settle tile"))
			{
				MapParent parent = map.Parent;
				Settlement settlement = SettleUtility.AddNewHome(map.Tile, Faction.OfPlayer);
				map.info.parent = settlement;
				if (parent != null)
				{
					settlement.questTags = parent.questTags;
					parent.Notify_MyMapSettled(map);
					parent.Destroy();
				}
				settlement.Notify_MyMapSettled(map);
				if (parent is Settlement mapParent)
				{
					TaggedString letterText = "";
					SettlementUtility.AffectRelationsOnAttacked(mapParent, ref letterText);
				}
			}
		}

		public static bool ConsideredKidnapped(Building_GravEngine engine, Pawn pawn)
		{
			if (!ModsConfig.OdysseyActive)
			{
				return false;
			}
			if (!engine.ValidSubstructureAt(pawn.PositionHeld))
			{
				return false;
			}
			if (pawn.Faction == Faction.OfPlayer || pawn.IsPlayerControlled || pawn.IsPrisonerOfColony || pawn.IsSlaveOfColony)
			{
				return false;
			}
			if (pawn.Faction.HostileTo(Faction.OfPlayer) || pawn.Faction == null)
			{
				return false;
			}
			return true;
		}

		public static float LaunchCooldownFromQuality(float quality)
		{
			return LaunchCooldownFromQualityCurve.Evaluate(quality);
		}

		public static float NegativeLandingOutcomeFromQuality(float quality)
		{
			return NegativeLandingOutcomeFromQualityCurve.Evaluate(quality);
		}

		public static bool IsOnboardGravship(IntVec3 cell, Building_GravEngine engine, Pawn pawn = null, bool desperate = false)
		{
			return IsOnboardGravship_NewTemp(cell, engine, pawn, desperate);
		}

		public static bool IsOnboardGravship_NewTemp(IntVec3 cell, Building_GravEngine engine, Pawn pawn = null, bool desperate = false, bool respectAllowedAreas = true)
		{
			if (!ModsConfig.OdysseyActive)
			{
				return false;
			}
			if (respectAllowedAreas && pawn != null && !cell.InAllowedArea(pawn))
			{
				return false;
			}
			if (!desperate && cell.GetRoom(engine.Map).PsychologicallyOutdoors)
			{
				return false;
			}
			return engine.ValidSubstructure.Contains(cell);
		}

		public static bool TryFindSpotOnGravship(Pawn pawn, Building_GravEngine engine, out IntVec3 spot)
		{
			spot = IntVec3.Invalid;
			bool desperate = false;
			if (!ModsConfig.OdysseyActive)
			{
				return false;
			}
			List<IntVec3> tmpWorkingCellList = new List<IntVec3>();
			TraverseParms traverseParms = TraverseParms.For(TraverseMode.PassDoors);
			Region region = ClosestShipRegion(pawn.Position, pawn.MapHeld, pawn, traverseParms);
			if (region == null)
			{
				desperate = true;
				region = ClosestShipRegion(pawn.Position, pawn.MapHeld, pawn, traverseParms, desperate: true);
			}
			if (region != null && TryGetAllowedCellInRegion(region, pawn, out spot, desperate))
			{
				return true;
			}
			return false;
			Region ClosestShipRegion(IntVec3 root, Map map, Pawn pawn2, TraverseParms tp, bool desperate2 = false)
			{
				RegionType regionType = RegionType.Set_Passable;
				Region region2 = root.GetRegion(map, regionType);
				if (region2 == null)
				{
					return null;
				}
				RegionEntryPredicate entryCondition = (Region _, Region r) => r.Allows(tp, isDestination: false);
				Region foundReg = null;
				RegionProcessor regionProcessor = delegate(Region r)
				{
					if (r.IsDoorway)
					{
						return false;
					}
					if (!TryGetAllowedCellInRegion(r, pawn2, out var _, desperate2))
					{
						return false;
					}
					foundReg = r;
					return true;
				};
				RegionTraverser.BreadthFirstTraverse(region2, entryCondition, regionProcessor, 9999, regionType);
				return foundReg;
			}
			bool TryGetAllowedCellInRegion(Region region2, Pawn pawn2, out IntVec3 cell, bool desperate2 = false)
			{
				cell = IntVec3.Invalid;
				for (int i = 0; i < 10; i++)
				{
					IntVec3 randomCell = region2.RandomCell;
					if (IsOnboardGravship_NewTemp(randomCell, engine, pawn2, desperate2, respectAllowedAreas: false))
					{
						cell = randomCell;
						return true;
					}
				}
				foreach (IntVec3 item in region2.Cells.InRandomOrder(tmpWorkingCellList))
				{
					if (IsOnboardGravship_NewTemp(item, engine, pawn2, desperate2, respectAllowedAreas: false))
					{
						cell = item;
						return true;
					}
				}
				return false;
			}
		}

		public static bool TryFindSpotOffGravship(Pawn pawn, Building_GravEngine engine, out IntVec3 spot)
		{
			spot = IntVec3.Invalid;
			if (!ModsConfig.OdysseyActive)
			{
				return false;
			}
			List<IntVec3> tmpWorkingCellList = new List<IntVec3>();
			Region region = ClosestNonShipRegion(pawn.Position, pawn.MapHeld, pawn, TraverseParms.For(pawn));
			if (region != null && TryGetAllowedCellInRegion(region, pawn, out spot))
			{
				return true;
			}
			return false;
			Region ClosestNonShipRegion(IntVec3 root, Map map, Pawn pawn2, TraverseParms traverseParms)
			{
				RegionType regionType = RegionType.Set_Passable;
				Region region2 = root.GetRegion(map, regionType);
				if (region2 == null)
				{
					return null;
				}
				RegionEntryPredicate entryCondition = (Region _, Region r) => r.Allows(traverseParms, isDestination: false);
				Region foundReg = null;
				RegionProcessor regionProcessor = delegate(Region r)
				{
					if (r.IsDoorway)
					{
						return false;
					}
					if (!TryGetAllowedCellInRegion(r, pawn2, out var _))
					{
						return false;
					}
					foundReg = r;
					return true;
				};
				RegionTraverser.BreadthFirstTraverse(region2, entryCondition, regionProcessor, 9999, regionType);
				return foundReg;
			}
			bool TryGetAllowedCellInRegion(Region region2, Pawn pawn2, out IntVec3 cell)
			{
				cell = IntVec3.Invalid;
				for (int i = 0; i < 10; i++)
				{
					IntVec3 randomCell = region2.RandomCell;
					if (!IsOnboardGravship_NewTemp(randomCell, engine, pawn2, desperate: true, respectAllowedAreas: false))
					{
						cell = randomCell;
						return true;
					}
				}
				foreach (IntVec3 item in region2.Cells.InRandomOrder(tmpWorkingCellList))
				{
					if (!IsOnboardGravship_NewTemp(item, engine, pawn2, desperate: true, respectAllowedAreas: false))
					{
						cell = item;
						return true;
					}
				}
				return false;
			}
		}

		public static void UpdateBillDestinations(Map map)
		{
			foreach (Bill item in BillUtility.MapBills(map))
			{
				foreach (StorageGroup item2 in map.storageGroups.StorageGroupsForReading)
				{
					if (!(item.GetSlotGroup() is StorageGroup storageGroup) || !(storageGroup.RenamableLabel == item2.RenamableLabel))
					{
						continue;
					}
					item.SetStoreMode(BillStoreModeDefOf.SpecificStockpile, item2);
					goto IL_012a;
				}
				foreach (Zone allZone in map.zoneManager.AllZones)
				{
					if (!(allZone is Zone_Stockpile zone_Stockpile) || !(item.GetSlotGroup() is SlotGroup { parent: Zone parent }) || !(parent.RenamableLabel == allZone.RenamableLabel))
					{
						continue;
					}
					item.SetStoreMode(BillStoreModeDefOf.SpecificStockpile, zone_Stockpile.GetSlotGroup());
					goto IL_012a;
				}
				if (item.GetStoreMode() == BillStoreModeDefOf.SpecificStockpile)
				{
					item.SetStoreMode(BillStoreModeDefOf.DropOnFloor);
				}
				IL_012a:;
			}
		}
	}
}
