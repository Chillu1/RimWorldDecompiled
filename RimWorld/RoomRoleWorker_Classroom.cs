using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RoomRoleWorker_Classroom : RoomRoleWorker
{
	private const int MinBabyBeds = 2;

	public override float GetScore(Room room)
	{
		if (!ModsConfig.BiotechActive)
		{
			return 0f;
		}
		List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
		int num = 0;
		for (int i = 0; i < containedAndAdjacentThings.Count; i++)
		{
			Thing thing = containedAndAdjacentThings[i];
			if (thing.def.IsBed && thing.def.building.bed_humanlike)
			{
				return 0f;
			}
			if (thing.def == ThingDefOf.SchoolDesk || thing.def == ThingDefOf.Blackboard)
			{
				num++;
			}
		}
		return (float)num * 8f;
	}

	public override float GetScoreDeltaIfBuildingPlaced(Room room, ThingDef buildingDef)
	{
		if (!ModsConfig.BiotechActive)
		{
			return 0f;
		}
		if (buildingDef == ThingDefOf.SchoolDesk || buildingDef == ThingDefOf.Blackboard)
		{
			return 8f;
		}
		return 0f;
	}
}
