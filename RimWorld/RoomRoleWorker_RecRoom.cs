using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RoomRoleWorker_RecRoom : RoomRoleWorker
	{
		public override float GetScore(Room room)
		{
			int num = 0;
			List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
			for (int i = 0; i < containedAndAdjacentThings.Count; i++)
			{
				Thing thing = containedAndAdjacentThings[i];
				if (thing.def.category != ThingCategory.Building)
				{
					continue;
				}
				List<JoyGiverDef> allDefsListForReading = DefDatabase<JoyGiverDef>.AllDefsListForReading;
				for (int j = 0; j < allDefsListForReading.Count; j++)
				{
					if (allDefsListForReading[j].thingDefs != null && allDefsListForReading[j].thingDefs.Contains(thing.def))
					{
						num++;
						break;
					}
				}
			}
			return (float)num * 5f;
		}
	}
}
