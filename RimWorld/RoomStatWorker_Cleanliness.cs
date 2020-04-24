using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RoomStatWorker_Cleanliness : RoomStatWorker
	{
		public override float GetScore(Room room)
		{
			float num = 0f;
			List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
			for (int i = 0; i < containedAndAdjacentThings.Count; i++)
			{
				Thing thing = containedAndAdjacentThings[i];
				if (thing.def.category == ThingCategory.Building || thing.def.category == ThingCategory.Item || thing.def.category == ThingCategory.Filth || thing.def.category == ThingCategory.Plant)
				{
					num += (float)thing.stackCount * thing.GetStatValue(StatDefOf.Cleanliness);
				}
			}
			foreach (IntVec3 cell in room.Cells)
			{
				num += cell.GetTerrain(room.Map).GetStatValueAbstract(StatDefOf.Cleanliness);
			}
			return num / (float)room.CellCount;
		}
	}
}
