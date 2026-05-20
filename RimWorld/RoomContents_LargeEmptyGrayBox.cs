using Verse;

namespace RimWorld;

public class RoomContents_LargeEmptyGrayBox : RoomContents_LargeGrayBox
{
	protected override int MaxCrates => 5;

	protected override void SpawnBox(IntVec3 cell, Map map)
	{
		RoomContents_GrayBox.SpawnBoxInRoom(cell, map, null, addRewards: false);
	}
}
