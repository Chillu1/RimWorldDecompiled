using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RoomRoleWorker_Workshop : RoomRoleWorker
{
	public override float GetScore(Room room)
	{
		int num = 0;
		List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
		for (int i = 0; i < containedAndAdjacentThings.Count; i++)
		{
			if (containedAndAdjacentThings[i].def.building?.workTableRoomRole == RoomRoleDefOf.Workshop)
			{
				num++;
			}
		}
		return 27f * (float)num;
	}

	public override float GetScoreDeltaIfBuildingPlaced(Room room, ThingDef buildingDef)
	{
		if (buildingDef.building?.workTableRoomRole == RoomRoleDefOf.Workshop)
		{
			return 27f;
		}
		return 0f;
	}
}
