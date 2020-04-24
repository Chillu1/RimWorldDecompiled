using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RoomRoleWorker_Workshop : RoomRoleWorker
	{
		public override float GetScore(Room room)
		{
			int num = 0;
			List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
			for (int i = 0; i < containedAndAdjacentThings.Count; i++)
			{
				if (containedAndAdjacentThings[i] is Building_WorkTable && containedAndAdjacentThings[i].def.designationCategory == DesignationCategoryDefOf.Production)
				{
					num++;
				}
			}
			return 13.5f * (float)num;
		}
	}
}
