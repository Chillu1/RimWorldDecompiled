using Verse;

namespace RimWorld;

public class RoomContents_Stabilizer : RoomContentsWorker
{
	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
		base.FillRoom(map, room, faction, threatPoints);
		CellRect cellRect = CellRect.CenteredOn(room.Boundary.CenterCell, 5, 5);
		foreach (IntVec3 item in cellRect)
		{
			map.terrainGrid.SetTerrain(item, TerrainDefOf.AncientTile);
		}
		foreach (IntVec3 corner in cellRect.Corners)
		{
			GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.MechPylon), corner, map);
		}
		GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.CerebrexStabilizer), cellRect.CenterCell, map);
	}
}
