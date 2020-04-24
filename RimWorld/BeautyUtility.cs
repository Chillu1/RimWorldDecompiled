using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class BeautyUtility
	{
		public static List<IntVec3> beautyRelevantCells = new List<IntVec3>();

		private static List<Room> visibleRooms = new List<Room>();

		public static readonly int SampleNumCells_Beauty = GenRadial.NumCellsInRadius(8.9f);

		private static List<Thing> tempCountedThings = new List<Thing>();

		public static float AverageBeautyPerceptible(IntVec3 root, Map map)
		{
			if (!root.IsValid || !root.InBounds(map))
			{
				return 0f;
			}
			tempCountedThings.Clear();
			float num = 0f;
			int num2 = 0;
			FillBeautyRelevantCells(root, map);
			for (int i = 0; i < beautyRelevantCells.Count; i++)
			{
				num += CellBeauty(beautyRelevantCells[i], map, tempCountedThings);
				num2++;
			}
			tempCountedThings.Clear();
			if (num2 == 0)
			{
				return 0f;
			}
			return num / (float)num2;
		}

		public static void FillBeautyRelevantCells(IntVec3 root, Map map)
		{
			beautyRelevantCells.Clear();
			Room room = root.GetRoom(map);
			if (room == null)
			{
				return;
			}
			visibleRooms.Clear();
			visibleRooms.Add(room);
			if (room.Regions.Count == 1 && room.Regions[0].type == RegionType.Portal)
			{
				foreach (Region neighbor in room.Regions[0].Neighbors)
				{
					if (!visibleRooms.Contains(neighbor.Room))
					{
						visibleRooms.Add(neighbor.Room);
					}
				}
			}
			for (int i = 0; i < SampleNumCells_Beauty; i++)
			{
				IntVec3 intVec = root + GenRadial.RadialPattern[i];
				if (!intVec.InBounds(map) || intVec.Fogged(map))
				{
					continue;
				}
				Room room2 = intVec.GetRoom(map);
				if (!visibleRooms.Contains(room2))
				{
					bool flag = false;
					for (int j = 0; j < 8; j++)
					{
						IntVec3 loc = intVec + GenAdj.AdjacentCells[j];
						if (visibleRooms.Contains(loc.GetRoom(map)))
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						continue;
					}
				}
				beautyRelevantCells.Add(intVec);
			}
			visibleRooms.Clear();
		}

		public static float CellBeauty(IntVec3 c, Map map, List<Thing> countedThings = null)
		{
			float num = 0f;
			float num2 = 0f;
			bool flag = false;
			List<Thing> list = map.thingGrid.ThingsListAt(c);
			for (int i = 0; i < list.Count; i++)
			{
				Thing thing = list[i];
				if (!BeautyRelevant(thing.def.category))
				{
					continue;
				}
				if (countedThings != null)
				{
					if (countedThings.Contains(thing))
					{
						continue;
					}
					countedThings.Add(thing);
				}
				SlotGroup slotGroup = thing.GetSlotGroup();
				if (slotGroup == null || slotGroup.parent == thing || !slotGroup.parent.IgnoreStoredThingsBeauty)
				{
					float num3 = thing.GetStatValue(StatDefOf.Beauty);
					if (thing is Filth && !map.roofGrid.Roofed(c))
					{
						num3 *= 0.3f;
					}
					if (thing.def.Fillage == FillCategory.Full)
					{
						flag = true;
						num2 += num3;
					}
					else
					{
						num += num3;
					}
				}
			}
			if (flag)
			{
				return num2;
			}
			return num + map.terrainGrid.TerrainAt(c).GetStatValueAbstract(StatDefOf.Beauty);
		}

		public static bool BeautyRelevant(ThingCategory cat)
		{
			if (cat != ThingCategory.Building && cat != ThingCategory.Item && cat != ThingCategory.Plant)
			{
				return cat == ThingCategory.Filth;
			}
			return true;
		}
	}
}
