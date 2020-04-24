using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class WindTurbineUtility
	{
		public static IEnumerable<IntVec3> CalculateWindCells(IntVec3 center, Rot4 rot, IntVec2 size)
		{
			CellRect rectA = default(CellRect);
			CellRect rectB = default(CellRect);
			int num = 0;
			int num2;
			int num3;
			if (rot == Rot4.North || rot == Rot4.East)
			{
				num2 = 9;
				num3 = 5;
			}
			else
			{
				num2 = 5;
				num3 = 9;
				num = -1;
			}
			if (rot.IsHorizontal)
			{
				rectA.minX = center.x + 2 + num;
				rectA.maxX = center.x + 2 + num2 + num;
				rectB.minX = center.x - 1 - num3 + num;
				rectB.maxX = center.x - 1 + num;
				rectB.minZ = (rectA.minZ = center.z - 3);
				rectB.maxZ = (rectA.maxZ = center.z + 3);
			}
			else
			{
				rectA.minZ = center.z + 2 + num;
				rectA.maxZ = center.z + 2 + num2 + num;
				rectB.minZ = center.z - 1 - num3 + num;
				rectB.maxZ = center.z - 1 + num;
				rectB.minX = (rectA.minX = center.x - 3);
				rectB.maxX = (rectA.maxX = center.x + 3);
			}
			for (int z2 = rectA.minZ; z2 <= rectA.maxZ; z2++)
			{
				for (int x = rectA.minX; x <= rectA.maxX; x++)
				{
					yield return new IntVec3(x, 0, z2);
				}
			}
			for (int z2 = rectB.minZ; z2 <= rectB.maxZ; z2++)
			{
				for (int x = rectB.minX; x <= rectB.maxX; x++)
				{
					yield return new IntVec3(x, 0, z2);
				}
			}
		}
	}
}
