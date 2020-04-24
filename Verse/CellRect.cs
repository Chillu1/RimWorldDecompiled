using RimWorld.Planet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public struct CellRect : IEquatable<CellRect>, IEnumerable<IntVec3>, IEnumerable
	{
		public struct Enumerator : IEnumerator<IntVec3>, IEnumerator, IDisposable
		{
			private CellRect ir;

			private int x;

			private int z;

			public IntVec3 Current => new IntVec3(x, 0, z);

			object IEnumerator.Current => new IntVec3(x, 0, z);

			public Enumerator(CellRect ir)
			{
				this.ir = ir;
				x = ir.minX - 1;
				z = ir.minZ;
			}

			public bool MoveNext()
			{
				x++;
				if (x > ir.maxX)
				{
					x = ir.minX;
					z++;
				}
				if (z > ir.maxZ)
				{
					return false;
				}
				return true;
			}

			public void Reset()
			{
				x = ir.minX - 1;
				z = ir.minZ;
			}

			void IDisposable.Dispose()
			{
			}
		}

		[Obsolete("Do not use this anymore, CellRect has a struct-enumerator as substitute")]
		public struct CellRectIterator
		{
			private int maxX;

			private int minX;

			private int maxZ;

			private int x;

			private int z;

			public IntVec3 Current => new IntVec3(x, 0, z);

			public CellRectIterator(CellRect cr)
			{
				minX = cr.minX;
				maxX = cr.maxX;
				maxZ = cr.maxZ;
				x = cr.minX;
				z = cr.minZ;
			}

			public void MoveNext()
			{
				x++;
				if (x > maxX)
				{
					x = minX;
					z++;
				}
			}

			public bool Done()
			{
				return z > maxZ;
			}
		}

		public int minX;

		public int maxX;

		public int minZ;

		public int maxZ;

		public static CellRect Empty => new CellRect(0, 0, 0, 0);

		public bool IsEmpty
		{
			get
			{
				if (Width > 0)
				{
					return Height <= 0;
				}
				return true;
			}
		}

		public int Area => Width * Height;

		public int Width
		{
			get
			{
				if (minX > maxX)
				{
					return 0;
				}
				return maxX - minX + 1;
			}
			set
			{
				maxX = minX + Mathf.Max(value, 0) - 1;
			}
		}

		public int Height
		{
			get
			{
				if (minZ > maxZ)
				{
					return 0;
				}
				return maxZ - minZ + 1;
			}
			set
			{
				maxZ = minZ + Mathf.Max(value, 0) - 1;
			}
		}

		public IEnumerable<IntVec3> Corners
		{
			get
			{
				if (IsEmpty)
				{
					yield break;
				}
				yield return new IntVec3(minX, 0, minZ);
				if (Width > 1)
				{
					yield return new IntVec3(maxX, 0, minZ);
				}
				if (Height > 1)
				{
					yield return new IntVec3(minX, 0, maxZ);
					if (Width > 1)
					{
						yield return new IntVec3(maxX, 0, maxZ);
					}
				}
			}
		}

		public IntVec3 BottomLeft => new IntVec3(minX, 0, minZ);

		public IntVec3 TopRight => new IntVec3(maxX, 0, maxZ);

		public IntVec3 RandomCell => new IntVec3(Rand.RangeInclusive(minX, maxX), 0, Rand.RangeInclusive(minZ, maxZ));

		public IntVec3 CenterCell => new IntVec3(minX + Width / 2, 0, minZ + Height / 2);

		public Vector3 CenterVector3 => new Vector3((float)minX + (float)Width / 2f, 0f, (float)minZ + (float)Height / 2f);

		public Vector3 RandomVector3 => new Vector3(Rand.Range(minX, (float)maxX + 1f), 0f, Rand.Range(minZ, (float)maxZ + 1f));

		public IEnumerable<IntVec3> Cells
		{
			get
			{
				for (int z = minZ; z <= maxZ; z++)
				{
					for (int x = minX; x <= maxX; x++)
					{
						yield return new IntVec3(x, 0, z);
					}
				}
			}
		}

		public IEnumerable<IntVec2> Cells2D
		{
			get
			{
				for (int z = minZ; z <= maxZ; z++)
				{
					for (int x = minX; x <= maxX; x++)
					{
						yield return new IntVec2(x, z);
					}
				}
			}
		}

		public IEnumerable<IntVec3> EdgeCells
		{
			get
			{
				if (!IsEmpty)
				{
					int x4 = minX;
					int z4 = minZ;
					for (; x4 <= maxX; x4++)
					{
						yield return new IntVec3(x4, 0, z4);
					}
					x4--;
					for (z4++; z4 <= maxZ; z4++)
					{
						yield return new IntVec3(x4, 0, z4);
					}
					z4--;
					for (x4--; x4 >= minX; x4--)
					{
						yield return new IntVec3(x4, 0, z4);
					}
					x4++;
					for (z4--; z4 > minZ; z4--)
					{
						yield return new IntVec3(x4, 0, z4);
					}
				}
			}
		}

		public int EdgeCellsCount
		{
			get
			{
				if (Area == 0)
				{
					return 0;
				}
				if (Area == 1)
				{
					return 1;
				}
				return Width * 2 + (Height - 2) * 2;
			}
		}

		public IEnumerable<IntVec3> AdjacentCellsCardinal
		{
			get
			{
				if (!IsEmpty)
				{
					for (int x2 = minX; x2 <= maxX; x2++)
					{
						yield return new IntVec3(x2, 0, minZ - 1);
						yield return new IntVec3(x2, 0, maxZ + 1);
					}
					for (int x2 = minZ; x2 <= maxZ; x2++)
					{
						yield return new IntVec3(minX - 1, 0, x2);
						yield return new IntVec3(maxX + 1, 0, x2);
					}
				}
			}
		}

		[Obsolete("Use foreach on the cellrect instead")]
		public CellRectIterator GetIterator()
		{
			return new CellRectIterator(this);
		}

		public static bool operator ==(CellRect lhs, CellRect rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(CellRect lhs, CellRect rhs)
		{
			return !(lhs == rhs);
		}

		public CellRect(int minX, int minZ, int width, int height)
		{
			this.minX = minX;
			this.minZ = minZ;
			maxX = minX + width - 1;
			maxZ = minZ + height - 1;
		}

		public static CellRect WholeMap(Map map)
		{
			return new CellRect(0, 0, map.Size.x, map.Size.z);
		}

		public static CellRect FromLimits(int minX, int minZ, int maxX, int maxZ)
		{
			CellRect result = default(CellRect);
			result.minX = Mathf.Min(minX, maxX);
			result.minZ = Mathf.Min(minZ, maxZ);
			result.maxX = Mathf.Max(maxX, minX);
			result.maxZ = Mathf.Max(maxZ, minZ);
			return result;
		}

		public static CellRect FromLimits(IntVec3 first, IntVec3 second)
		{
			CellRect result = default(CellRect);
			result.minX = Mathf.Min(first.x, second.x);
			result.minZ = Mathf.Min(first.z, second.z);
			result.maxX = Mathf.Max(first.x, second.x);
			result.maxZ = Mathf.Max(first.z, second.z);
			return result;
		}

		public static CellRect CenteredOn(IntVec3 center, int radius)
		{
			CellRect result = default(CellRect);
			result.minX = center.x - radius;
			result.maxX = center.x + radius;
			result.minZ = center.z - radius;
			result.maxZ = center.z + radius;
			return result;
		}

		public static CellRect CenteredOn(IntVec3 center, int width, int height)
		{
			CellRect result = default(CellRect);
			result.minX = center.x - width / 2;
			result.minZ = center.z - height / 2;
			result.maxX = result.minX + width - 1;
			result.maxZ = result.minZ + height - 1;
			return result;
		}

		public static CellRect ViewRect(Map map)
		{
			if (Current.ProgramState != ProgramState.Playing || Find.CurrentMap != map || WorldRendererUtility.WorldRenderedNow)
			{
				return Empty;
			}
			return Find.CameraDriver.CurrentViewRect;
		}

		public static CellRect SingleCell(IntVec3 c)
		{
			return new CellRect(c.x, c.z, 1, 1);
		}

		public bool InBounds(Map map)
		{
			if (minX >= 0 && minZ >= 0 && maxX < map.Size.x)
			{
				return maxZ < map.Size.z;
			}
			return false;
		}

		public bool FullyContainedWithin(CellRect within)
		{
			CellRect rhs = this;
			rhs.ClipInsideRect(within);
			return this == rhs;
		}

		public bool Overlaps(CellRect other)
		{
			if (IsEmpty || other.IsEmpty)
			{
				return false;
			}
			if (minX <= other.maxX && maxX >= other.minX && maxZ >= other.minZ)
			{
				return minZ <= other.maxZ;
			}
			return false;
		}

		public bool IsOnEdge(IntVec3 c)
		{
			if ((c.x != minX || c.z < minZ || c.z > maxZ) && (c.x != maxX || c.z < minZ || c.z > maxZ) && (c.z != minZ || c.x < minX || c.x > maxX))
			{
				if (c.z == maxZ && c.x >= minX)
				{
					return c.x <= maxX;
				}
				return false;
			}
			return true;
		}

		public bool IsOnEdge(IntVec3 c, Rot4 rot)
		{
			if (rot == Rot4.West)
			{
				if (c.x == minX && c.z >= minZ)
				{
					return c.z <= maxZ;
				}
				return false;
			}
			if (rot == Rot4.East)
			{
				if (c.x == maxX && c.z >= minZ)
				{
					return c.z <= maxZ;
				}
				return false;
			}
			if (rot == Rot4.South)
			{
				if (c.z == minZ && c.x >= minX)
				{
					return c.x <= maxX;
				}
				return false;
			}
			if (c.z == maxZ && c.x >= minX)
			{
				return c.x <= maxX;
			}
			return false;
		}

		public bool IsOnEdge(IntVec3 c, int edgeWidth)
		{
			if (!Contains(c))
			{
				return false;
			}
			if (c.x >= minX + edgeWidth && c.z >= minZ + edgeWidth && c.x < maxX + 1 - edgeWidth)
			{
				return c.z >= maxZ + 1 - edgeWidth;
			}
			return true;
		}

		public bool IsCorner(IntVec3 c)
		{
			if ((c.x != minX || c.z != minZ) && (c.x != maxX || c.z != minZ) && (c.x != minX || c.z != maxZ))
			{
				if (c.x == maxX)
				{
					return c.z == maxZ;
				}
				return false;
			}
			return true;
		}

		public Rot4 GetClosestEdge(IntVec3 c)
		{
			int num = Mathf.Abs(c.x - minX);
			int num2 = Mathf.Abs(c.x - maxX);
			int num3 = Mathf.Abs(c.z - maxZ);
			int num4 = Mathf.Abs(c.z - minZ);
			return GenMath.MinBy(Rot4.West, num, Rot4.East, num2, Rot4.North, num3, Rot4.South, num4);
		}

		public CellRect ClipInsideMap(Map map)
		{
			if (minX < 0)
			{
				minX = 0;
			}
			if (minZ < 0)
			{
				minZ = 0;
			}
			if (maxX > map.Size.x - 1)
			{
				maxX = map.Size.x - 1;
			}
			if (maxZ > map.Size.z - 1)
			{
				maxZ = map.Size.z - 1;
			}
			return this;
		}

		public CellRect ClipInsideRect(CellRect otherRect)
		{
			if (minX < otherRect.minX)
			{
				minX = otherRect.minX;
			}
			if (maxX > otherRect.maxX)
			{
				maxX = otherRect.maxX;
			}
			if (minZ < otherRect.minZ)
			{
				minZ = otherRect.minZ;
			}
			if (maxZ > otherRect.maxZ)
			{
				maxZ = otherRect.maxZ;
			}
			return this;
		}

		public bool Contains(IntVec3 c)
		{
			if (c.x >= minX && c.x <= maxX && c.z >= minZ)
			{
				return c.z <= maxZ;
			}
			return false;
		}

		public float ClosestDistSquaredTo(IntVec3 c)
		{
			if (Contains(c))
			{
				return 0f;
			}
			if (c.x < minX)
			{
				if (c.z < minZ)
				{
					return (c - new IntVec3(minX, 0, minZ)).LengthHorizontalSquared;
				}
				if (c.z > maxZ)
				{
					return (c - new IntVec3(minX, 0, maxZ)).LengthHorizontalSquared;
				}
				return (minX - c.x) * (minX - c.x);
			}
			if (c.x > maxX)
			{
				if (c.z < minZ)
				{
					return (c - new IntVec3(maxX, 0, minZ)).LengthHorizontalSquared;
				}
				if (c.z > maxZ)
				{
					return (c - new IntVec3(maxX, 0, maxZ)).LengthHorizontalSquared;
				}
				return (c.x - maxX) * (c.x - maxX);
			}
			if (c.z < minZ)
			{
				return (minZ - c.z) * (minZ - c.z);
			}
			return (c.z - maxZ) * (c.z - maxZ);
		}

		public IntVec3 ClosestCellTo(IntVec3 c)
		{
			if (Contains(c))
			{
				return c;
			}
			if (c.x < minX)
			{
				if (c.z < minZ)
				{
					return new IntVec3(minX, 0, minZ);
				}
				if (c.z > maxZ)
				{
					return new IntVec3(minX, 0, maxZ);
				}
				return new IntVec3(minX, 0, c.z);
			}
			if (c.x > maxX)
			{
				if (c.z < minZ)
				{
					return new IntVec3(maxX, 0, minZ);
				}
				if (c.z > maxZ)
				{
					return new IntVec3(maxX, 0, maxZ);
				}
				return new IntVec3(maxX, 0, c.z);
			}
			if (c.z < minZ)
			{
				return new IntVec3(c.x, 0, minZ);
			}
			return new IntVec3(c.x, 0, maxZ);
		}

		public bool InNoBuildEdgeArea(Map map)
		{
			if (IsEmpty)
			{
				return false;
			}
			if (minX >= 10 && minZ >= 10 && maxX < map.Size.x - 10)
			{
				return maxZ >= map.Size.z - 10;
			}
			return true;
		}

		public IEnumerable<IntVec3> GetEdgeCells(Rot4 dir)
		{
			if (dir == Rot4.North)
			{
				for (int x4 = minX; x4 <= maxX; x4++)
				{
					yield return new IntVec3(x4, 0, maxZ);
				}
			}
			else if (dir == Rot4.South)
			{
				for (int x4 = minX; x4 <= maxX; x4++)
				{
					yield return new IntVec3(x4, 0, minZ);
				}
			}
			else if (dir == Rot4.West)
			{
				for (int x4 = minZ; x4 <= maxZ; x4++)
				{
					yield return new IntVec3(minX, 0, x4);
				}
			}
			else if (dir == Rot4.East)
			{
				for (int x4 = minZ; x4 <= maxZ; x4++)
				{
					yield return new IntVec3(maxX, 0, x4);
				}
			}
		}

		public bool TryFindRandomInnerRectTouchingEdge(IntVec2 size, out CellRect rect, Predicate<CellRect> predicate = null)
		{
			if (Width < size.x || Height < size.z)
			{
				rect = Empty;
				return false;
			}
			if (size.x <= 0 || size.z <= 0 || IsEmpty)
			{
				rect = Empty;
				return false;
			}
			CellRect cellRect = this;
			cellRect.maxX -= size.x - 1;
			cellRect.maxZ -= size.z - 1;
			if (cellRect.EdgeCells.Where(delegate(IntVec3 x)
			{
				if (predicate == null)
				{
					return true;
				}
				CellRect obj = new CellRect(x.x, x.z, size.x, size.z);
				return predicate(obj);
			}).TryRandomElement(out IntVec3 result))
			{
				rect = new CellRect(result.x, result.z, size.x, size.z);
				return true;
			}
			rect = Empty;
			return false;
		}

		public bool TryFindRandomInnerRect(IntVec2 size, out CellRect rect, Predicate<CellRect> predicate = null)
		{
			if (Width < size.x || Height < size.z)
			{
				rect = Empty;
				return false;
			}
			if (size.x <= 0 || size.z <= 0 || IsEmpty)
			{
				rect = Empty;
				return false;
			}
			CellRect cellRect = this;
			cellRect.maxX -= size.x - 1;
			cellRect.maxZ -= size.z - 1;
			if (cellRect.Cells.Where(delegate(IntVec3 x)
			{
				if (predicate == null)
				{
					return true;
				}
				CellRect obj = new CellRect(x.x, x.z, size.x, size.z);
				return predicate(obj);
			}).TryRandomElement(out IntVec3 result))
			{
				rect = new CellRect(result.x, result.z, size.x, size.z);
				return true;
			}
			rect = Empty;
			return false;
		}

		public CellRect ExpandedBy(int dist)
		{
			CellRect result = this;
			result.minX -= dist;
			result.minZ -= dist;
			result.maxX += dist;
			result.maxZ += dist;
			return result;
		}

		public CellRect ContractedBy(int dist)
		{
			return ExpandedBy(-dist);
		}

		public CellRect MovedBy(IntVec2 offset)
		{
			return MovedBy(offset.ToIntVec3);
		}

		public CellRect MovedBy(IntVec3 offset)
		{
			CellRect result = this;
			result.minX += offset.x;
			result.minZ += offset.z;
			result.maxX += offset.x;
			result.maxZ += offset.z;
			return result;
		}

		public int IndexOf(IntVec3 location)
		{
			return location.x - minX + (location.z - minZ) * Width;
		}

		public void DebugDraw()
		{
			float y = AltitudeLayer.MetaOverlays.AltitudeFor();
			Vector3 vector = new Vector3(minX, y, minZ);
			Vector3 vector2 = new Vector3(minX, y, maxZ + 1);
			Vector3 vector3 = new Vector3(maxX + 1, y, maxZ + 1);
			Vector3 vector4 = new Vector3(maxX + 1, y, minZ);
			GenDraw.DrawLineBetween(vector, vector2);
			GenDraw.DrawLineBetween(vector2, vector3);
			GenDraw.DrawLineBetween(vector3, vector4);
			GenDraw.DrawLineBetween(vector4, vector);
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		IEnumerator<IntVec3> IEnumerable<IntVec3>.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public override string ToString()
		{
			return "(" + minX + "," + minZ + "," + maxX + "," + maxZ + ")";
		}

		public static CellRect FromString(string str)
		{
			str = str.TrimStart('(');
			str = str.TrimEnd(')');
			string[] array = str.Split(',');
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			int num = Convert.ToInt32(array[0], invariantCulture);
			int num2 = Convert.ToInt32(array[1], invariantCulture);
			int num3 = Convert.ToInt32(array[2], invariantCulture);
			int num4 = Convert.ToInt32(array[3], invariantCulture);
			return new CellRect(num, num2, num3 - num + 1, num4 - num2 + 1);
		}

		public override int GetHashCode()
		{
			return Gen.HashCombineInt(Gen.HashCombineInt(Gen.HashCombineInt(Gen.HashCombineInt(0, minX), maxX), minZ), maxZ);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is CellRect))
			{
				return false;
			}
			return Equals((CellRect)obj);
		}

		public bool Equals(CellRect other)
		{
			if (minX == other.minX && maxX == other.maxX && minZ == other.minZ)
			{
				return maxZ == other.maxZ;
			}
			return false;
		}
	}
}
