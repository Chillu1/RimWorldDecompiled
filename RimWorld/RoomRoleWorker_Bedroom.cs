using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RoomRoleWorker_Bedroom : RoomRoleWorker
{
	private static List<Building_Bed> tmpBeds = new List<Building_Bed>();

	private static List<Pawn> children = new List<Pawn>();

	private static List<Pawn> adults = new List<Pawn>();

	public override float GetScore(Room room)
	{
		tmpBeds.Clear();
		List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
		int num = 0;
		for (int i = 0; i < containedAndAdjacentThings.Count; i++)
		{
			if (containedAndAdjacentThings[i] is Building_Bed building_Bed && building_Bed.def.building.bed_humanlike && building_Bed.def.building.bed_countsForBedroomOrBarracks)
			{
				if (building_Bed.Medical || building_Bed.ForPrisoners)
				{
					tmpBeds.Clear();
					return 0f;
				}
				num++;
				tmpBeds.Add(building_Bed);
			}
		}
		if (num == 0)
		{
			tmpBeds.Clear();
			return 0f;
		}
		bool num2 = IsBedroom(tmpBeds);
		tmpBeds.Clear();
		if (!num2)
		{
			return 0f;
		}
		return 100000f;
	}

	public static bool IsBedroom(List<Building_Bed> beds)
	{
		children.Clear();
		adults.Clear();
		bool result = IsBedroomHelper(beds);
		children.Clear();
		adults.Clear();
		return result;
	}

	private static bool IsBedroomHelper(List<Building_Bed> beds)
	{
		List<Pawn> list = null;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		foreach (Building_Bed bed in beds)
		{
			List<Pawn> ownersForReading = bed.OwnersForReading;
			if (ownersForReading.NullOrEmpty() && bed.def.building.bed_emptyCountsForBarracks)
			{
				num++;
				continue;
			}
			if (ownersForReading.Count > 0)
			{
				num2++;
			}
			bool flag = false;
			foreach (Pawn item in ownersForReading)
			{
				if (item.DevelopmentalStage.Juvenile())
				{
					children.Add(item);
					continue;
				}
				adults.Add(item);
				if (list == null)
				{
					list = item.GetLoveCluster();
				}
				if (!list.Contains(item))
				{
					flag = true;
				}
			}
			if (flag)
			{
				num3++;
			}
		}
		if (num == 1 && num2 == 0)
		{
			return true;
		}
		if (num == 0 && num2 == 1)
		{
			return true;
		}
		if (num > 0)
		{
			return false;
		}
		if (adults.NullOrEmpty())
		{
			return true;
		}
		if (num3 > 0)
		{
			return false;
		}
		foreach (Pawn child in children)
		{
			Pawn mother = child.GetMother();
			Pawn father = child.GetFather();
			if (!adults.Any((Pawn adult) => adult == mother || adult == father))
			{
				return false;
			}
		}
		return true;
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
			if (worker is RoomRoleWorker_Barracks || worker is RoomRoleWorker_Hospital || worker is RoomRoleWorker_PrisonCell || worker is RoomRoleWorker_PrisonBarracks)
			{
				return 0f;
			}
		}
		if (room.Role != null && room.Role.Worker is RoomRoleWorker_Bedroom)
		{
			return -100000f;
		}
		return 100000f;
	}
}
