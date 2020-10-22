using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class RoomOutlinesGenerator
	{
		private const int MinFreeRoomCellsToDivide = 32;

		private const int MinAllowedRoomWidthAndHeight = 2;

		public static List<RoomOutline> GenerateRoomOutlines(CellRect initialRect, Map map, int divisionsCount, int finalRoomsCount, int maxRoomCells, int minTotalRoomsNonWallCellsCount)
		{
			int num = 0;
			List<RoomOutline> list;
			do
			{
				list = GenerateRoomOutlines(initialRect, map, divisionsCount, finalRoomsCount, maxRoomCells);
				int num2 = 0;
				for (int i = 0; i < list.Count; i++)
				{
					num2 += list[i].CellsCountIgnoringWalls;
				}
				if (num2 >= minTotalRoomsNonWallCellsCount)
				{
					return list;
				}
				num++;
			}
			while (num <= 15);
			return list;
		}

		public static List<RoomOutline> GenerateRoomOutlines(CellRect initialRect, Map map, int divisionsCount, int finalRoomsCount, int maxRoomCells)
		{
			List<RoomOutline> list = new List<RoomOutline>();
			list.Add(new RoomOutline(initialRect));
			for (int i = 0; i < divisionsCount; i++)
			{
				if (!list.Where((RoomOutline x) => x.CellsCountIgnoringWalls >= 32).TryRandomElementByWeight((RoomOutline x) => Mathf.Max(x.rect.Width, x.rect.Height), out var result))
				{
					break;
				}
				bool flag = result.rect.Height > result.rect.Width;
				if ((!flag || result.rect.Height > 6) && (flag || result.rect.Width > 6))
				{
					Split(result, list, flag);
				}
			}
			while (list.Any((RoomOutline x) => x.CellsCountIgnoringWalls > maxRoomCells))
			{
				RoomOutline roomOutline = list.Where((RoomOutline x) => x.CellsCountIgnoringWalls > maxRoomCells).RandomElement();
				bool horizontalWall = roomOutline.rect.Height > roomOutline.rect.Width;
				Split(roomOutline, list, horizontalWall);
			}
			while (list.Count > finalRoomsCount)
			{
				list.Remove(list.RandomElement());
			}
			return list;
		}

		private static void Split(RoomOutline room, List<RoomOutline> allRooms, bool horizontalWall)
		{
			allRooms.Remove(room);
			if (horizontalWall)
			{
				int z = room.rect.CenterCell.z;
				allRooms.Add(new RoomOutline(new CellRect(room.rect.minX, room.rect.minZ, room.rect.Width, z - room.rect.minZ + 1)));
				allRooms.Add(new RoomOutline(new CellRect(room.rect.minX, z, room.rect.Width, room.rect.maxZ - z + 1)));
			}
			else
			{
				int x = room.rect.CenterCell.x;
				allRooms.Add(new RoomOutline(new CellRect(room.rect.minX, room.rect.minZ, x - room.rect.minX + 1, room.rect.Height)));
				allRooms.Add(new RoomOutline(new CellRect(x, room.rect.minZ, room.rect.maxX - x + 1, room.rect.Height)));
			}
		}
	}
}
