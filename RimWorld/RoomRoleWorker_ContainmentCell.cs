using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RoomRoleWorker_ContainmentCell : RoomRoleWorker
{
	public override float GetScore(Room room)
	{
		float num = 0f;
		List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
		for (int i = 0; i < containedAndAdjacentThings.Count; i++)
		{
			Thing thing = containedAndAdjacentThings[i];
			if (ThingRequestGroup.EntityHolder.Includes(thing.def))
			{
				num += 100f;
			}
			else if (thing.def == ThingDefOf.Electroharvester || thing.def == ThingDefOf.ElectricInhibitor || thing.def == ThingDefOf.BioferriteHarvester)
			{
				num += 50f;
			}
		}
		return num;
	}

	public override float GetScoreDeltaIfBuildingPlaced(Room room, ThingDef buildingDef)
	{
		if (ThingRequestGroup.EntityHolder.Includes(buildingDef))
		{
			return 100f;
		}
		if (buildingDef == ThingDefOf.Electroharvester || buildingDef == ThingDefOf.ElectricInhibitor || buildingDef == ThingDefOf.BioferriteHarvester)
		{
			return 50f;
		}
		return 0f;
	}
}
