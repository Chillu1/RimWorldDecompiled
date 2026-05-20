using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse;

public static class GenSight
{
	private static readonly List<IntVec3> tmpCells = new List<IntVec3>();

	private static List<IntVec3> tmpGoodCells = new List<IntVec3>();

	private static List<IntVec3> tmpBadCells = new List<IntVec3>();

	private static List<IntVec3> tmpViewCells = new List<IntVec3>();

	private static readonly Color InViewCol = new Color(1f, 1f, 1f, 0.7f);

	public static bool LineOfSight(IntVec3 start, IntVec3 end, Map map, bool skipFirstCell = false, Func<IntVec3, bool> validator = null, int halfXOffset = 0, int halfZOffset = 0)
	{
		if (!start.InBounds(map) || !end.InBounds(map))
		{
			return false;
		}
		bool flag = ((start.x != end.x) ? (start.x < end.x) : (start.z < end.z));
		int num = Mathf.Abs(end.x - start.x);
		int num2 = Mathf.Abs(end.z - start.z);
		int num3 = start.x;
		int num4 = start.z;
		int num5 = 1 + num + num2;
		int num6 = ((end.x > start.x) ? 1 : (-1));
		int num7 = ((end.z > start.z) ? 1 : (-1));
		num *= 4;
		num2 *= 4;
		num += halfXOffset * 2;
		num2 += halfZOffset * 2;
		int num8 = num / 2 - num2 / 2;
		IntVec3 intVec = default(IntVec3);
		while (num5 > 1)
		{
			intVec.x = num3;
			intVec.z = num4;
			if (!skipFirstCell || intVec != start)
			{
				if (!intVec.CanBeSeenOverFast(map))
				{
					return false;
				}
				if (validator != null && !validator(intVec))
				{
					return false;
				}
			}
			if (num8 > 0 || (num8 == 0 && flag))
			{
				num3 += num6;
				num8 -= num2;
			}
			else
			{
				num4 += num7;
				num8 += num;
			}
			num5--;
		}
		return true;
	}

	public static bool LineOfSight(IntVec3 start, IntVec3 end, Map map, CellRect startRect, CellRect endRect, Func<IntVec3, bool> validator = null, bool forLeaning = false)
	{
		if (!start.InBounds(map) || !end.InBounds(map))
		{
			return false;
		}
		bool flag = ((start.x != end.x) ? (start.x < end.x) : (start.z < end.z));
		int num = Mathf.Abs(end.x - start.x);
		int num2 = Mathf.Abs(end.z - start.z);
		int num3 = start.x;
		int num4 = start.z;
		int num5 = 1 + num + num2;
		int num6 = ((end.x > start.x) ? 1 : (-1));
		int num7 = ((end.z > start.z) ? 1 : (-1));
		int num8 = num - num2;
		num *= 2;
		num2 *= 2;
		IntVec3 intVec = default(IntVec3);
		while (num5 > 1)
		{
			intVec.x = num3;
			intVec.z = num4;
			if (endRect.Contains(intVec))
			{
				return true;
			}
			if (!startRect.Contains(intVec))
			{
				if (!intVec.CanBeSeenOverFast(map))
				{
					return false;
				}
				if (validator != null && !validator(intVec))
				{
					return false;
				}
			}
			if (num8 > 0 || (num8 == 0 && flag))
			{
				num3 += num6;
				num8 -= num2;
			}
			else
			{
				num4 += num7;
				num8 += num;
			}
			num5--;
		}
		return true;
	}

	public static bool LineOfSight(IntVec3 start, IntVec3 end, Map map)
	{
		return LineOfSight(start, end, map, CellRect.SingleCell(start), CellRect.SingleCell(end));
	}

	public static IEnumerable<IntVec3> PointsOnLineOfSight(IntVec3 start, IntVec3 end)
	{
		bool sideOnEqual = ((start.x != end.x) ? (start.x < end.x) : (start.z < end.z));
		int dx = Mathf.Abs(end.x - start.x);
		int dz = Mathf.Abs(end.z - start.z);
		int x = start.x;
		int z = start.z;
		int n = 1 + dx + dz;
		int x_inc = ((end.x > start.x) ? 1 : (-1));
		int z_inc = ((end.z > start.z) ? 1 : (-1));
		int error = dx - dz;
		dx *= 2;
		dz *= 2;
		IntVec3 c = default(IntVec3);
		while (n > 0)
		{
			c.x = x;
			c.z = z;
			yield return c;
			if (error > 0 || (error == 0 && sideOnEqual))
			{
				x += x_inc;
				error -= dz;
			}
			else
			{
				z += z_inc;
				error += dx;
			}
			int num = n - 1;
			n = num;
		}
	}

	public static void PointsOnLineOfSight(IntVec3 start, IntVec3 end, Action<IntVec3> visitor)
	{
		bool flag = ((start.x != end.x) ? (start.x < end.x) : (start.z < end.z));
		int num = Mathf.Abs(end.x - start.x);
		int num2 = Mathf.Abs(end.z - start.z);
		int num3 = start.x;
		int num4 = start.z;
		int num5 = 1 + num + num2;
		int num6 = ((end.x > start.x) ? 1 : (-1));
		int num7 = ((end.z > start.z) ? 1 : (-1));
		int num8 = num - num2;
		num *= 2;
		num2 *= 2;
		IntVec3 obj = default(IntVec3);
		while (num5 > 1)
		{
			obj.x = num3;
			obj.z = num4;
			visitor(obj);
			if (num8 > 0 || (num8 == 0 && flag))
			{
				num3 += num6;
				num8 -= num2;
			}
			else
			{
				num4 += num7;
				num8 += num;
			}
			num5--;
		}
	}

	public static IntVec3 LastPointOnLineOfSight(IntVec3 start, IntVec3 end, Func<IntVec3, bool> validator, bool skipFirstCell = false)
	{
		foreach (IntVec3 item in PointsOnLineOfSight(start, end))
		{
			if (!skipFirstCell || !(item == start))
			{
				if (item == end)
				{
					return end;
				}
				if (!validator(item))
				{
					return item;
				}
			}
		}
		return IntVec3.Invalid;
	}

	public static bool LineOfSightToEdges(IntVec3 start, IntVec3 end, Map map, bool skipFirstCell = false, Func<IntVec3, bool> validator = null)
	{
		if (LineOfSight(start, end, map, skipFirstCell, validator))
		{
			return true;
		}
		int num = (start * 2).DistanceToSquared(end * 2);
		for (int i = 0; i < 4; i++)
		{
			if ((start * 2).DistanceToSquared(end * 2 + GenAdj.CardinalDirections[i]) <= num && LineOfSight(start, end, map, skipFirstCell, validator, GenAdj.CardinalDirections[i].x, GenAdj.CardinalDirections[i].z))
			{
				return true;
			}
		}
		return false;
	}

	public static bool LineOfSightToThing(IntVec3 start, Thing t, Map map, bool skipFirstCell = false, Func<IntVec3, bool> validator = null)
	{
		if (t.def.size == IntVec2.One)
		{
			return LineOfSight(start, t.Position, map);
		}
		foreach (IntVec3 item in t.OccupiedRect())
		{
			if (LineOfSight(start, item, map, skipFirstCell, validator))
			{
				return true;
			}
		}
		return false;
	}

	public static List<IntVec3> BresenhamCellsBetween(IntVec3 a, IntVec3 b)
	{
		return BresenhamCellsBetween(a.x, a.z, b.x, b.z);
	}

	public static List<IntVec3> BresenhamCellsBetween(int x0, int y0, int x1, int y1)
	{
		tmpCells.Clear();
		int num = Mathf.Abs(x1 - x0);
		int num2 = ((x0 < x1) ? 1 : (-1));
		int num3 = -Mathf.Abs(y1 - y0);
		int num4 = ((y0 < y1) ? 1 : (-1));
		int num5 = num + num3;
		int num6 = 1000;
		while (true)
		{
			tmpCells.Add(new IntVec3(x0, 0, y0));
			if (x0 == x1 && y0 == y1)
			{
				break;
			}
			int num7 = 2 * num5;
			if (num7 >= num3)
			{
				num5 += num3;
				x0 += num2;
			}
			if (num7 <= num)
			{
				num5 += num;
				y0 += num4;
			}
			num6--;
			if (num6 <= 0)
			{
				Log.Error("BresenhamCellsBetween exceeded iterations limit of 1000.");
				break;
			}
		}
		return tmpCells;
	}

	public static void DebugDrawFOVSymmetry_Update()
	{
		Pawn pawn = Find.Selector.SelectedPawns.FirstOrDefault();
		if (pawn == null || !pawn.Spawned)
		{
			return;
		}
		foreach (IntVec3 item in GenRadial.RadialCellsAround(pawn.Position, 25f, useCenter: true))
		{
			if (item.InBounds(Find.CurrentMap))
			{
				if (LineOfSight(pawn.Position, item, Find.CurrentMap, skipFirstCell: true))
				{
					tmpViewCells.Add(item);
				}
				if (ShootLeanUtility.CellCanSeeCell(pawn.Position, item, pawn.Map) != ShootLeanUtility.CellCanSeeCell(item, pawn.Position, pawn.Map))
				{
					tmpBadCells.Add(item);
				}
				else
				{
					tmpGoodCells.Add(item);
				}
			}
		}
		GenDraw.DrawFieldEdges(tmpGoodCells, Color.cyan);
		GenDraw.DrawFieldEdges(tmpBadCells, Color.magenta);
		GenDraw.DrawFieldEdges(tmpViewCells, InViewCol);
		tmpGoodCells.Clear();
		tmpBadCells.Clear();
		tmpViewCells.Clear();
	}
}
