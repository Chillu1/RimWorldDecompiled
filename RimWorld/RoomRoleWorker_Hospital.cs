using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RoomRoleWorker_Hospital : RoomRoleWorker
{
	public override float GetScore(Room room)
	{
		bool flag = false;
		List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
		for (int i = 0; i < containedAndAdjacentThings.Count; i++)
		{
			if (containedAndAdjacentThings[i] is Building_Bed building_Bed && building_Bed.def.building.bed_humanlike)
			{
				if (building_Bed.ForPrisoners)
				{
					return 0f;
				}
				if (building_Bed.Medical)
				{
					flag = true;
					break;
				}
			}
		}
		if (!flag)
		{
			return 0f;
		}
		return 100000f;
	}

	public override float GetScoreDeltaIfBuildingPlaced(Room room, ThingDef buildingDef)
	{
		if (room.Role != null && room.Role.Worker is RoomRoleWorker_Hospital)
		{
			return 0f;
		}
		if (buildingDef.building != null && buildingDef.building.bed_humanlike && buildingDef.building.bed_defaultMedical)
		{
			return 100000f;
		}
		return 0f;
	}
}
