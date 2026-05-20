using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class LayoutStructureSketch : IExposable, ILoadReferenceable
{
	public LayoutSketch layoutSketch;

	public StructureLayout structureLayout;

	public List<Thing> thingsToSpawn = new List<Thing>();

	public string thingDiscoveredMessage;

	public LayoutDef layoutDef;

	public bool spawned;

	public IntVec3 center;

	public int id;

	public string uniqueId;

	public string GetUniqueLoadID()
	{
		return uniqueId;
	}

	public void ExposeData()
	{
		Scribe_Values.Look(ref thingDiscoveredMessage, "thingDiscoveredMessage");
		Scribe_Values.Look(ref spawned, "spawned", defaultValue: false);
		Scribe_Values.Look(ref center, "center");
		Scribe_Values.Look(ref id, "id", 0);
		Scribe_Values.Look(ref uniqueId, "uniqueId");
		Scribe_Defs.Look(ref layoutDef, "layoutDef");
		Scribe_Deep.Look(ref layoutSketch, "layoutSketch");
		Scribe_Deep.Look(ref structureLayout, "structureLayout", this);
		Scribe_Collections.Look(ref thingsToSpawn, "thingsToSpawn", LookMode.Deep);
		if (Scribe.mode == LoadSaveMode.LoadingVars && string.IsNullOrEmpty(uniqueId))
		{
			uniqueId = $"LayoutStructureSketch_{Find.UniqueIDsManager.GetNextStructureSketchID()}";
		}
	}

	public bool AnyRoomContains(IntVec3 c, LayoutRoom exclude = null)
	{
		foreach (LayoutRoom room in structureLayout.Rooms)
		{
			if (room != exclude && room.Contains(c))
			{
				return true;
			}
		}
		return false;
	}

	public float ClosestDistTo(IntVec3 c)
	{
		float num = float.MaxValue;
		foreach (LayoutRoom room in structureLayout.Rooms)
		{
			foreach (CellRect rect in room.rects)
			{
				num = Mathf.Min(rect.ClosestDistanceTo(c), num);
			}
		}
		return num;
	}
}
