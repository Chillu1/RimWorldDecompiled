using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RoomRoleWorker_DiningRoom : RoomRoleWorker
{
	public override float GetScore(Room room)
	{
		int num = 0;
		List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
		for (int i = 0; i < containedAndAdjacentThings.Count; i++)
		{
			Thing thing = containedAndAdjacentThings[i];
			if (thing.def.category == ThingCategory.Building && thing.def.surfaceType == SurfaceType.Eat)
			{
				num++;
			}
		}
		return (float)num * 12f;
	}

	public override float GetScoreDeltaIfBuildingPlaced(Room room, ThingDef buildingDef)
	{
		if (buildingDef.category == ThingCategory.Building && buildingDef.surfaceType == SurfaceType.Eat)
		{
			return 12f;
		}
		return 0f;
	}
}
