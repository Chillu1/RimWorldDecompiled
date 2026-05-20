using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RoomRoleWorker_Barracks : RoomRoleWorker
{
	private static List<Building_Bed> tmpBeds = new List<Building_Bed>();

	public override float GetScore(Room room)
	{
		tmpBeds.Clear();
		int num = 0;
		List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
		for (int i = 0; i < containedAndAdjacentThings.Count; i++)
		{
			if (containedAndAdjacentThings[i] is Building_Bed building_Bed && building_Bed.def.building.bed_humanlike && building_Bed.def.building.bed_countsForBedroomOrBarracks)
			{
				if (building_Bed.ForPrisoners)
				{
					tmpBeds.Clear();
					return 0f;
				}
				tmpBeds.Add(building_Bed);
				if (!building_Bed.Medical)
				{
					num++;
				}
			}
		}
		bool num2 = RoomRoleWorker_Bedroom.IsBedroom(tmpBeds);
		tmpBeds.Clear();
		if (num2)
		{
			return 0f;
		}
		return (float)num * 100100f;
	}

	public override float GetScoreDeltaIfBuildingPlaced(Room room, ThingDef buildingDef)
	{
		if (buildingDef.building == null || !buildingDef.thingClass.IsAssignableFrom(typeof(Building_Bed)) || !buildingDef.building.bed_humanlike || !buildingDef.building.bed_countsForBedroomOrBarracks)
		{
			return 0f;
		}
		if (room.Role != null)
		{
			RoomRoleWorker worker = room.Role.Worker;
			if (!(worker is RoomRoleWorker_Bedroom) && !(worker is RoomRoleWorker_Barracks) && !(worker is RoomRoleWorker_Hospital))
			{
				return 0f;
			}
		}
		return 100100f;
	}
}
