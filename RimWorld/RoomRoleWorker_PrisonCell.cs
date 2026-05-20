using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RoomRoleWorker_PrisonCell : RoomRoleWorker
{
	public override float GetScore(Room room)
	{
		int num = 0;
		int num2 = 0;
		List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
		for (int i = 0; i < containedAndAdjacentThings.Count; i++)
		{
			if (containedAndAdjacentThings[i] is Building_Bed building_Bed && building_Bed.def.building.bed_humanlike)
			{
				if (!building_Bed.ForPrisoners)
				{
					return 0f;
				}
				if (building_Bed.Medical)
				{
					num2++;
				}
				else
				{
					num++;
				}
			}
		}
		if (num == 1)
		{
			return 170000f;
		}
		if (num2 == 1)
		{
			return 100000f;
		}
		return 0f;
	}

	public override float GetScoreDeltaIfBuildingPlaced(Room room, ThingDef buildingDef)
	{
		if (room.Role == null || !buildingDef.thingClass.IsAssignableFrom(typeof(Building_Bed)) || !(room.Role.Worker is RoomRoleWorker_PrisonCell))
		{
			return 0f;
		}
		if (buildingDef.building == null || !buildingDef.building.bed_humanlike)
		{
			return 0f;
		}
		return -170000f;
	}
}
