using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class StructureLayoutDef : LayoutDef
{
	public ThingDef wallDef;

	public ThingDef wallStuffDef;

	public ThingDef doorDef;

	public ThingDef doorStuffDef;

	public ThingDef wallLampDef;

	public float wallLampChancePerPosition = 0.6f;

	public ThingDef exteriorDoorDef;

	public ThingDef exteriorDoorStuffDef;

	public TerrainDef terrainDef;

	public TerrainDef defaultAffordanceTerrainDef;

	public LayoutRoomDef importantRoomDef;

	public float roomToExteriorDoorRatio = 0.33f;

	public bool spawnDoors = true;

	public bool clearDoorFaction = true;

	public bool canConnectRoomsExternally = true;

	public float areaPrunePercent = 0.3f;

	public bool canDisconnectRooms = true;

	public bool ensureOneDoorUnlocked;

	public LayoutRoomDef corridorDef;

	public IntRange wallDamageRange = IntRange.Invalid;

	public CorridorShape corridorShapes = CorridorShape.All;

	public TerrainDef surroundingTerrainDef;

	public TerrainDef surroundingScatterTerrainDef;

	public IntRange surroundingScatterRange = new IntRange(2, 6);

	public List<ScatteredPrefabs> scatteredPrefabs = new List<ScatteredPrefabs>();
}
