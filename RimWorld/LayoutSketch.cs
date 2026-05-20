using Verse;

namespace RimWorld;

public class LayoutSketch : Sketch
{
	public StructureLayout structureLayout;

	public ThingDef wall;

	public ThingDef wallStuff;

	public ThingDef door;

	public ThingDef doorStuff;

	public TerrainDef floor;

	public TerrainDef defaultAffordanceTerrain;

	public ThingDef wallLamp;

	protected virtual ThingDef WallThing => wall ?? ThingDefOf.Wall;

	protected virtual ThingDef DoorThing => door ?? ThingDefOf.Door;

	public virtual ThingDef WallLampThing => wallLamp;

	protected virtual TerrainDef FloorTerrain => floor ?? TerrainDefOf.PavedTile;

	public virtual TerrainDef DefaultAffordanceTerrain => defaultAffordanceTerrain;

	protected virtual ThingDef GetWallStuff(int roomId)
	{
		return wallStuff;
	}

	protected virtual ThingDef GetDoorStuff(int roomId)
	{
		return doorStuff;
	}

	public void FlushLayoutToSketch()
	{
		FlushLayoutToSketch(IntVec3.Zero);
	}

	public void FlushLayoutToSketch(IntVec3 at)
	{
		structureLayout.container = structureLayout.container.MovedBy(at);
		foreach (LayoutRoom room in structureLayout.Rooms)
		{
			for (int i = 0; i < room.rects.Count; i++)
			{
				room.rects[i] = room.rects[i].MovedBy(at);
			}
		}
		for (int j = structureLayout.container.minX; j <= structureLayout.container.maxX; j++)
		{
			for (int k = structureLayout.container.minZ; k <= structureLayout.container.maxZ; k++)
			{
				IntVec3 intVec = new IntVec3(j, 0, k);
				int roomIdAt = structureLayout.GetRoomIdAt(intVec);
				if (structureLayout.IsWallAt(intVec))
				{
					AddThing(WallThing, at + intVec, Rot4.North, GetWallStuff(roomIdAt));
				}
				if (structureLayout.IsFloorAt(intVec) || structureLayout.IsDoorAt(intVec) || structureLayout.IsWallAt(intVec))
				{
					AddTerrain(FloorTerrain, at + intVec);
				}
				if (structureLayout.IsDoorAt(intVec))
				{
					AddThing(DoorThing, at + intVec, Rot4.North, GetDoorStuff(roomIdAt));
				}
			}
		}
	}
}
