using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RoomRoleWorker_PrisonBarracks : RoomRoleWorker
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
					num++;
				}
				else
				{
					num2++;
				}
			}
		}
		if (num2 + num <= 1)
		{
			return 0f;
		}
		return (float)num2 * 100100f + (float)num * 50001f;
	}

	public override float GetScoreDeltaIfBuildingPlaced(Room room, ThingDef buildingDef)
	{
		if (buildingDef.building == null || !buildingDef.thingClass.IsAssignableFrom(typeof(Building_Bed)) || !buildingDef.building.bed_humanlike)
		{
			return 0f;
		}
		if (room.Role != null)
		{
			RoomRoleWorker worker = room.Role.Worker;
			if (worker is RoomRoleWorker_PrisonCell || worker is RoomRoleWorker_PrisonBarracks)
			{
				if (!buildingDef.building.bed_defaultMedical)
				{
					return 100100f;
				}
				return 50001f;
			}
		}
		return 0f;
	}
}
