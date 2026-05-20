using RimWorld;
using Verse;

public class LayoutWorker_SimpleRuin : LayoutWorker_Structure
{
	protected override float RoomToExteriorDoorRatio => 0f;

	public LayoutWorker_SimpleRuin(LayoutDef def)
		: base(def)
	{
	}

	protected override StructureLayout GetStructureLayout(StructureGenParams parms, CellRect rect)
	{
		return RoomLayoutGenerator.GenerateRandomLayout(parms.sketch, rect, minRoomHeight: base.Def.minRoomHeight, minRoomWidth: base.Def.minRoomWidth, areaPrunePercent: 0f, canRemoveRooms: false, generateDoors: false, corridor: null, corridorExpansion: 2, maxMergeRoomsRange: IntRange.Zero);
	}
}
