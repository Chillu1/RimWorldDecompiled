using Verse;

namespace RimWorld;

public class RoomContents_Checkpoint_Corridor : RoomContents_Corridor
{
	private const float CheckpointsPerHundredCells = 8f;

	private const int MinTilesPerCheckpoint = 20;

	protected override TerrainDef StripCorridorTerrain => TerrainDefOf.AncientTile;

	protected override ThingDef DoorThing => ThingDefOf.AncientBlastDoor;

	protected override IntRange ExteriorDoorCount => IntRange.Between(4, 4);

	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
		SpawnCheckpoints(map, room, faction);
		base.FillRoom(map, room, faction, threatPoints);
	}

	private static void SpawnCheckpoints(Map map, LayoutRoom room, Faction faction)
	{
		foreach (CellRect rect2 in room.rects)
		{
			Rot4 rot = ((rect2.Width > rect2.Height) ? Rot4.East : Rot4.North);
			Rot4 rot2 = rot.Rotated(RotationDirection.Clockwise);
			CellRect cellRect = rect2.ContractedBy(1);
			int sideLength = cellRect.GetSideLength(rot2);
			float chance = (float)sideLength / 100f * 8f / (float)sideLength;
			IntVec3 centerCellOnEdge = cellRect.GetCenterCellOnEdge(rot.Opposite);
			while (cellRect.Contains(centerCellOnEdge))
			{
				CellRect rect = centerCellOnEdge.RectAbout(new IntVec2(3, 3));
				if (Rand.Chance(chance) && CanSpawnCheckpoint(map, rect, rot2))
				{
					SpawnCheckpoint(map, rect, rot2, faction);
					centerCellOnEdge += rot.FacingCell * 20;
				}
				else
				{
					centerCellOnEdge += rot.FacingCell;
				}
			}
		}
	}

	private static bool CanSpawnCheckpoint(Map map, CellRect rect, Rot4 rot)
	{
		foreach (IntVec3 item in rect)
		{
			if (item.GetEdifice(map) != null)
			{
				return false;
			}
		}
		CellRect cellRect = rect.ExpandedBy(1);
		for (int i = 0; i < 2; i++)
		{
			Rot4 dir = rot;
			if (i == 1)
			{
				dir = dir.Opposite;
			}
			foreach (IntVec3 edgeCell in cellRect.GetEdgeCells(dir))
			{
				if (edgeCell.GetEdifice(map) == null)
				{
					return false;
				}
			}
		}
		foreach (IntVec3 item2 in rect.AdjacentCellsCardinal)
		{
			if (item2.GetDoor(map) != null)
			{
				return false;
			}
		}
		return true;
	}

	private static void SpawnCheckpoint(Map map, CellRect rect, Rot4 rot, Faction faction)
	{
		foreach (IntVec3 corner in rect.Corners)
		{
			GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.Barricade, ThingDefOf.Steel), corner, map);
		}
		for (int i = 0; i < 2; i++)
		{
			Rot4 rot2 = rot;
			if (i == 1)
			{
				rot2 = rot.Opposite;
			}
			IntVec3 centerCellOnEdge = rect.GetCenterCellOnEdge(rot2);
			Thing thing = ThingMaker.MakeThing(Rand.Bool ? ThingDefOf.Turret_AncientArmoredTurret : ThingDefOf.AncientSecurityTurret);
			thing.SetFaction(faction ?? Faction.OfAncientsHostile);
			GenSpawn.Spawn(thing, centerCellOnEdge, map);
		}
	}
}
