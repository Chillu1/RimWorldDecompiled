using System.Collections.Generic;
using Verse;

namespace RimWorld;

public static class BeautyUtility
{
	public static List<IntVec3> beautyRelevantCells = new List<IntVec3>();

	private static HashSet<Room> visibleRooms = new HashSet<Room>();

	public static readonly int SampleNumCells_Beauty = GenRadial.NumCellsInRadius(8.9f);

	private const float PollutedTerrainBeautyOffset = -1f;

	private static HashSet<Thing> tempCountedThings = new HashSet<Thing>();

	public static float AverageBeautyPerceptible(IntVec3 root, Map map)
	{
		if (!root.IsValid || !root.InBounds(map))
		{
			return 0f;
		}
		tempCountedThings.Clear();
		float num = 0f;
		int num2 = 0;
		using (new ProfilerBlock("FillBeautyRelevantCells"))
		{
			FillBeautyRelevantCells(root, map);
		}
		for (int i = 0; i < beautyRelevantCells.Count; i++)
		{
			using (new ProfilerBlock("CellBeauty"))
			{
				num += CellBeauty(beautyRelevantCells[i], map, tempCountedThings);
			}
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
		if (room.IsDoorway)
		{
			foreach (Region neighbor in room.FirstRegion.Neighbors)
			{
				visibleRooms.Add(neighbor.Room);
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

	public static float CellBeauty(IntVec3 c, Map map, HashSet<Thing> countedThings = null)
	{
		float num = 0f;
		float num2 = 0f;
		bool flag = false;
		bool flag2 = c.GetRoom(map)?.PsychologicallyOutdoors ?? true;
		bool flag3 = map.roofGrid.Roofed(c);
		List<Thing> list = map.thingGrid.ThingsListAt(c);
		for (int i = 0; i < list.Count; i++)
		{
			Thing thing = list[i];
			if (!BeautyRelevant(thing.def.category) || (countedThings != null && countedThings.Contains(thing)))
			{
				continue;
			}
			countedThings?.Add(thing);
			SlotGroup slotGroup = thing.GetSlotGroup();
			if (!thing.def.EverHaulable || slotGroup == null || slotGroup.parent == thing || !slotGroup.parent.IgnoreStoredThingsBeauty)
			{
				float num3 = thing.GetBeauty(flag2);
				if (thing.def.filth != null && flag3)
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
		TerrainDef terrainDef = map.terrainGrid.TerrainAt(c);
		if (ModsConfig.BiotechActive && !terrainDef.BuildableByPlayer && c.IsPolluted(map))
		{
			num += -1f;
		}
		if (flag2 && terrainDef.StatBaseDefined(StatDefOf.BeautyOutdoors))
		{
			return num + terrainDef.GetStatValueAbstract(StatDefOf.BeautyOutdoors);
		}
		return num + terrainDef.GetStatValueAbstract(StatDefOf.Beauty);
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
