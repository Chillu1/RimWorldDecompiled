using Verse;

namespace RimWorld;

public class RoomContents_GrayBox : RoomContentsWorker
{
	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
		base.FillRoom(map, room, faction, threatPoints);
		TrySpawnBoxInRoom(map, room, out var _);
	}

	public static bool TrySpawnBoxInRoom(Map map, LayoutRoom room, out Building_Crate spawned)
	{
		spawned = null;
		if (!room.TryGetRandomCellInRoom(ThingDefOf.GrayBox, map, out var cell, null, 2, 1))
		{
			return false;
		}
		spawned = SpawnBoxInRoom(cell, map);
		return true;
	}

	public static Building_Crate SpawnBoxInRoom(IntVec3 cell, Map map, ThingSetMakerDef rewardMaker = null, bool addRewards = true)
	{
		return RoomGenUtility.SpawnCrate(ThingDefOf.GrayBox, cell, map, rewardMaker ?? ThingSetMakerDefOf.Reward_GrayBox, addRewards);
	}
}
