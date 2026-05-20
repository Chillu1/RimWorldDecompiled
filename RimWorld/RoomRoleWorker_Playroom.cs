using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RoomRoleWorker_Playroom : RoomRoleWorker
{
	public override float GetScore(Room room)
	{
		if (!ModsConfig.BiotechActive)
		{
			return 0f;
		}
		int num = 0;
		List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
		for (int i = 0; i < containedAndAdjacentThings.Count; i++)
		{
			Thing thing = containedAndAdjacentThings[i];
			if (thing is Building_Bed building_Bed && building_Bed.def.building.bed_humanlike)
			{
				return 0f;
			}
			if (thing.GetStatValue(StatDefOf.BabyPlayGainFactor) > 1f)
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
		if (buildingDef.GetStatValueAbstract(StatDefOf.BabyPlayGainFactor) > 1f)
		{
			return 8f;
		}
		return 0f;
	}
}
