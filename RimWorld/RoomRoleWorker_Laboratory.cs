using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RoomRoleWorker_Laboratory : RoomRoleWorker
{
	public override float GetScore(Room room)
	{
		int num = 0;
		List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
		for (int i = 0; i < containedAndAdjacentThings.Count; i++)
		{
			_ = containedAndAdjacentThings[i];
			if (containedAndAdjacentThings[i].def.building?.workTableRoomRole == RoomRoleDefOf.Laboratory)
			{
				num++;
			}
		}
		return 60f * (float)num;
	}

	public override float GetScoreDeltaIfBuildingPlaced(Room room, ThingDef buildingDef)
	{
		if (buildingDef.building?.workTableRoomRole == RoomRoleDefOf.Laboratory)
		{
			return 60f;
		}
		return 0f;
	}
}
