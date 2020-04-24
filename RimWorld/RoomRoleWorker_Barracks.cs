using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RoomRoleWorker_Barracks : RoomRoleWorker
	{
		public override float GetScore(Room room)
		{
			int num = 0;
			int num2 = 0;
			List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
			for (int i = 0; i < containedAndAdjacentThings.Count; i++)
			{
				Building_Bed building_Bed = containedAndAdjacentThings[i] as Building_Bed;
				if (building_Bed != null && building_Bed.def.building.bed_humanlike)
				{
					if (building_Bed.ForPrisoners)
					{
						return 0f;
					}
					num++;
					if (!building_Bed.Medical)
					{
						num2++;
					}
				}
			}
			if (num <= 1)
			{
				return 0f;
			}
			return (float)num2 * 100100f;
		}
	}
}
