using System.Linq;
using Verse;

namespace RimWorld;

public class RoomContents_Terraformer : RoomContentsWorker
{
	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
		RoomGenUtility.TryPlaceInRandomCorner(map, room, ThingDefOf.HunterDroneTrap, faction);
		if (Rand.Bool)
		{
			RoomGenUtility.TryPlaceInRandomCorner(map, room, ThingDefOf.HunterDroneTrap, faction);
		}
		CellRect largest = (from r in room.rects
			where r.Width >= 6 && r.Height >= 6
			orderby r.Area descending
			select r).FirstOrDefault();
		SpawnTerraformer(map, largest);
		RemoveRoof(map, room);
		base.FillRoom(map, room, faction, threatPoints);
	}

	private void SpawnTerraformer(Map map, CellRect largest)
	{
		GenSpawn.Spawn(ThingDefOf.AncientTerraformer, largest.CenterCell, map);
	}

	private void RemoveRoof(Map map, LayoutRoom room)
	{
		foreach (IntVec3 cell in room.Cells)
		{
			map.roofGrid.SetRoof(cell, null);
		}
	}
}
