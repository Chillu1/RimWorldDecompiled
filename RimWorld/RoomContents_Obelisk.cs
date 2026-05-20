using Verse;

namespace RimWorld;

public class RoomContents_Obelisk : RoomContentsWorker
{
	private const float WallRadius = 8.9f;

	private const float MetalRadius = 3.9f;

	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
		base.FillRoom(map, room, faction, threatPoints);
		CellRect cellRect = room.rects[0];
		foreach (IntVec3 cell in cellRect.Cells)
		{
			if (cell.GetFirstBuilding(map) == null)
			{
				if (!(cell - cellRect.CenterCell).IsCardinal && cellRect.CenterCell.DistanceTo(cell) >= 8.9f)
				{
					GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.GrayWall, ThingDefOf.LabyrinthMatter), cell, map);
				}
				if (cellRect.CenterCell.DistanceTo(cell) < 3.9f)
				{
					map.terrainGrid.SetTerrain(cell, TerrainDefOf.Voidmetal);
				}
			}
		}
		Building building = (Building)GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.WarpedObelisk_Labyrinth), cellRect.CenterCell, map);
		map.GetComponent<LabyrinthMapComponent>().labyrinthObelisk = building;
		string signalTag = "ThingDiscovered" + Find.UniqueIDsManager.GetNextSignalTagID();
		SignalAction_Letter signalAction_Letter = (SignalAction_Letter)ThingMaker.MakeThing(ThingDefOf.SignalAction_Letter);
		signalAction_Letter.signalTag = signalTag;
		signalAction_Letter.letterDef = LetterDefOf.PositiveEvent;
		signalAction_Letter.letterLabelKey = "LetterLabelObeliskDiscovered";
		signalAction_Letter.letterMessageKey = "LetterObeliskDiscovered";
		GenSpawn.Spawn(signalAction_Letter, building.Position, map);
		room.SpawnRectTriggersForAction(signalAction_Letter, map);
		RoomContents_GrayBox.SpawnBoxInRoom(cellRect.CenterCell + Rot4.East.FacingCell * 3, map);
		RoomContents_GrayBox.SpawnBoxInRoom(cellRect.CenterCell + Rot4.West.FacingCell * 3, map);
	}
}
