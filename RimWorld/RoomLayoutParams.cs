using Verse;

namespace RimWorld;

public struct RoomLayoutParams
{
	public LayoutStructureSketch sketch;

	public CellRect container;

	public int minRoomWidth;

	public int minRoomHeight;

	public float areaPrunePercent;

	public IntRange? maxMergeRoomsRange;

	public int entranceCount;

	public bool canRemoveRooms;

	public bool generateDoors;

	public bool canDisconnectRooms;

	public LayoutRoomDef corridor;

	public int corridorExpansion;

	public CorridorShape corridorShapes;

	public bool singleRoom;
}
