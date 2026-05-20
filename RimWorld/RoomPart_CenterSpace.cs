using Verse;

namespace RimWorld;

public class RoomPart_CenterSpace : RoomPartWorker
{
	private const int Size = 4;

	public RoomPart_CenterSpace(RoomPartDef def)
		: base(def)
	{
	}

	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float threatPoints)
	{
		ThingDef wall = room.sketch.layoutSketch.wall;
		ThingDef wallStuff = room.sketch.layoutSketch.wallStuff;
		TerrainDef floor = room.sketch.layoutSketch.floor;
		CellRect cellRect = room.rects[0];
		IntVec3 center = cellRect.CenterCell - new IntVec3(1, 0, 1);
		if (cellRect.Size.x < 8 || cellRect.Size.z < 8)
		{
			return;
		}
		CellRect cellRect2 = center.RectAbout(new IntVec2(4, 4));
		foreach (IntVec3 edgeCell in cellRect2.EdgeCells)
		{
			map.terrainGrid.SetTerrain(edgeCell, floor);
			GenSpawn.Spawn(ThingMaker.MakeThing(wall, wallStuff), edgeCell, map);
		}
		foreach (IntVec3 cell in cellRect2.ContractedBy(1).Cells)
		{
			TerrainDef newTerr = (map.Biome.inVacuum ? TerrainDefOf.Space : TerrainDefOf.PackedDirt);
			map.terrainGrid.SetTerrain(cell, newTerr);
			map.roofGrid.SetRoof(cell, null);
		}
	}
}
