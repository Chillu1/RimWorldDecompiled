using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public static class GenSight
	{
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
				if (!skipFirstCell || !(intVec == start))
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

		public static bool LineOfSight(IntVec3 start, IntVec3 end, Map map, CellRect startRect, CellRect endRect, Func<IntVec3, bool> validator = null)
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

		public static IEnumerable<IntVec3> PointsOnLineOfSight(IntVec3 start, IntVec3 end)
		{
			bool sideOnEqual = ((start.x != end.x) ? (start.x < end.x) : (start.z < end.z));
			int dx2 = Mathf.Abs(end.x - start.x);
			int dz2 = Mathf.Abs(end.z - start.z);
			int x = start.x;
			int z = start.z;
			int i = 1 + dx2 + dz2;
			int x_inc = ((end.x > start.x) ? 1 : (-1));
			int z_inc = ((end.z > start.z) ? 1 : (-1));
			int error = dx2 - dz2;
			dx2 *= 2;
			dz2 *= 2;
			IntVec3 c = default(IntVec3);
			while (i > 1)
			{
				c.x = x;
				c.z = z;
				yield return c;
				if (error > 0 || (error == 0 && sideOnEqual))
				{
					x += x_inc;
					error -= dz2;
				}
				else
				{
					z += z_inc;
					error += dx2;
				}
				int num = i - 1;
				i = num;
			}
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
	}
}
