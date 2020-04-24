using UnityEngine;
using Verse;

namespace RimWorld
{
	public class RoomStatWorker_Space : RoomStatWorker
	{
		public override float GetScore(Room room)
		{
			if (room.PsychologicallyOutdoors)
			{
				return 350f;
			}
			float num = 0f;
			foreach (IntVec3 cell in room.Cells)
			{
				if (cell.Standable(room.Map))
				{
					num += 1.4f;
				}
				else if (cell.Walkable(room.Map))
				{
					num += 0.5f;
				}
			}
			return Mathf.Min(num, 350f);
		}
	}
}
