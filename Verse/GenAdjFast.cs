using System;
using System.Collections.Generic;

namespace Verse
{
	public static class GenAdjFast
	{
		private static List<IntVec3> resultList = new List<IntVec3>();

		private static bool working = false;

		public static List<IntVec3> AdjacentCells8Way(LocalTargetInfo pack)
		{
			if (pack.HasThing)
			{
				return AdjacentCells8Way((Thing)pack);
			}
			return AdjacentCells8Way((IntVec3)pack);
		}

		public static List<IntVec3> AdjacentCells8Way(IntVec3 root)
		{
			if (working)
			{
				throw new InvalidOperationException("GenAdjFast is already working.");
			}
			resultList.Clear();
			working = true;
			for (int i = 0; i < 8; i++)
			{
				resultList.Add(root + GenAdj.AdjacentCells[i]);
			}
			working = false;
			return resultList;
		}

		private static List<IntVec3> AdjacentCells8Way(Thing t)
		{
			return AdjacentCells8Way(t.Position, t.Rotation, t.def.size);
		}

		public static List<IntVec3> AdjacentCells8Way(IntVec3 thingCenter, Rot4 thingRot, IntVec2 thingSize)
		{
			if (thingSize.x == 1 && thingSize.z == 1)
			{
				return AdjacentCells8Way(thingCenter);
			}
			if (working)
			{
				throw new InvalidOperationException("GenAdjFast is already working.");
			}
			resultList.Clear();
			working = true;
			GenAdj.AdjustForRotation(ref thingCenter, ref thingSize, thingRot);
			int num = thingCenter.x - (thingSize.x - 1) / 2 - 1;
			int num2 = num + thingSize.x + 1;
			int num3 = thingCenter.z - (thingSize.z - 1) / 2 - 1;
			int num4 = num3 + thingSize.z + 1;
			IntVec3 item = new IntVec3(num - 1, 0, num3);
			do
			{
				item.x++;
				resultList.Add(item);
			}
			while (item.x < num2);
			do
			{
				item.z++;
				resultList.Add(item);
			}
			while (item.z < num4);
			do
			{
				item.x--;
				resultList.Add(item);
			}
			while (item.x > num);
			do
			{
				item.z--;
				resultList.Add(item);
			}
			while (item.z > num3 + 1);
			working = false;
			return resultList;
		}

		public static List<IntVec3> AdjacentCellsCardinal(LocalTargetInfo pack)
		{
			if (pack.HasThing)
			{
				return AdjacentCellsCardinal((Thing)pack);
			}
			return AdjacentCellsCardinal((IntVec3)pack);
		}

		public static List<IntVec3> AdjacentCellsCardinal(IntVec3 root)
		{
			if (working)
			{
				throw new InvalidOperationException("GenAdjFast is already working.");
			}
			resultList.Clear();
			working = true;
			for (int i = 0; i < 4; i++)
			{
				resultList.Add(root + GenAdj.CardinalDirections[i]);
			}
			working = false;
			return resultList;
		}

		private static List<IntVec3> AdjacentCellsCardinal(Thing t)
		{
			return AdjacentCellsCardinal(t.Position, t.Rotation, t.def.size);
		}

		public static List<IntVec3> AdjacentCellsCardinal(IntVec3 thingCenter, Rot4 thingRot, IntVec2 thingSize)
		{
			if (thingSize.x == 1 && thingSize.z == 1)
			{
				return AdjacentCellsCardinal(thingCenter);
			}
			if (working)
			{
				throw new InvalidOperationException("GenAdjFast is already working.");
			}
			resultList.Clear();
			working = true;
			GenAdj.AdjustForRotation(ref thingCenter, ref thingSize, thingRot);
			int num = thingCenter.x - (thingSize.x - 1) / 2 - 1;
			int num2 = num + thingSize.x + 1;
			int num3 = thingCenter.z - (thingSize.z - 1) / 2 - 1;
			int num4 = num3 + thingSize.z + 1;
			IntVec3 item = new IntVec3(num, 0, num3);
			do
			{
				item.x++;
				resultList.Add(item);
			}
			while (item.x < num2 - 1);
			item.x++;
			do
			{
				item.z++;
				resultList.Add(item);
			}
			while (item.z < num4 - 1);
			item.z++;
			do
			{
				item.x--;
				resultList.Add(item);
			}
			while (item.x > num + 1);
			item.x--;
			do
			{
				item.z--;
				resultList.Add(item);
			}
			while (item.z > num3 + 1);
			working = false;
			return resultList;
		}

		public static void AdjacentThings8Way(Thing thing, List<Thing> outThings)
		{
			outThings.Clear();
			if (!thing.Spawned)
			{
				return;
			}
			Map map = thing.Map;
			List<IntVec3> list = AdjacentCells8Way(thing);
			for (int i = 0; i < list.Count; i++)
			{
				List<Thing> thingList = list[i].GetThingList(map);
				for (int j = 0; j < thingList.Count; j++)
				{
					if (!outThings.Contains(thingList[j]))
					{
						outThings.Add(thingList[j]);
					}
				}
			}
		}
	}
}
