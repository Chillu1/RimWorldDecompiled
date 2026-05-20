using Verse;

namespace RimWorld;

public class RoomPart_InsectHive : RoomPartWorker
{
	protected virtual IntRange HivesCountRange => new IntRange(1, 3);

	public RoomPart_InsectHive(RoomPartDef def)
		: base(def)
	{
	}

	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float threatPoints)
	{
		if (!room.TryGetRandomCellInRoom(map, out var cell, 3))
		{
			cell = room.rects[0].CenterCell;
		}
		int randomInRange = HivesCountRange.RandomInRange;
		HiveUtility.SpawnHives(cell, map, randomInRange, 5f, WipeMode.VanishOrMoveAside, spawnInsectsImmediately: false, canSpawnHives: false, canSpawnInsects: true, dormant: true);
	}
}
