using Verse;

namespace RimWorld;

public class RoomContents_LargeGrayBox : RoomContents_GrayBox
{
	private const int NormalCrates = 1;

	private const float ChanceEmpty = 0.5f;

	private int placedCrates;

	protected virtual int MaxCrates { get; } = 20;

	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
		base.FillRoom(map, room, faction, threatPoints);
		placedCrates = 0;
		for (int i = 0; i < MaxCrates; i++)
		{
			if (room.TryGetRandomCellInRoom(ThingDefOf.GrayBox, map, out var cell, null, 1, 1))
			{
				SpawnBox(cell, map);
			}
		}
	}

	protected virtual void SpawnBox(IntVec3 cell, Map map)
	{
		ThingSetMakerDef rewardMaker = ThingSetMakerDefOf.Reward_GrayBoxLowReward;
		bool addRewards = true;
		if (placedCrates < 1)
		{
			rewardMaker = ThingSetMakerDefOf.Reward_GrayBox;
		}
		else if (Rand.Chance(0.5f))
		{
			addRewards = false;
		}
		placedCrates++;
		RoomContents_GrayBox.SpawnBoxInRoom(cell, map, rewardMaker, addRewards);
	}
}
