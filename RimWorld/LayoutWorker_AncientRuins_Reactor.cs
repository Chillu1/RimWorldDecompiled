using Verse;

namespace RimWorld;

public class LayoutWorker_AncientRuins_Reactor : LayoutWorker_AncientRuins
{
	protected override float RoomToExteriorDoorRatio => 0.1f;

	protected override bool SpawnRandomBlastDoors => true;

	public LayoutWorker_AncientRuins_Reactor(LayoutDef def)
		: base(def)
	{
	}

	protected override StructureLayout GetStructureLayout(StructureGenParams parms, CellRect rect)
	{
		LayoutStructureSketch sketch = parms.sketch;
		int minRoomHeight = base.Def.minRoomHeight;
		return RoomLayoutGenerator.GenerateRandomLayout(minRoomWidth: base.Def.minRoomWidth, minRoomHeight: minRoomHeight, areaPrunePercent: 0.1f, canRemoveRooms: true, generateDoors: false, maxMergeRoomsRange: new IntRange(2, 4), sketch: sketch, container: rect, corridor: base.Def.corridorDef, corridorExpansion: 2, corridorShapes: base.Def.corridorShapes, canDisconnectRooms: false);
	}
}
