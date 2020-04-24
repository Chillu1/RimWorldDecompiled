using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RoomRoleWorker_ThroneRoom : RoomRoleWorker
	{
		public static string Validate(Room room)
		{
			if (room == null || room.OutdoorsForWork)
			{
				return "ThroneMustBePlacedInside".Translate();
			}
			return null;
		}

		public override float GetScore(Room room)
		{
			List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
			bool flag = false;
			for (int i = 0; i < containedAndAdjacentThings.Count; i++)
			{
				if (containedAndAdjacentThings[i] is Building_Throne)
				{
					flag = true;
					break;
				}
			}
			return (flag && Validate(room) == null) ? 10000 : 0;
		}
	}
}
