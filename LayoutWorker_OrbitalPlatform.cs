using RimWorld;
using Verse;

public class LayoutWorker_OrbitalPlatform : LayoutWorker_Structure
{
	public LayoutWorker_OrbitalPlatform(LayoutDef def)
		: base(def)
	{
	}

	protected override StructureLayout GetStructureLayout(StructureGenParams parms, CellRect rect)
	{
		return RoomLayoutGenerator.GenerateRandomLayout(rect, 10, 10, 0.1f, canRemoveRooms: true, generateDoors: false, maxMergeRoomsRange: new IntRange(2, 4), corridorShapes: base.Def.corridorShapes, corridor: base.Def.corridorDef, corridorExpansion: 2, canDisconnectRooms: false);
	}
}
