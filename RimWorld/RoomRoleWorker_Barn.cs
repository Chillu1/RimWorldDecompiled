using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RoomRoleWorker_Barn : RoomRoleWorker
{
	public override float GetScore(Room room)
	{
		int num = 0;
		List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
		for (int i = 0; i < containedAndAdjacentThings.Count; i++)
		{
			if (containedAndAdjacentThings[i] is Building_Bed building_Bed && !building_Bed.def.building.bed_humanlike)
			{
				num++;
			}
		}
		return (float)num * 7.6f;
	}

	public override float GetScoreDeltaIfBuildingPlaced(Room room, ThingDef buildingDef)
	{
		if (buildingDef.building == null || !buildingDef.building.bed_humanlike)
		{
			return 0f;
		}
		return 7.6f;
	}
}
