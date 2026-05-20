using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RoomRoleWorker_CeremonialChamber : RoomRoleWorker
{
	public override float GetScore(Room room)
	{
		float num = 0f;
		List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
		for (int i = 0; i < containedAndAdjacentThings.Count; i++)
		{
			if (containedAndAdjacentThings[i].def == ThingDefOf.PsychicRitualSpot)
			{
				num += 200f;
			}
		}
		return num;
	}

	public override float GetScoreDeltaIfBuildingPlaced(Room room, ThingDef buildingDef)
	{
		if (buildingDef == ThingDefOf.PsychicRitualSpot)
		{
			return 200f;
		}
		return 0f;
	}
}
