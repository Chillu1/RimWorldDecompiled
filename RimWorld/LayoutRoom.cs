using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class LayoutRoom : IExposable, ILoadReferenceable
{
	public List<CellRect> rects;

	public List<LayoutRoomDef> defs = new List<LayoutRoomDef>();

	public List<LayoutRoom> merged = new List<LayoutRoom>();

	public List<IntVec3> entryCells;

	public LayoutRoomDef requiredDef;

	public LayoutStructureSketch sketch;

	public bool noExteriorDoors;

	private string threatSignal;

	public int loadId;

	public int id;

	public readonly List<LayoutRoom> connections = new List<LayoutRoom>();

	private static readonly HashSet<IntVec3> cells = new HashSet<IntVec3>();

	public int Area => rects.Sum((CellRect r) => r.Area);

	public CellRect Boundary
	{
		get
		{
			CellRect result = rects[0];
			for (int i = 1; i < rects.Count; i++)
			{
				result.Encapsulate(rects[i]);
			}
			return result;
		}
	}

	public bool DontDestroyWallsDoors
	{
		get
		{
			if (!defs.NullOrEmpty())
			{
				return defs.Any((LayoutRoomDef d) => d.dontDestroyWallsDoors);
			}
			return false;
		}
	}

	public IEnumerable<IntVec3> Corners => rects.SelectMany((CellRect r) => r.Corners);

	public string ThreatSignal => threatSignal;

	public IEnumerable<IntVec3> Cells
	{
		get
		{
			for (int i = 0; i < rects.Count; i++)
			{
				foreach (IntVec3 cell in rects[i].Cells)
				{
					yield return cell;
				}
			}
		}
	}

	public LayoutRoom()
	{
	}

	public LayoutRoom(LayoutStructureSketch sketch, List<CellRect> rects)
	{
		this.rects = rects;
		this.sketch = sketch;
		threatSignal = "RoomThreat" + Find.UniqueIDsManager.GetNextSignalTagID();
		loadId = Find.UniqueIDsManager.GetNextRoomID();
	}

	public bool TryGetRectOfSize(int minWidth, int minHeight, out CellRect rect)
	{
		foreach (CellRect rect2 in rects)
		{
			if (rect2.AreSidesEqualOrGreater(minWidth, minHeight))
			{
				rect = rect2;
				return true;
			}
		}
		rect = default(CellRect);
		return false;
	}

	public bool IsCorner(IntVec3 position)
	{
		for (int i = 0; i < rects.Count; i++)
		{
			if (rects[i].IsCorner(position))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasLayoutDef(LayoutRoomDef def)
	{
		return defs.Contains(def);
	}

	public bool IsAdjacentTo(LayoutRoom room, int minAdjacencyScore = 1)
	{
		foreach (CellRect rect in rects)
		{
			foreach (CellRect rect2 in room.rects)
			{
				if (rect.ContractedBy(1).GetAdjacencyScore(rect2) >= minAdjacencyScore)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool Contains(IntVec3 position, int contractedBy = 0)
	{
		foreach (CellRect rect in rects)
		{
			if (rect.ContractedBy(contractedBy).Contains(position))
			{
				return true;
			}
		}
		return false;
	}

	public void SpawnRectTriggersForAction(SignalAction action, Map map)
	{
		foreach (CellRect rect in rects)
		{
			RectTrigger obj = (RectTrigger)ThingMaker.MakeThing(ThingDefOf.RectTrigger);
			obj.signalTag = action.signalTag;
			obj.Rect = rect;
			GenSpawn.Spawn(obj, rect.CenterCell, map);
		}
	}

	public bool TryGetRandomCellInRoom(ThingDef def, Map map, out IntVec3 cell, Rot4? rot = null, int contractedBy = 0, int padding = 0, Func<IntVec3, bool> validator = null, bool ignoreBuildings = false)
	{
		Rot4 rot2 = rot ?? Rot4.North;
		foreach (CellRect item in rects.InRandomOrder())
		{
			foreach (IntVec3 item2 in item.ContractedBy(contractedBy).Cells.InRandomOrder())
			{
				CellRect cellRect = item2.RectAbout(def.size, rot2);
				if (!GenSpawn.CanSpawnAt(def, item2, map, rot, ignoreBuildings) || (def.hasInteractionCell && !ThingUtility.InteractionCellWhenAt(def, item2, rot2, map).Standable(map)))
				{
					continue;
				}
				bool flag = false;
				foreach (IntVec3 item3 in cellRect.ExpandedBy(padding))
				{
					flag = false;
					if (!item3.InBounds(map) || !Contains(item3) || (!ignoreBuildings && item3.GetEdifice(map) != null) || (!ignoreBuildings && item3.GetFirstBuilding(map) != null) || (!ignoreBuildings && def.passability != Traversability.Standable && RoomGenUtility.IsDoorAdjacentTo(item3, map)) || item3.GetFirstPawn(map) != null || (validator != null && !validator(item3)))
					{
						break;
					}
					flag = true;
				}
				if (flag)
				{
					cell = item2;
					return true;
				}
			}
		}
		cell = IntVec3.Invalid;
		return false;
	}

	public bool TryGetRandomCellInRoom(Map map, out IntVec3 cell, int contractedBy = 0, int padding = 0, Func<IntVec3, bool> validator = null, bool ignoreBuildings = false)
	{
		foreach (CellRect item in rects.InRandomOrder())
		{
			foreach (IntVec3 item2 in item.ContractedBy(contractedBy).Cells.InRandomOrder())
			{
				bool flag = false;
				foreach (IntVec3 item3 in CellRect.FromCell(item2).ExpandedBy(padding).ExpandedBy(padding))
				{
					flag = false;
					if (!item3.InBounds(map) || !Contains(item3) || (!ignoreBuildings && item3.GetEdifice(map) != null) || (!ignoreBuildings && item3.GetFirstBuilding(map) != null) || item3.GetFirstPawn(map) != null || (validator != null && !validator(item3)))
					{
						break;
					}
					flag = true;
				}
				if (flag)
				{
					cell = item2;
					return true;
				}
			}
		}
		cell = IntVec3.Invalid;
		return false;
	}

	public bool TryGetRectContainingCell(IntVec3 cell, out CellRect rect)
	{
		foreach (CellRect rect2 in rects)
		{
			if (rect2.Contains(cell))
			{
				rect = rect2;
				return true;
			}
		}
		rect = CellRect.Empty;
		return false;
	}

	public bool TryGetRandomCellInRoom(out IntVec3 cell, int contractedBy = 0, Func<IntVec3, bool> validator = null)
	{
		for (int i = 0; i < rects.Count; i++)
		{
			foreach (IntVec3 cell2 in rects[i].ContractedBy(contractedBy).Cells)
			{
				if (validator == null || validator(cell2))
				{
					cells.Add(cell2);
				}
			}
		}
		if (cells.Count == 0)
		{
			cell = IntVec3.Invalid;
			return false;
		}
		cell = cells.RandomElement();
		cells.Clear();
		return true;
	}

	public void ExposeData()
	{
		Scribe_Values.Look(ref noExteriorDoors, "noExteriorDoors", defaultValue: false);
		Scribe_Values.Look(ref loadId, "loadId", 0);
		Scribe_Values.Look(ref id, "id", 0);
		Scribe_Values.Look(ref threatSignal, "threatSignal");
		Scribe_Defs.Look(ref requiredDef, "requiredDef");
		Scribe_References.Look(ref sketch, "sketch");
		Scribe_Collections.Look(ref defs, "defs", LookMode.Def);
		Scribe_Collections.Look(ref merged, "merged", LookMode.Reference);
		Scribe_Collections.Look(ref rects, "rects", LookMode.Value);
		Scribe_Collections.Look(ref entryCells, "entryCells", LookMode.Value);
		if (Scribe.mode == LoadSaveMode.LoadingVars && loadId == 0)
		{
			loadId = Rand.Int;
		}
	}

	public string GetUniqueLoadID()
	{
		return $"LayoutRoom_{loadId}";
	}
}
