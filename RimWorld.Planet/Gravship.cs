using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class Gravship : WorldObject, IIncidentTarget, ILoadReferenceable
	{
		private Rot4 rotation;

		private CellRect bounds;

		private Building_GravEngine engine;

		private Building pilotConsole;

		private Dictionary<Thing, PositionData> things;

		private Dictionary<Pawn, PositionData> pawns;

		private Dictionary<Thing, bool> powerOn;

		private Dictionary<Thing, Thing> connectParents;

		private Dictionary<IntVec3, RoofDef> roofs;

		private Dictionary<IntVec3, TerrainDef> foundations;

		private Dictionary<IntVec3, TerrainDef> terrains;

		private Dictionary<IntVec3, ColorDef> terrainColors;

		private Dictionary<IntVec3, Designation> terrainDesignations;

		private Dictionary<IntVec3, uint> gases;

		private List<CellRect> occupiedRects;

		private List<RoomTemperatureVacuum> roomTemperatures;

		private List<IntVec3> gravFieldExtenderPositions;

		private StoryState storyState;

		public MoveableAreas areas;

		public List<AutoSlaughterConfig> autoSlaughterConfigs;

		private Dictionary<Thing, PositionData> thrusters;

		private Dictionary<Thing, PositionData> exteriorDoors;

		[Unsaved(false)]
		public IntVec3 originalPosition;

		public Vector3 engineToCenter;

		public IntVec3 launchDirection;

		[Unsaved(false)]
		public Capture capture;

		[Unsaved(false)]
		public List<LayerSubMesh> bakedIndoorMasks = new List<LayerSubMesh>();

		public PlanetTile destinationTile = PlanetTile.Invalid;

		private PlanetTile initialTile = PlanetTile.Invalid;

		private float traveledPct;

		private bool arrived;

		private Material cachedMat;

		private const float TravelSpeed = 0.00025f;

		private List<Thing> tmpPoweredThings = new List<Thing>();

		private List<bool> tmpPoweredThingsOn = new List<bool>();

		private List<Thing> tmpConnectedThings = new List<Thing>();

		private List<Thing> tmpConnectedParents = new List<Thing>();

		private List<Thing> tmpThrusters = new List<Thing>();

		private List<PositionData> tmpThrusterPositionData = new List<PositionData>();

		private List<Thing> tmpExteriorDoors = new List<Thing>();

		private List<PositionData> tmpExteriorDoorPositionData = new List<PositionData>();

		private static readonly Dictionary<Faction, int> tmpFactionMembersKidnappedCount = new Dictionary<Faction, int>();

		private Dictionary<Thing, PositionData.Data> tmpThingPlacements;

		private Rot4 tmpThingsRot;

		private Dictionary<Pawn, PositionData.Data> tmpPawns;

		private Rot4 tmpPawnsRot;

		private Dictionary<Thing, PositionData.Data> tmpThrusterPlacements;

		private Rot4 tmpThrusterRot;

		private Dictionary<Thing, PositionData.Data> tmpExteriorDoorPlacements;

		private Rot4 tmpExteriorDoorRot;

		private Dictionary<IntVec3, TerrainDef> tmpFoundations;

		private Rot4 tmpFoundationRot;

		private Dictionary<IntVec3, TerrainDef> tmpTerrains;

		private Rot4 tmpTerrainRot;

		private Vector3 Start => Find.WorldGrid.GetTileCenter(initialTile);

		private Vector3 End => Find.WorldGrid.GetTileCenter(destinationTile);

		public override Vector3 DrawPos => Vector3.Slerp(Start, End, traveledPct);

		public IEnumerable<Pawn> Pawns => pawns.Keys;

		public IEnumerable<Thing> Things => things.Keys;

		public override string Label
		{
			get
			{
				if (!engine.nameHidden)
				{
					return engine.RenamableLabel;
				}
				return base.Label;
			}
		}

		public override Material Material => cachedMat ?? (cachedMat = MaterialPool.MatFrom(def.texture, ShaderDatabase.WorldOverlayTransparentLit, base.Faction.Color, 3550));

		public override Color ExpandingIconColor => base.Faction?.Color ?? Color.white;

		public Rot4 Rotation
		{
			get
			{
				return rotation;
			}
			set
			{
				rotation = value;
			}
		}

		private float TraveledPctStepPerTick
		{
			get
			{
				Vector3 start = Start;
				Vector3 end = End;
				if (start == end)
				{
					return 1f;
				}
				float num = GenMath.SphericalDistance(start.normalized, end.normalized);
				if (num == 0f)
				{
					return 1f;
				}
				return 0.00025f / num;
			}
		}

		public int ConstantRandSeed => HashCode.Combine(ID, 18948292);

		public StoryState StoryState => storyState;

		public GameConditionManager GameConditionManager
		{
			get
			{
				Log.ErrorOnce("Attempted to retrieve condition manager directly from gravship", 13291050);
				return null;
			}
		}

		public float PlayerWealthForStoryteller
		{
			get
			{
				float num = 0f;
				foreach (KeyValuePair<Pawn, PositionData> pawn2 in pawns)
				{
					pawn2.Deconstruct(out var key, out var _);
					Pawn pawn = key;
					num += WealthWatcher.GetEquipmentApparelAndInventoryWealth(pawn);
					if (pawn.Faction == Faction.OfPlayer)
					{
						float num2 = pawn.MarketValue;
						if (pawn.IsSlave)
						{
							num2 *= 0.75f;
						}
						num += num2;
					}
				}
				return num * 0.7f;
			}
		}

		public IEnumerable<Pawn> PlayerPawnsForStoryteller => Pawns.Where((Pawn x) => x.Faction == Faction.OfPlayer);

		public FloatRange IncidentPointsRandomFactorRange => StorytellerUtility.GravshipPointsRandomFactorRange;

		public CellRect Bounds => bounds;

		public Building_GravEngine Engine => engine;

		public Building PilotConsole => pilotConsole;

		public Dictionary<Thing, bool> PoweredOn => powerOn;

		public Dictionary<Thing, Thing> ConnectParents => connectParents;

		public IEnumerable<IntVec3> GravFieldExtenderPositions => GetRotatedValues(gravFieldExtenderPositions);

		public Dictionary<Thing, PositionData.Data> ThingPlacements => GetPlacementValues(things, ref tmpThingPlacements, ref tmpThingsRot);

		public Dictionary<Pawn, PositionData.Data> PawnPlacements => GetPlacementValues(pawns, ref tmpPawns, ref tmpPawnsRot);

		public Dictionary<Thing, PositionData>.KeyCollection Thrusters => thrusters.Keys;

		public Dictionary<Thing, PositionData.Data> ThrusterPlacements => GetPlacementValues(thrusters, ref tmpThrusterPlacements, ref tmpThrusterRot);

		public Dictionary<Thing, PositionData.Data> ExteriorDoorPlacements => GetPlacementValues(exteriorDoors, ref tmpExteriorDoorPlacements, ref tmpExteriorDoorRot);

		public Dictionary<IntVec3, TerrainDef> Foundations => GetRotatedValues(foundations, ref tmpFoundations, ref tmpFoundationRot);

		public Dictionary<IntVec3, TerrainDef> Terrains => GetRotatedValues(terrains, ref tmpTerrains, ref tmpTerrainRot);

		public IEnumerable<(IntVec3, RoofDef)> Roofs => GetRotatedValues(roofs);

		public IEnumerable<(IntVec3, ColorDef)> TerrainColors => GetRotatedValues(terrainColors);

		public IEnumerable<(IntVec3, Designation)> TerrainDesignations => GetRotatedValues(terrainDesignations);

		public IEnumerable<(IntVec3, uint)> Gases => GetRotatedValues(gases);

		public IEnumerable<CellRect> OccupiedRects
		{
			get
			{
				foreach (CellRect occupiedRect in occupiedRects)
				{
					IntVec3 adjustedLocalPosition = PrefabUtility.GetAdjustedLocalPosition(occupiedRect.Min, rotation);
					IntVec3 adjustedLocalPosition2 = PrefabUtility.GetAdjustedLocalPosition(occupiedRect.Max, rotation);
					yield return CellRect.FromLimits(adjustedLocalPosition, adjustedLocalPosition2);
				}
			}
		}

		public IEnumerable<RoomTemperatureVacuum.Data> RoomTemperatures
		{
			get
			{
				foreach (RoomTemperatureVacuum roomTemperature in roomTemperatures)
				{
					yield return new RoomTemperatureVacuum.Data
					{
						local = PrefabUtility.GetAdjustedLocalPosition(roomTemperature.roomCell, rotation),
						temperature = roomTemperature.temperature,
						vacuum = roomTemperature.vacuum
					};
				}
			}
		}

		private Gravship()
		{
			things = new Dictionary<Thing, PositionData>();
			pawns = new Dictionary<Pawn, PositionData>();
			powerOn = new Dictionary<Thing, bool>();
			connectParents = new Dictionary<Thing, Thing>();
			roofs = new Dictionary<IntVec3, RoofDef>();
			foundations = new Dictionary<IntVec3, TerrainDef>();
			terrains = new Dictionary<IntVec3, TerrainDef>();
			terrainColors = new Dictionary<IntVec3, ColorDef>();
			terrainDesignations = new Dictionary<IntVec3, Designation>();
			gases = new Dictionary<IntVec3, uint>();
			areas = new MoveableAreas();
			bounds = CellRect.Empty;
			occupiedRects = new List<CellRect>();
			roomTemperatures = new List<RoomTemperatureVacuum>();
			thrusters = new Dictionary<Thing, PositionData>();
			exteriorDoors = new Dictionary<Thing, PositionData>();
			gravFieldExtenderPositions = new List<IntVec3>();
		}

		public Gravship(Building_GravEngine engine)
			: this()
		{
			if (ModsConfig.OdysseyActive)
			{
				this.engine = engine;
				Map map = engine.Map;
				originalPosition = engine.Position;
				engine.ForceSubstructureDirty();
				HashSet<IntVec3> validSubstructure = engine.ValidSubstructure;
				storyState = new StoryState(this);
				map.storyState.CopyTo(storyState);
				autoSlaughterConfigs = map.autoSlaughterManager.configs;
				TransferZones(map, originalPosition, validSubstructure);
				CopyCellContents(map, originalPosition, validSubstructure, out var outOccupiedCells);
				CopyAreas(map, originalPosition, validSubstructure);
				CopyStorageGroups(map);
				UpdateBoundingBoxes(outOccupiedCells);
				DetermineLaunchDirection();
				CheckAffectedGoodwill();
				engineToCenter = CellRect.FromCellList(engine.ValidSubstructure).CenterVector3 - originalPosition.ToVector3();
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			roofs?.RemoveAll((KeyValuePair<IntVec3, RoofDef> x) => x.Value == null);
			foundations?.RemoveAll((KeyValuePair<IntVec3, TerrainDef> x) => x.Value == null);
			terrains?.RemoveAll((KeyValuePair<IntVec3, TerrainDef> x) => x.Value == null);
			terrainColors?.RemoveAll((KeyValuePair<IntVec3, ColorDef> x) => x.Value == null);
			terrainDesignations?.RemoveAll((KeyValuePair<IntVec3, Designation> x) => x.Value == null);
			Scribe_References.Look(ref engine, "engine");
			Scribe_Collections.Look(ref things, "things", LookMode.Deep, LookMode.Deep);
			Scribe_Collections.Look(ref pawns, "pawns", LookMode.Deep, LookMode.Deep);
			Scribe_Collections.Look(ref powerOn, "powerOn", LookMode.Reference, LookMode.Value, ref tmpPoweredThings, ref tmpPoweredThingsOn);
			Scribe_Collections.Look(ref connectParents, "connectParents", LookMode.Reference, LookMode.Reference, ref tmpConnectedThings, ref tmpConnectedParents);
			Scribe_Collections.Look(ref roofs, "roofs", LookMode.Value, LookMode.Def);
			Scribe_Collections.Look(ref foundations, "foundations", LookMode.Value, LookMode.Def);
			Scribe_Collections.Look(ref terrains, "terrains", LookMode.Value, LookMode.Def);
			Scribe_Collections.Look(ref terrainColors, "terrainColors", LookMode.Value, LookMode.Def);
			Scribe_Collections.Look(ref gases, "gases", LookMode.Value, LookMode.Value);
			Scribe_Collections.Look(ref terrainDesignations, "terrainDesignations", LookMode.Value, LookMode.Deep);
			Scribe_Deep.Look(ref areas, "areas");
			Scribe_Values.Look(ref engineToCenter, "engineToCenter");
			Scribe_Values.Look(ref rotation, "rotation");
			Scribe_Values.Look(ref bounds, "bounds");
			Scribe_Collections.Look(ref occupiedRects, "occupiedRects", LookMode.Value);
			Scribe_Collections.Look(ref roomTemperatures, "roomTemperatures", LookMode.Deep);
			Scribe_Collections.Look(ref thrusters, "thrusters", LookMode.Reference, LookMode.Deep, ref tmpThrusters, ref tmpThrusterPositionData);
			Scribe_Collections.Look(ref exteriorDoors, "exteriorDoors", LookMode.Reference, LookMode.Deep, ref tmpExteriorDoors, ref tmpExteriorDoorPositionData);
			Scribe_Values.Look(ref destinationTile, "destinationTile");
			Scribe_Values.Look(ref initialTile, "initialTile");
			Scribe_Values.Look(ref traveledPct, "traveledPct", 0f);
			Scribe_Values.Look(ref arrived, "arrived", defaultValue: false);
			Scribe_Deep.Look(ref storyState, "storyState", this);
			Scribe_Collections.Look(ref autoSlaughterConfigs, "autoSlaughterConfigs", LookMode.Deep);
			Scribe_Values.Look(ref launchDirection, "launchDirection");
			if (Scribe.mode != LoadSaveMode.PostLoadInit)
			{
				return;
			}
			if (base.Spawned)
			{
				LongEventHandler.ExecuteWhenFinished(delegate
				{
					CameraJumper.TryJump(this);
				});
			}
			Current.Game.Gravship = this;
			if (connectParents == null)
			{
				connectParents = new Dictionary<Thing, Thing>();
			}
		}

		public bool ContainsThing(Thing thing)
		{
			return things.ContainsKey(thing);
		}

		public bool ContainsPawn(Pawn pawn)
		{
			return pawns.ContainsKey(pawn);
		}

		private void DetermineLaunchDirection()
		{
			if (launchDirection == IntVec3.Zero)
			{
				launchDirection = pilotConsole.Rotation.AsIntVec3;
			}
		}

		private void TransferZones(Map oldMap, IntVec3 origin, HashSet<IntVec3> engineFloors)
		{
			for (int num = oldMap.zoneManager.AllZones.Count - 1; num >= 0; num--)
			{
				Zone zone = oldMap.zoneManager.AllZones[num];
				if (zone is Zone_Stockpile zone_Stockpile)
				{
					MoveableStockpile moveableStockpile = null;
					foreach (IntVec3 cell in zone_Stockpile.Cells)
					{
						if (engineFloors.Contains(cell))
						{
							if (moveableStockpile == null)
							{
								moveableStockpile = new MoveableStockpile(this, zone_Stockpile);
							}
							moveableStockpile.Add(cell - origin);
						}
					}
					if (moveableStockpile != null)
					{
						areas.stockpiles.Add(moveableStockpile);
						zone.Delete();
					}
				}
				else if (zone is Zone_Growing zone_Growing)
				{
					MoveableGrowZone moveableGrowZone = null;
					foreach (IntVec3 cell2 in zone_Growing.Cells)
					{
						if (engineFloors.Contains(cell2))
						{
							if (moveableGrowZone == null)
							{
								moveableGrowZone = new MoveableGrowZone(this, zone_Growing);
							}
							moveableGrowZone.Add(cell2 - origin);
						}
					}
					if (moveableGrowZone != null)
					{
						areas.growZones.Add(moveableGrowZone);
						zone.Delete();
					}
				}
			}
		}

		private void CopyAreas(Map oldMap, IntVec3 origin, HashSet<IntVec3> engineFloors)
		{
			foreach (Area area in oldMap.areaManager.AllAreas)
			{
				if (area is Area_Home)
				{
					areas.homeArea = new MoveableArea_Allowed(this, area);
					foreach (IntVec3 activeCell in area.ActiveCells)
					{
						if (engineFloors.Contains(activeCell))
						{
							areas.homeArea.Add(activeCell - origin);
						}
					}
					areas.homeArea.assignedPawns.AddRange(Pawns.Where((Pawn x) => x.playerSettings?.AreaRestrictionInPawnCurrentMap == area));
					foreach (Pawn item in oldMap.mapPawns.AllPawnsUnspawned)
					{
						if (item.SpawnedParentOrMe != null && things.ContainsKey(item.SpawnedParentOrMe) && item.playerSettings?.AreaRestrictionInPawnCurrentMap == area)
						{
							areas.homeArea.assignedPawns.Add(item);
						}
					}
					foreach (Pawn assignedPawn in areas.homeArea.assignedPawns)
					{
						assignedPawn.playerSettings?.Notify_MapRemoved(assignedPawn.MapHeld);
					}
					continue;
				}
				if (area is Area_Allowed area2)
				{
					MoveableArea_Allowed moveableArea_Allowed = new MoveableArea_Allowed(this, area2);
					foreach (IntVec3 activeCell2 in area.ActiveCells)
					{
						if (engineFloors.Contains(activeCell2))
						{
							moveableArea_Allowed.Add(activeCell2 - origin);
						}
					}
					moveableArea_Allowed.assignedPawns.AddRange(Pawns.Where((Pawn x) => x.playerSettings?.AreaRestrictionInPawnCurrentMap == area));
					areas.allowedAreas.Add(moveableArea_Allowed);
					foreach (Pawn assignedPawn2 in moveableArea_Allowed.assignedPawns)
					{
						assignedPawn2.playerSettings?.Notify_MapRemoved(assignedPawn2.Map);
					}
					continue;
				}
				MoveableArea moveableArea = null;
				foreach (IntVec3 activeCell3 in area.ActiveCells)
				{
					if (engineFloors.Contains(activeCell3))
					{
						if (moveableArea == null)
						{
							moveableArea = new MoveableArea(this, area.Label, area.RenamableLabel, area.Color, area.ID);
						}
						moveableArea.Add(activeCell3 - origin);
					}
				}
				if (moveableArea != null)
				{
					if (area is Area_NoRoof)
					{
						areas.noRoofArea = moveableArea;
					}
					else if (area is Area_BuildRoof)
					{
						areas.buildRoofArea = moveableArea;
					}
					else if (area is Area_SnowOrSandClear)
					{
						areas.snowClearArea = moveableArea;
					}
					else if (ModsConfig.BiotechActive && area is Area_PollutionClear)
					{
						areas.pollutionClearArea = moveableArea;
					}
				}
			}
		}

		private void CopyStorageGroups(Map oldMap)
		{
			for (int num = oldMap.storageGroups.StorageGroupsForReading.Count - 1; num >= 0; num--)
			{
				StorageGroup storageGroup = oldMap.storageGroups.StorageGroupsForReading[num];
				MoveableStorageGroup moveableStorageGroup = null;
				foreach (IStorageGroupMember member in storageGroup.members)
				{
					if (member is Thing key && things.ContainsKey(key))
					{
						if (moveableStorageGroup == null)
						{
							moveableStorageGroup = new MoveableStorageGroup(storageGroup);
						}
						moveableStorageGroup.members.Add(member);
					}
				}
				if (moveableStorageGroup != null)
				{
					if (moveableStorageGroup.members.Any())
					{
						areas.storageGroups.Add(moveableStorageGroup);
					}
					foreach (IStorageGroupMember member2 in moveableStorageGroup.members)
					{
						member2.SetStorageGroup(null);
					}
				}
			}
		}

		private void CopyCellContents(Map oldMap, IntVec3 origin, HashSet<IntVec3> engineFloors, out HashSet<IntVec3> outOccupiedCells)
		{
			HashSet<Room> hashSet = new HashSet<Room>();
			outOccupiedCells = new HashSet<IntVec3>();
			foreach (CompGravshipFacility gravshipComponent in engine.GravshipComponents)
			{
				AddThing(gravshipComponent.parent, gravshipComponent.parent.Position - origin);
			}
			AddThing(engine, engine.Position - origin);
			foreach (IntVec3 engineFloor in engineFloors)
			{
				IntVec3 intVec = engineFloor - origin;
				List<Thing> list = oldMap.thingGrid.ThingsListAt(engineFloor);
				for (int num = list.Count - 1; num >= 0; num--)
				{
					Thing thing = list[num];
					if (ShouldBringOnGravship(thing, engineFloor) && (!(thing is Building building) || !building.def.building.isAttachment))
					{
						foreach (Thing attachedBuilding in GenConstruct.GetAttachedBuildings(thing))
						{
							AddThing(attachedBuilding, intVec + attachedBuilding.Rotation.Opposite.FacingCell);
						}
						AddThing(thing, intVec);
					}
				}
				outOccupiedCells.Add(intVec);
				foundations[intVec] = oldMap.terrainGrid.FoundationAt(engineFloor);
				terrains[intVec] = oldMap.terrainGrid.TerrainAt(engineFloor);
				terrainColors[intVec] = oldMap.terrainGrid.ColorAt(engineFloor);
				roofs[intVec] = oldMap.roofGrid.RoofAt(engineFloor);
				foreach (Designation item in oldMap.designationManager.AllDesignationsAt(engineFloor))
				{
					if (item.def == DesignationDefOf.RemoveFloor || item.def == DesignationDefOf.PaintFloor || item.def == DesignationDefOf.RemovePaintFloor)
					{
						terrainDesignations[intVec] = item;
						oldMap.designationManager.RemoveDesignation(item);
					}
				}
				gases[intVec] = oldMap.gasGrid.GetDirect(engineFloor);
				Room room = engineFloor.GetRoom(oldMap);
				if (room != null && hashSet.Add(room) && !room.UsesOutdoorTemperature)
				{
					roomTemperatures.Add(new RoomTemperatureVacuum
					{
						roomCell = intVec,
						temperature = room.Temperature,
						vacuum = room.Vacuum
					});
				}
			}
		}

		private static bool ShouldBringOnGravship(Thing thing, IntVec3 cell)
		{
			if (thing is Mote)
			{
				return false;
			}
			if (thing is Skyfaller)
			{
				return false;
			}
			if (!thing.def.bringAlongOnGravship)
			{
				return false;
			}
			if (cell != thing.Position)
			{
				return false;
			}
			if (thing.def.category == ThingCategory.Ethereal && !(thing is Blueprint) && !(thing is Frame))
			{
				return false;
			}
			return true;
		}

		private void AddThing(Thing thing, IntVec3 offset)
		{
			if (thing is Mote || thing is Skyfaller || !thing.def.bringAlongOnGravship)
			{
				return;
			}
			if (thing is Pawn pawn)
			{
				if (pawns.ContainsKey(pawn))
				{
					return;
				}
				pawns.Add(pawn, new PositionData(offset, pawn.Rotation, pawn.Drafted));
				if (pawn.carryTracker.CarriedThing != null && pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out var resultingThing) && !resultingThing.Destroyed)
				{
					AddThing(resultingThing, offset);
				}
			}
			else
			{
				if (things.ContainsKey(thing) || !engine.OnValidSubstructure(thing))
				{
					return;
				}
				things.Add(thing, new PositionData(offset, thing.Rotation));
			}
			if (thing.TryGetComp(out CompPowerTrader comp))
			{
				powerOn.Add(thing, comp.PowerOn);
			}
			if (thing.TryGetComp(out CompPower comp2))
			{
				connectParents.Add(thing, comp2.connectParent?.parent);
			}
			if (thing.TryGetComp(out CompGravshipThruster comp3) && comp3.CanBeActive)
			{
				thrusters.Add(thing, new PositionData(offset, thing.Rotation));
				launchDirection += thing.Rotation.AsIntVec3 * comp3.Props.directionInfluence;
			}
			if (thing is Building_Door t)
			{
				foreach (IntVec3 item in GenAdj.CellsAdjacentCardinal(t))
				{
					if (!engine.ValidSubstructureAt(item))
					{
						exteriorDoors.Add(thing, new PositionData(offset, thing.Rotation));
						break;
					}
				}
			}
			if (thing.def == ThingDefOf.GravFieldExtender)
			{
				gravFieldExtenderPositions.Add(offset);
			}
			if (thing.def == ThingDefOf.PilotConsole)
			{
				pilotConsole = (Building)thing;
			}
		}

		private void UpdateBoundingBoxes(HashSet<IntVec3> occupiedCells)
		{
			HashSet<IntVec3> hashSet = new HashSet<IntVec3>();
			bounds = CellRect.FromCellList(occupiedCells);
			for (int i = bounds.minX; i <= bounds.maxX; i++)
			{
				for (int j = bounds.minZ; j <= bounds.maxZ; j++)
				{
					IntVec3 item = new IntVec3(i, 0, j);
					if (!occupiedCells.Contains(item) || hashSet.Contains(item))
					{
						continue;
					}
					int num = 1;
					int num2 = 1;
					for (int k = i + 1; k <= bounds.maxX && occupiedCells.Contains(new IntVec3(k, 0, j)); k++)
					{
						num++;
					}
					bool flag = true;
					while (j + num2 <= bounds.maxZ && flag)
					{
						for (int l = 0; l < num; l++)
						{
							if (!occupiedCells.Contains(new IntVec3(i + l, 0, j + num2)) || hashSet.Contains(new IntVec3(i + 1, 0, j + num2)))
							{
								flag = false;
								break;
							}
						}
						if (flag)
						{
							num2++;
						}
					}
					for (int m = 0; m < num; m++)
					{
						for (int n = 0; n < num2; n++)
						{
							hashSet.Add(new IntVec3(i + m, 0, j + n));
						}
					}
					occupiedRects.Add(new CellRect(i, j, num, num2));
				}
			}
		}

		private void CheckAffectedGoodwill()
		{
			tmpFactionMembersKidnappedCount.Clear();
			Faction key;
			int value;
			foreach (var (pawn2, _) in pawns)
			{
				if (GravshipUtility.ConsideredKidnapped(engine, pawn2))
				{
					tmpFactionMembersKidnappedCount.TryAdd(pawn2.Faction, 0);
					Dictionary<Faction, int> dictionary = tmpFactionMembersKidnappedCount;
					key = pawn2.Faction;
					value = dictionary[key]++;
				}
			}
			foreach (KeyValuePair<Faction, int> item in tmpFactionMembersKidnappedCount)
			{
				item.Deconstruct(out key, out value);
				key.TryAffectGoodwillWith(goodwillChange: -10 * value, other: Faction.OfPlayer, canSendMessage: true, canSendHostilityLetter: true, reason: HistoryEventDefOf.PawnKidnappedOnGravShip);
			}
		}

		public override void PostAdd()
		{
			base.PostAdd();
			initialTile = base.Tile;
		}

		protected override void TickInterval(int delta)
		{
			base.TickInterval(delta);
			traveledPct += TraveledPctStepPerTick * (float)delta;
			if (!(traveledPct >= 1f))
			{
				return;
			}
			traveledPct = 1f;
			if (Find.WorldObjects.MapParentAt(destinationTile)?.Map != null)
			{
				GravshipUtility.ArriveExistingMap(this);
				return;
			}
			LongEventHandler.QueueLongEvent(delegate
			{
				GravshipUtility.ArriveNewMap(this);
			}, "GeneratingMap", doAsynchronously: false, GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap);
		}

		private Dictionary<T, PositionData.Data> GetPlacementValues<T>(Dictionary<T, PositionData> dictionary, ref Dictionary<T, PositionData.Data> working, ref Rot4 prevRotation) where T : Thing
		{
			if (working != null && prevRotation == rotation)
			{
				return working;
			}
			if (working == null)
			{
				working = new Dictionary<T, PositionData.Data>(dictionary.Count);
			}
			working.Clear();
			foreach (KeyValuePair<T, PositionData> item in dictionary)
			{
				item.Deconstruct(out var key, out var value);
				T val = key;
				PositionData positionData = value;
				IntVec3 center = PrefabUtility.GetAdjustedLocalPosition(positionData.position, rotation);
				IntVec2 size = val.def.size;
				bool flag = true;
				if (!val.def.rotatable && size.x == size.z)
				{
					GenAdj.AdjustForRotation(ref center, ref size, val.def.defaultPlacingRot, rotation);
					flag = false;
				}
				else if (!val.def.rotatable && val.def.category != ThingCategory.Building)
				{
					flag = false;
				}
				Rot4 rot = ((!flag) ? val.def.defaultPlacingRot : rotation.Rotated(positionData.relativeRotation));
				working[val] = new PositionData.Data
				{
					local = center,
					rotation = rot,
					drafted = positionData.drafted
				};
			}
			prevRotation = rotation;
			return working;
		}

		private Dictionary<IntVec3, T> GetRotatedValues<T>(Dictionary<IntVec3, T> dictionary, ref Dictionary<IntVec3, T> working, ref Rot4 prevRotation)
		{
			if (working != null && prevRotation == rotation)
			{
				return working;
			}
			if (working == null)
			{
				working = new Dictionary<IntVec3, T>(dictionary.Count);
			}
			working.Clear();
			foreach (var (local, value) in dictionary)
			{
				working[PrefabUtility.GetAdjustedLocalPosition(local, rotation)] = value;
			}
			prevRotation = rotation;
			return working;
		}

		private IEnumerable<(IntVec3, T)> GetRotatedValues<T>(Dictionary<IntVec3, T> dictionary)
		{
			foreach (var (local, item) in dictionary)
			{
				yield return (PrefabUtility.GetAdjustedLocalPosition(local, rotation), item);
			}
		}

		private IEnumerable<IntVec3> GetRotatedValues(List<IntVec3> list)
		{
			foreach (IntVec3 item in list)
			{
				yield return PrefabUtility.GetAdjustedLocalPosition(item, rotation);
			}
		}
	}
}
