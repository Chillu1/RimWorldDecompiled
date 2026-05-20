using Verse;

namespace RimWorld;

public class LayoutWorker_SingleRoom : LayoutWorker_Structure
{
	public LayoutWorker_SingleRoom(LayoutDef def)
		: base(def)
	{
	}

	protected override StructureLayout GetStructureLayout(StructureGenParams parms, CellRect rect)
	{
		return RoomLayoutGenerator.GenerateRandomLayout(parms.sketch, rect, minRoomHeight: base.Def.minRoomHeight, minRoomWidth: base.Def.minRoomWidth, areaPrunePercent: 0.2f, canRemoveRooms: false, generateDoors: false, corridor: null, corridorExpansion: 2, maxMergeRoomsRange: null, corridorShapes: CorridorShape.All, canDisconnectRooms: true, singleRoom: true);
	}
}
