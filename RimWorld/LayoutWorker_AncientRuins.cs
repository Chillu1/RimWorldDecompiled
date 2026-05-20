using UnityEngine;
using Verse;

namespace RimWorld;

public class LayoutWorker_AncientRuins : LayoutWorker_Structure
{
	protected override float RoomToExteriorDoorRatio => 0.33f;

	protected virtual float RandomBlastDoorRatio => 0.5f;

	protected virtual bool SpawnRandomBlastDoors => false;

	protected override ThingDef GetWallDoorStuff(StructureGenParams parms)
	{
		ThingDef wallDoorStuff = base.GetWallDoorStuff(parms);
		if (wallDoorStuff != ThingDefOf.WoodLog)
		{
			return wallDoorStuff;
		}
		return ThingDefOf.Steel;
	}

	public LayoutWorker_AncientRuins(LayoutDef def)
		: base(def)
	{
	}

	protected override StructureLayout GetStructureLayout(StructureGenParams parms, CellRect rect)
	{
		LayoutStructureSketch sketch = parms.sketch;
		float areaPrunePercent = base.Def.areaPrunePercent;
		int minRoomHeight = base.Def.minRoomHeight;
		return RoomLayoutGenerator.GenerateRandomLayout(minRoomWidth: base.Def.minRoomWidth, minRoomHeight: minRoomHeight, areaPrunePercent: areaPrunePercent, canRemoveRooms: true, generateDoors: false, maxMergeRoomsRange: IntRange.One, sketch: sketch, container: rect, corridor: base.Def.corridorDef ?? LayoutRoomDefOf.AncientRuinsCorridor, corridorExpansion: 2, corridorShapes: base.Def.corridorShapes, canDisconnectRooms: base.Def.canDisconnectRooms);
	}

	protected override void PostLayoutFlushedToSketch(LayoutStructureSketch parms)
	{
		base.PostLayoutFlushedToSketch(parms);
		if (SpawnRandomBlastDoors)
		{
			ReplaceRandomBlastDoors(parms.layoutSketch);
		}
	}

	protected virtual void ReplaceRandomBlastDoors(LayoutSketch sketch)
	{
		int num = Mathf.CeilToInt((float)sketch.Things.Count((SketchThing thing) => thing.def.IsDoor) * RandomBlastDoorRatio);
		foreach (SketchThing item in sketch.Things.InRandomOrder())
		{
			if (item.def.IsDoor && item.def != ThingDefOf.AncientBlastDoor)
			{
				item.def = ThingDefOf.AncientBlastDoor;
				item.stuff = null;
				num--;
				if (num <= 0)
				{
					break;
				}
			}
		}
	}
}
