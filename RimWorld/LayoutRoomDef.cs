using System;
using System.Collections.Generic;
using RimWorld.SketchGen;
using Verse;

namespace RimWorld;

public class LayoutRoomDef : Def
{
	public SketchResolverDef sketchResolverDef;

	public IntRange areaSizeRange = new IntRange(25, int.MaxValue);

	public bool requiresSingleRectRoom;

	public List<TerrainDef> floorTypes;

	public TerrainDef edgeTerrain;

	public int minSingleRectWidth;

	public int minSingleRectHeight;

	public Type roomContentsWorkerType = typeof(RoomContentsWorker);

	public bool canBeInMixedRoom;

	public bool dontPlaceRandomly;

	public bool isValidPlayerSpawnRoom = true;

	public bool dontDestroyWallsDoors;

	public SimpleCurve threatPointsScaleCurve;

	public int minConnectedRooms;

	public int minAdjRooms;

	public bool spawnJunk = true;

	public bool canMergeWithAdjacentRoom;

	public bool canRemoveBorderDoors;

	public bool canRemoveBorderWalls;

	public RoofDef roofDef;

	public bool noRoof;

	public FloatRange? itemsPer100CellsRange;

	public ThingSetMakerDef thingSetMakerDef;

	public List<LayoutScatterParms> scatter = new List<LayoutScatterParms>();

	public List<LayoutScatterTerrainParms> scatterTerrain = new List<LayoutScatterTerrainParms>();

	public List<LayoutFillEdgesParms> fillEdges = new List<LayoutFillEdgesParms>();

	public List<LayoutFillInteriorParms> fillInterior = new List<LayoutFillInteriorParms>();

	public List<LayoutWallAttatchmentParms> wallAttachments = new List<LayoutWallAttatchmentParms>();

	public List<LayoutPrefabParms> prefabs = new List<LayoutPrefabParms>();

	public List<LayoutPartParms> parts = new List<LayoutPartParms>();

	[Unsaved(false)]
	private RoomContentsWorker workerInt;

	public RoomContentsWorker ContentsWorker => GetWorker(ref workerInt);

	public bool CanResolve(LayoutRoom room)
	{
		int area = room.Area;
		if (room.requiredDef != this && dontPlaceRandomly)
		{
			return false;
		}
		if ((minSingleRectHeight > 0 || minSingleRectWidth > 0) && !room.TryGetRectOfSize(minSingleRectWidth, minSingleRectHeight, out var _))
		{
			return false;
		}
		if (!SatisfiesMinAdjRooms(room))
		{
			return false;
		}
		if (area >= areaSizeRange.min && area <= areaSizeRange.max && room.connections.Count >= minConnectedRooms)
		{
			if (requiresSingleRectRoom)
			{
				return room.rects.Count == 1;
			}
			return true;
		}
		return false;
	}

	private bool SatisfiesMinAdjRooms(LayoutRoom room)
	{
		if (minAdjRooms <= 0)
		{
			return true;
		}
		if (room.connections.Count < minAdjRooms)
		{
			return false;
		}
		int num = 0;
		foreach (LayoutRoom connection in room.connections)
		{
			int num2 = num;
			foreach (CellRect rect in connection.rects)
			{
				foreach (CellRect rect2 in room.rects)
				{
					if (rect.OverlapsCardinal(rect2))
					{
						num++;
						break;
					}
				}
				if (num != num2)
				{
					break;
				}
			}
			if (num >= minAdjRooms)
			{
				break;
			}
		}
		if (num < minAdjRooms)
		{
			return false;
		}
		return true;
	}

	public void ResolveSketch(LayoutRoomParams parms)
	{
		SketchResolveParams parms2 = default(SketchResolveParams);
		foreach (CellRect rect in parms.room.rects)
		{
			if (!floorTypes.NullOrEmpty())
			{
				TerrainDef def = floorTypes.RandomElement();
				foreach (IntVec3 item in rect)
				{
					parms.sketch.AddTerrain(def, item);
				}
			}
			if (edgeTerrain != null)
			{
				foreach (IntVec3 edgeCell in rect.ContractedBy(1).EdgeCells)
				{
					bool flag = true;
					foreach (CellRect rect2 in parms.room.rects)
					{
						if (!(rect2 == rect) && rect2.ContractedBy(1).Contains(edgeCell))
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						parms.sketch.AddTerrain(edgeTerrain, edgeCell);
					}
				}
			}
			parms2.rect = rect;
			parms2.sketch = parms.sketch;
			if (sketchResolverDef != null)
			{
				sketchResolverDef.Resolve(parms2);
			}
		}
	}

	public void PreResolveContents(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
		ContentsWorker?.PreFillRooms(map, room, faction, threatPoints);
	}

	public void PostResolveContents(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
		ContentsWorker?.PostFillRooms(map, room, faction, threatPoints);
	}

	public void ResolveContents(Map map, LayoutRoom room, float? threatPoints = null, Faction faction = null)
	{
		ContentsWorker?.FillRoom(map, room, faction, threatPoints);
	}

	private RoomContentsWorker GetWorker(ref RoomContentsWorker worker)
	{
		if (roomContentsWorkerType == null)
		{
			return null;
		}
		if (worker != null)
		{
			return worker;
		}
		worker = (RoomContentsWorker)Activator.CreateInstance(roomContentsWorkerType);
		worker.Initialize(this);
		return worker;
	}
}
