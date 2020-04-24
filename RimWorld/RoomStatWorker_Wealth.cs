using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RoomStatWorker_Wealth : RoomStatWorker
	{
		public override float GetScore(Room room)
		{
			float num = 0f;
			List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
			for (int i = 0; i < containedAndAdjacentThings.Count; i++)
			{
				Thing thing = containedAndAdjacentThings[i];
				if (thing.def.category == ThingCategory.Building || thing.def.category == ThingCategory.Plant)
				{
					num += (float)thing.stackCount * thing.MarketValue;
				}
			}
			foreach (IntVec3 cell in room.Cells)
			{
				num += cell.GetTerrain(room.Map).GetStatValueAbstract(StatDefOf.MarketValue);
			}
			return num;
		}
	}
}
