using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RoomRoleWorker_DeathrestChamber : RoomRoleWorker
{
	public override float GetScore(Room room)
	{
		float num = 0f;
		List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
		for (int i = 0; i < containedAndAdjacentThings.Count; i++)
		{
			Thing thing = containedAndAdjacentThings[i];
			if (thing.def == ThingDefOf.DeathrestCasket)
			{
				num += 1f;
			}
			else if (thing.TryGetComp<CompDeathrestBindable>() != null)
			{
				num += 0.5f;
			}
		}
		return num * 100f;
	}

	public override float GetScoreDeltaIfBuildingPlaced(Room room, ThingDef buildingDef)
	{
		if (buildingDef == ThingDefOf.DeathrestCasket)
		{
			return 100f;
		}
		if (buildingDef.HasComp(typeof(CompDeathrestBindable)))
		{
			return 50f;
		}
		return 0f;
	}
}
