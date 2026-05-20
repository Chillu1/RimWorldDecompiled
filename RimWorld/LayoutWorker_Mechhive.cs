using UnityEngine;
using Verse;

namespace RimWorld;

public class LayoutWorker_Mechhive : LayoutWorker_Structure
{
	private const float BlastDoorRatio = 0.2f;

	public LayoutWorker_Mechhive(LayoutDef def)
		: base(def)
	{
	}

	protected override StructureLayout GetStructureLayout(StructureGenParams parms, CellRect rect)
	{
		return RoomLayoutGenerator.GenerateRandomLayout(parms.sketch, rect, minRoomHeight: base.Def.minRoomHeight, minRoomWidth: base.Def.minRoomWidth, areaPrunePercent: 0.1f, canRemoveRooms: true, generateDoors: false, corridor: null, corridorExpansion: 2, maxMergeRoomsRange: new IntRange(2, 4));
	}

	protected override void PostLayoutFlushedToSketch(LayoutStructureSketch parms)
	{
		base.PostLayoutFlushedToSketch(parms);
		ReplaceDoors(parms.layoutSketch);
	}

	private static void ReplaceDoors(LayoutSketch sketch)
	{
		int num = Mathf.CeilToInt((float)sketch.Things.Count((SketchThing thing) => thing.def.IsDoor) * 0.2f);
		foreach (SketchThing item in sketch.Things.InRandomOrder())
		{
			if (item.def.IsDoor)
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
