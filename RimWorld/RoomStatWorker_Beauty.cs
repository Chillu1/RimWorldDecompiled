using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RoomStatWorker_Beauty : RoomStatWorker
	{
		private static readonly SimpleCurve CellCountCurve = new SimpleCurve
		{
			new CurvePoint(0f, 20f),
			new CurvePoint(40f, 40f),
			new CurvePoint(100000f, 100000f)
		};

		private static List<Thing> countedThings = new List<Thing>();

		private static List<IntVec3> countedAdjCells = new List<IntVec3>();

		public override float GetScore(Room room)
		{
			float num = 0f;
			int num2 = 0;
			countedThings.Clear();
			foreach (IntVec3 cell in room.Cells)
			{
				num += BeautyUtility.CellBeauty(cell, room.Map, countedThings);
				num2++;
			}
			countedAdjCells.Clear();
			List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
			for (int i = 0; i < containedAndAdjacentThings.Count; i++)
			{
				Thing thing = containedAndAdjacentThings[i];
				if (thing.GetRoom() != room && !countedAdjCells.Contains(thing.Position))
				{
					num += BeautyUtility.CellBeauty(thing.Position, room.Map, countedThings);
					countedAdjCells.Add(thing.Position);
				}
			}
			countedThings.Clear();
			if (num2 == 0)
			{
				return 0f;
			}
			return num / CellCountCurve.Evaluate(num2);
		}
	}
}
