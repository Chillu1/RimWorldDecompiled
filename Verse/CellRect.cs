using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;

namespace Verse;

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

	public int minX;

	public int maxX;

	public int minZ;

	public int maxZ;

	private static readonly HashSet<IntVec3> tmpUnvistedCells = new HashSet<IntVec3>();

	public static CellRect Empty => new CellRect(0, 0, 0, 0);

	public readonly bool IsEmpty
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

	public readonly int Area => Width * Height;

	public int Width
	{
		readonly get
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
		readonly get
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

	public IntVec2 Size => new IntVec2(Width, Height);

	public readonly IEnumerable<IntVec3> Corners
	{
		get
		{
			if (IsEmpty)
			{
				yield break;
			}
			yield return new IntVec3(minX, 0, minZ);
			if (Height > 1)
			{
				yield return new IntVec3(minX, 0, maxZ);
				if (Width > 1)
				{
					yield return new IntVec3(maxX, 0, maxZ);
				}
			}
			if (Width > 1)
			{
				yield return new IntVec3(maxX, 0, minZ);
			}
		}
	}

	public IntVec3 Min
	{
		get
		{
			return new IntVec3(minX, 0, minZ);
		}
		set
		{
			minX = value.x;
			minZ = value.z;
		}
	}

	public IntVec3 Max
	{
		get
		{
			return new IntVec3(maxX, 0, maxZ);
		}
		set
		{
			maxX = value.x;
			maxZ = value.z;
		}
	}

	public IntVec3 RandomCell => new IntVec3(Rand.RangeInclusive(minX, maxX), 0, Rand.RangeInclusive(minZ, maxZ));

	public IntVec3 CenterCell => new IntVec3(minX + Width / 2, 0, minZ + Height / 2);

	public Vector3 CenterVector3 => new Vector3((float)minX + (float)Width / 2f, 0f, (float)minZ + (float)Height / 2f);

	public Vector3 RandomVector3 => new Vector3(Rand.Range(minX, (float)maxX + 1f), 0f, Rand.Range(minZ, (float)maxZ + 1f));

	public readonly IEnumerable<IntVec3> Cells
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

	public readonly IEnumerable<IntVec2> Cells2D
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

	public readonly IEnumerable<IntVec3> EdgeCells
	{
		get
		{
			if (!IsEmpty)
			{
				int x = minX;
				int z = minZ;
				for (; x <= maxX; x++)
				{
					yield return new IntVec3(x, 0, z);
				}
				x--;
				for (z++; z <= maxZ; z++)
				{
					yield return new IntVec3(x, 0, z);
				}
				z--;
				for (x--; x >= minX; x--)
				{
					yield return new IntVec3(x, 0, z);
				}
				x++;
				for (z--; z > minZ; z--)
				{
					yield return new IntVec3(x, 0, z);
				}
			}
		}
	}

	public IEnumerable<IntVec3> EdgeCellsNoCorners
	{
		get
		{
			foreach (IntVec3 edgeCell in EdgeCells)
			{
				if (!IsCorner(edgeCell))
				{
					yield return edgeCell;
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
				for (int x = minX; x <= maxX; x++)
				{
					yield return new IntVec3(x, 0, minZ - 1);
					yield return new IntVec3(x, 0, maxZ + 1);
				}
				for (int x = minZ; x <= maxZ; x++)
				{
					yield return new IntVec3(minX - 1, 0, x);
					yield return new IntVec3(maxX + 1, 0, x);
				}
			}
		}
	}

	public IEnumerable<IntVec3> AdjacentCells
	{
		get
		{
			if (IsEmpty)
			{
				yield break;
			}
			foreach (IntVec3 item in AdjacentCellsCardinal)
			{
				yield return item;
			}
			yield return new IntVec3(minX - 1, 0, minZ - 1);
			yield return new IntVec3(maxX + 1, 0, minZ - 1);
			yield return new IntVec3(minX - 1, 0, maxZ + 1);
			yield return new IntVec3(maxX + 1, 0, maxZ + 1);
		}
	}

	public readonly bool AreSidesEqualOrGreater(int width, int height)
	{
		if (Width >= width)
		{
			return Height >= height;
		}
		return false;
	}

	public IntVec3 FarthestPoint(IntVec3 startingPoint, Rot4 direction)
	{
		if (!Contains(startingPoint))
		{
			throw new ArgumentException(startingPoint.ToString());
		}
		return direction.AsInt switch
		{
			0 => new IntVec3(startingPoint.x, 0, maxZ), 
			1 => new IntVec3(maxX, 0, startingPoint.z), 
			2 => new IntVec3(startingPoint.x, 0, minZ), 
			3 => new IntVec3(minX, 0, startingPoint.z), 
			_ => throw new ArgumentException(direction.ToString()), 
		};
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
		return new CellRect
		{
			minX = Mathf.Min(minX, maxX),
			minZ = Mathf.Min(minZ, maxZ),
			maxX = Mathf.Max(maxX, minX),
			maxZ = Mathf.Max(maxZ, minZ)
		};
	}

	public static CellRect FromLimits(IntVec3 first, IntVec3 second)
	{
		return new CellRect
		{
			minX = Mathf.Min(first.x, second.x),
			minZ = Mathf.Min(first.z, second.z),
			maxX = Mathf.Max(first.x, second.x),
			maxZ = Mathf.Max(first.z, second.z)
		};
	}

	public static CellRect FromCellList(IEnumerable<IntVec3> cells)
	{
		CellRect result = new CellRect
		{
			minX = int.MaxValue,
			minZ = int.MaxValue
		};
		foreach (IntVec3 cell in cells)
		{
			if (cell.x < result.minX)
			{
				result.minX = cell.x;
			}
			if (cell.z < result.minZ)
			{
				result.minZ = cell.z;
			}
			if (cell.x > result.maxX)
			{
				result.maxX = cell.x;
			}
			if (cell.z > result.maxZ)
			{
				result.maxZ = cell.z;
			}
		}
		return result;
	}

	public static CellRect CenteredOn(IntVec3 center, int radius)
	{
		return new CellRect
		{
			minX = center.x - radius,
			maxX = center.x + radius,
			minZ = center.z - radius,
			maxZ = center.z + radius
		};
	}

	public static CellRect CenteredOn(IntVec3 center, int width, int height)
	{
		CellRect result = new CellRect
		{
			minX = center.x - width / 2,
			minZ = center.z - height / 2
		};
		result.maxX = result.minX + width - 1;
		result.maxZ = result.minZ + height - 1;
		return result;
	}

	public static CellRect CenteredOn(IntVec3 center, IntVec2 size)
	{
		return CenteredOn(center, size.x, size.z);
	}

	public static CellRect ViewRect(Map map)
	{
		if (Current.ProgramState != ProgramState.Playing || Find.CurrentMap != map || WorldRendererUtility.WorldSelected)
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
		CellRect cellRect = this;
		cellRect.ClipInsideRect(within);
		return this == cellRect;
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

	public bool OverlapsCardinal(CellRect other)
	{
		if (IsEmpty || other.IsEmpty)
		{
			return false;
		}
		CellRect other2 = this;
		other2.minZ++;
		other2.maxZ--;
		CellRect other3 = this;
		other3.minX++;
		other3.maxX--;
		if (!other.Overlaps(other2))
		{
			return other.Overlaps(other3);
		}
		return true;
	}

	public bool Overlaps(Bounds other)
	{
		if (IsEmpty || other.size == Vector3.zero)
		{
			return false;
		}
		if ((float)minX <= other.max.x && (float)maxX >= other.min.x && (float)maxZ >= other.min.z)
		{
			return (float)minZ <= other.max.z;
		}
		return false;
	}

	public Bounds ToBounds()
	{
		return new Bounds
		{
			center = CenterVector3,
			size = new Vector3(Width, 1f, Height)
		};
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

	public IntVec3 ClosestCorner(IntVec3 cell)
	{
		IntVec3 corner = GetCorner(Rot4.North);
		IntVec3 corner2 = GetCorner(Rot4.East);
		IntVec3 corner3 = GetCorner(Rot4.South);
		IntVec3 corner4 = GetCorner(Rot4.West);
		IntVec3 intVec = cell - corner;
		IntVec3 intVec2 = cell - corner2;
		IntVec3 intVec3 = cell - corner3;
		IntVec3 intVec4 = cell - corner4;
		return GenMath.MinBy(corner, intVec.SqrMagnitude, corner2, intVec2.SqrMagnitude, corner3, intVec3.SqrMagnitude, corner4, intVec4.SqrMagnitude);
	}

	public Rot4 GetClosestEdge(IntVec3 c)
	{
		int num = Mathf.Abs(c.x - minX);
		int num2 = Mathf.Abs(c.x - maxX);
		int num3 = Mathf.Abs(c.z - maxZ);
		int num4 = Mathf.Abs(c.z - minZ);
		return GenMath.MinBy(Rot4.West, num, Rot4.East, num2, Rot4.North, num3, Rot4.South, num4);
	}

	public Rot4 GetEdgeCellRot(IntVec3 c)
	{
		if (c.x == maxX && c.z >= minZ && c.z <= maxZ)
		{
			return Rot4.East;
		}
		if (c.x == minX && c.z >= minZ && c.z <= maxZ)
		{
			return Rot4.West;
		}
		if (c.z == maxZ && c.x >= minX && c.x <= maxX)
		{
			return Rot4.North;
		}
		if (c.z == minZ && c.x >= minX && c.x <= maxX)
		{
			return Rot4.South;
		}
		Log.Error($"Attempted to get edge cell rotation but cell was not on an edge, cell: {c}, rect: {ToString()}");
		return Rot4.North;
	}

	public bool IsDiagonalCorner(IntVec3 c)
	{
		if (c.x == minX || c.x == maxX)
		{
			if (c.z != minZ)
			{
				return c.z == maxZ;
			}
			return true;
		}
		return false;
	}

	public IntVec3 GetCenterCellOnEdge(Rot4 rot)
	{
		return GetCellOnEdge(rot, CenterCell);
	}

	public IntVec3 GetCellOnEdge(Rot4 rot, IntVec3 point)
	{
		if (rot == Rot4.North)
		{
			return new IntVec3(point.x, point.y, maxZ);
		}
		if (rot == Rot4.East)
		{
			return new IntVec3(maxX, point.y, point.z);
		}
		if (rot == Rot4.South)
		{
			return new IntVec3(point.x, point.y, minZ);
		}
		if (rot == Rot4.West)
		{
			return new IntVec3(minX, point.y, point.z);
		}
		return IntVec3.Invalid;
	}

	public IntVec3 GetCenterCellOnEdge(Rot4 rot, int offset)
	{
		if (rot == Rot4.North)
		{
			return new IntVec3(CenterCell.x + offset, CenterCell.y, maxZ);
		}
		if (rot == Rot4.East)
		{
			return new IntVec3(maxX, CenterCell.y, CenterCell.z + offset);
		}
		if (rot == Rot4.South)
		{
			return new IntVec3(CenterCell.x + offset, CenterCell.y, minZ);
		}
		if (rot == Rot4.West)
		{
			return new IntVec3(minX, CenterCell.y, CenterCell.z + offset);
		}
		return IntVec3.Invalid;
	}

	public IEnumerable<IntVec3> GetCenterCellsOnEdge(Rot4 rot, int range)
	{
		for (int i = 0; i < range * 2 + 1; i++)
		{
			IntVec3 centerCellOnEdge = GetCenterCellOnEdge(rot, i - range);
			if (centerCellOnEdge.IsValid && Contains(centerCellOnEdge))
			{
				yield return centerCellOnEdge;
			}
		}
	}

	public IEnumerable<IntVec3> GetCellsCenterOutOnEdge(Rot4 rot)
	{
		return GetCenterCellsOnEdge(rot, GetSideLength(rot));
	}

	public int GetSideLength(Rot4 rot)
	{
		if (rot == Rot4.North || rot == Rot4.South)
		{
			return Width;
		}
		return Height;
	}

	public IEnumerable<IntVec3> GetCellsOnEdge(Rot4 rot)
	{
		if (IsEmpty)
		{
			yield break;
		}
		if (rot == Rot4.North)
		{
			for (int x = minX; x <= maxX; x++)
			{
				yield return new IntVec3(x, 0, maxZ);
			}
		}
		else if (rot == Rot4.East)
		{
			for (int x = minZ; x <= maxZ; x++)
			{
				yield return new IntVec3(maxX, 0, x);
			}
		}
		else if (rot == Rot4.South)
		{
			for (int x = minX; x <= maxX; x++)
			{
				yield return new IntVec3(x, 0, minZ);
			}
		}
		else if (rot == Rot4.West)
		{
			for (int x = minZ; x <= maxZ; x++)
			{
				yield return new IntVec3(minX, 0, x);
			}
		}
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

	public Rect ToScreenRect(Camera camera)
	{
		Vector3 vector = camera.WorldToScreenPoint(Min.ToVector3());
		Vector3 vector2 = camera.WorldToScreenPoint(Max.ToVector3() + new Vector3(1f, 0f, 1f));
		return Rect.MinMaxRect(vector.x, vector.y, vector2.x, vector2.y);
	}

	public (CellRect bottom, CellRect up) SplitVertical(int separation = 0)
	{
		IntVec3 centerCell = CenterCell;
		CellRect item = this;
		CellRect item2 = this;
		item.maxZ = centerCell.z - separation;
		item2.minZ = centerCell.z + separation;
		return (bottom: item, up: item2);
	}

	public (CellRect left, CellRect right) SplitHorizontal(int separation = 0)
	{
		IntVec3 centerCell = CenterCell;
		CellRect item = this;
		CellRect item2 = this;
		item.maxX = centerCell.x - separation;
		item2.minX = centerCell.x + separation;
		return (left: item, right: item2);
	}

	public (CellRect leftBottom, CellRect leftUp, CellRect rightBottom, CellRect rightUp) Subdivide(int separation)
	{
		var (cellRect, cellRect2) = SplitHorizontal(separation);
		var (item, item2) = cellRect.SplitVertical(separation);
		var (item3, item4) = cellRect2.SplitVertical(separation);
		return (leftBottom: item, leftUp: item2, rightBottom: item3, rightUp: item4);
	}

	public List<CellRect> Subdivide(int times, int separation)
	{
		List<CellRect> list = new List<CellRect> { this };
		for (int i = 0; i < times; i++)
		{
			int count = list.Count;
			for (int j = 0; j < count; j++)
			{
				var (item, item2, item3, item4) = list[j].Subdivide(separation);
				list.Add(item);
				list.Add(item2);
				list.Add(item3);
				list.Add(item4);
			}
			list.RemoveRange(0, count);
		}
		return list;
	}

	public void SubdivideToMaxLength(int size, List<CellRect> rects)
	{
		rects.Clear();
		rects.Add(this);
		for (int i = 0; i < rects.Count; i++)
		{
			CellRect cellRect = rects[i];
			while (cellRect.Width > size)
			{
				rects.RemoveAt(i);
				(CellRect left, CellRect right) tuple = cellRect.SplitHorizontal(1);
				CellRect item = tuple.left;
				CellRect item2 = tuple.right;
				cellRect = item;
				rects.Add(item);
				rects.Add(item2);
			}
			while (cellRect.Height > size)
			{
				rects.RemoveAt(i);
				(CellRect bottom, CellRect up) tuple2 = cellRect.SplitVertical(1);
				CellRect item3 = tuple2.bottom;
				CellRect item4 = tuple2.up;
				cellRect = item3;
				rects.Add(item3);
				rects.Add(item4);
			}
		}
	}

	public bool TryContractToRemove(IntVec3 point, out CellRect rect)
	{
		rect = this;
		IntVec3 centerCell = CenterCell;
		if (point == centerCell)
		{
			if (Width < 3 && Height < 3)
			{
				rect = default(CellRect);
				return false;
			}
			if (Width > Height)
			{
				if (minX < point.x - 1)
				{
					rect.maxX = point.x - 1;
				}
				else
				{
					rect.minX = point.x + 1;
				}
			}
			else if (minZ < point.z - 1)
			{
				rect.maxZ = point.z - 1;
			}
			else
			{
				rect.minZ = point.z + 1;
			}
			return true;
		}
		if (point.x <= maxX && point.x >= minX)
		{
			if (point.x >= centerCell.x)
			{
				rect.maxX = point.x - 1;
			}
			else
			{
				rect.minX = point.x + 1;
			}
		}
		if (point.z <= maxZ && point.z >= minZ)
		{
			if (point.z >= centerCell.z)
			{
				rect.maxZ = point.z - 1;
			}
			else
			{
				rect.minZ = point.z + 1;
			}
		}
		return true;
	}

	public readonly bool Contains(IntVec3 c)
	{
		if (c.x >= minX && c.x <= maxX && c.z >= minZ)
		{
			return c.z <= maxZ;
		}
		return false;
	}

	public readonly bool InlineWith(CellRect other)
	{
		if (minX != other.minX || maxX != other.maxX)
		{
			if (minZ == other.minZ)
			{
				return maxZ == other.maxZ;
			}
			return false;
		}
		return true;
	}

	public IntVec3 GetCorner(Rot4 rot, bool clockwise = true)
	{
		if (rot == Rot4.North)
		{
			return new IntVec3(clockwise ? maxX : minX, 0, maxZ);
		}
		if (rot == Rot4.East)
		{
			return new IntVec3(clockwise ? maxX : minX, 0, minZ);
		}
		if (rot == Rot4.South)
		{
			return new IntVec3(clockwise ? minX : maxX, 0, minZ);
		}
		if (rot == Rot4.West)
		{
			return new IntVec3(clockwise ? minX : maxX, 0, maxZ);
		}
		return IntVec3.Invalid;
	}

	public void GetInternalCorners(out IntVec3 BL, out IntVec3 TL, out IntVec3 TR, out IntVec3 BR)
	{
		BL = new IntVec3(minX, 0, minZ);
		TL = new IntVec3(minX, 0, maxZ);
		TR = new IntVec3(maxX, 0, maxZ);
		BR = new IntVec3(maxX, 0, minZ);
	}

	public void GetAdjacentCorners(out IntVec3 BL, out IntVec3 TL, out IntVec3 TR, out IntVec3 BR)
	{
		BL = new IntVec3(minX - 1, 0, minZ - 1);
		TL = new IntVec3(minX - 1, 0, maxZ + 1);
		TR = new IntVec3(maxX + 1, 0, maxZ + 1);
		BR = new IntVec3(maxX + 1, 0, minZ - 1);
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

	public float ClosestDistanceTo(IntVec3 c)
	{
		return ClosestCellTo(c).DistanceTo(c);
	}

	public float DistanceToEdge(IntVec3 v)
	{
		return Mathf.Max(Mathf.Min(Mathf.Min(Mathf.Min(v.x, v.z), Width - v.x - 1), Height - v.z - 1), 0);
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
			for (int x = minX; x <= maxX; x++)
			{
				yield return new IntVec3(x, 0, maxZ);
			}
		}
		else if (dir == Rot4.South)
		{
			for (int x = minX; x <= maxX; x++)
			{
				yield return new IntVec3(x, 0, minZ);
			}
		}
		else if (dir == Rot4.West)
		{
			for (int x = minZ; x <= maxZ; x++)
			{
				yield return new IntVec3(minX, 0, x);
			}
		}
		else if (dir == Rot4.East)
		{
			for (int x = minZ; x <= maxZ; x++)
			{
				yield return new IntVec3(maxX, 0, x);
			}
		}
	}

	public readonly CellRect GetEdgeRect(Rot4 rot)
	{
		if (rot == Rot4.North)
		{
			return FromLimits(new IntVec3(minX, 0, maxZ), new IntVec3(maxX, 0, maxZ));
		}
		if (rot == Rot4.South)
		{
			return FromLimits(new IntVec3(minX, 0, minZ), new IntVec3(maxX, 0, minZ));
		}
		if (rot == Rot4.West)
		{
			return FromLimits(new IntVec3(minX, 0, minZ), new IntVec3(minX, 0, maxZ));
		}
		return FromLimits(new IntVec3(maxX, 0, minZ), new IntVec3(maxX, 0, maxZ));
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
		}).TryRandomElement(out var result))
		{
			rect = new CellRect(result.x, result.z, size.x, size.z);
			return true;
		}
		rect = Empty;
		return false;
	}

	public readonly bool TryFindRandomInnerRectTouchingCorner(IntVec2 size, out CellRect rect, Predicate<CellRect> predicate = null)
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
		if (cellRect.Corners.Where(delegate(IntVec3 x)
		{
			if (predicate == null)
			{
				return true;
			}
			CellRect obj = new CellRect(x.x, x.z, size.x, size.z);
			return predicate(obj);
		}).TryRandomElement(out var result))
		{
			rect = new CellRect(result.x, result.z, size.x, size.z);
			return true;
		}
		rect = Empty;
		return false;
	}

	public readonly bool TryFindRandomCell(out IntVec3 cell, Predicate<IntVec3> validator = null)
	{
		foreach (IntVec3 item in Cells.InRandomOrder())
		{
			if (validator == null || validator(item))
			{
				cell = item;
				return true;
			}
		}
		cell = IntVec3.Invalid;
		return false;
	}

	public readonly bool TryFindRandomInnerRect(IntVec2 size, out CellRect rect, Predicate<CellRect> validator = null)
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
			if (validator == null)
			{
				return true;
			}
			CellRect obj = new CellRect(x.x, x.z, size.x, size.z);
			return validator(obj);
		}).TryRandomElement(out var result))
		{
			rect = new CellRect(result.x, result.z, size.x, size.z);
			return true;
		}
		rect = Empty;
		return false;
	}

	public readonly bool TryFindNearestInnerRectTo(IntVec2 size, IntVec3 position, out CellRect rect)
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
		CellRect cellRect2 = Empty;
		float num = float.MaxValue;
		foreach (IntVec3 cell in cellRect.Cells)
		{
			CellRect cellRect3 = new CellRect(cell.x, cell.z, size.x, size.z);
			int num2 = position.DistanceToSquared(cellRect3.CenterCell);
			if ((float)num2 < num)
			{
				cellRect2 = cellRect3;
				num = num2;
			}
		}
		rect = cellRect2;
		return true;
	}

	public readonly CellRect ExpandedBy(int dist)
	{
		CellRect result = this;
		result.minX -= dist;
		result.minZ -= dist;
		result.maxX += dist;
		result.maxZ += dist;
		return result;
	}

	public readonly CellRect ExpandedBy(int x, int z)
	{
		CellRect result = this;
		result.minX -= x;
		result.minZ -= z;
		result.maxX += x;
		result.maxZ += z;
		return result;
	}

	public readonly CellRect ExpandedBy(IntVec3 offset)
	{
		CellRect result = this;
		result.minX -= offset.x;
		result.minZ -= offset.z;
		result.maxX += offset.x;
		result.maxZ += offset.z;
		return result;
	}

	public readonly CellRect MaxExpandedBy(int dist)
	{
		CellRect result = this;
		result.maxX += dist;
		result.maxZ += dist;
		return result;
	}

	public readonly CellRect MaxExpandedBy(int x, int z)
	{
		CellRect result = this;
		result.maxX += x;
		result.maxZ += z;
		return result;
	}

	public readonly CellRect MaxExpandedBy(IntVec3 offset)
	{
		CellRect result = this;
		result.maxX += offset.x;
		result.maxZ += offset.z;
		return result;
	}

	public readonly CellRect MinExpandedBy(int dist)
	{
		CellRect result = this;
		result.minX -= dist;
		result.minZ -= dist;
		return result;
	}

	public readonly CellRect MinExpandedBy(int x, int z)
	{
		CellRect result = this;
		result.minX -= x;
		result.minZ -= z;
		return result;
	}

	public readonly CellRect MinExpandedBy(IntVec3 offset)
	{
		CellRect result = this;
		result.minX -= offset.x;
		result.minZ -= offset.z;
		return result;
	}

	public readonly CellRect ContractedBy(int x, int z)
	{
		return ExpandedBy(new IntVec3(-x, 0, -z));
	}

	public readonly CellRect ContractedBy(IntVec3 dist)
	{
		return ExpandedBy(-dist);
	}

	public readonly CellRect ContractedBy(int dist)
	{
		return ExpandedBy(-dist);
	}

	public CellRect ContractedBy(int x, int z, int negativeX, int negativeZ)
	{
		CellRect result = this;
		result.minX += x;
		result.minZ += z;
		result.maxX -= negativeX;
		result.maxZ -= negativeZ;
		return result;
	}

	public CellRect MovedBy(int x, int z)
	{
		return MovedBy(new IntVec3(x, 0, z));
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

	public CellRect Encapsulate(CellRect otherRect)
	{
		CellRect result = this;
		result.minX = Mathf.Min(minX, otherRect.minX);
		result.minZ = Mathf.Min(minZ, otherRect.minZ);
		result.maxX = Mathf.Max(maxX, otherRect.maxX);
		result.maxZ = Mathf.Max(maxZ, otherRect.maxZ);
		return result;
	}

	public CellRect Encapsulate(IntVec3 point)
	{
		CellRect result = this;
		result.minX = Mathf.Min(minX, point.x);
		result.minZ = Mathf.Min(minZ, point.z);
		result.maxX = Mathf.Max(maxX, point.x);
		result.maxZ = Mathf.Max(maxZ, point.z);
		return result;
	}

	public IEnumerable<IntVec3> UnionCells(CellRect container)
	{
		for (int z = container.minZ; z <= container.maxZ; z++)
		{
			for (int x = container.minX; x <= container.maxX; x++)
			{
				IntVec3 intVec = new IntVec3(x, 0, z);
				if (Contains(intVec))
				{
					yield return intVec;
				}
			}
		}
	}

	public IEnumerable<IntVec3> DifferenceCells(CellRect container)
	{
		for (int z = container.minZ; z <= container.maxZ; z++)
		{
			for (int x = container.minX; x <= container.maxX; x++)
			{
				IntVec3 intVec = new IntVec3(x, 0, z);
				if (!Contains(intVec))
				{
					yield return intVec;
				}
			}
		}
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

	public void Reset()
	{
		this = default(CellRect);
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
		if (!(obj is CellRect other))
		{
			return false;
		}
		return Equals(other);
	}

	public bool Equals(CellRect other)
	{
		if (minX == other.minX && maxX == other.maxX && minZ == other.minZ)
		{
			return maxZ == other.maxZ;
		}
		return false;
	}

	public static CellRect FromCell(IntVec3 cell)
	{
		return new CellRect(cell.x, cell.z, 1, 1);
	}

	public readonly IEnumerable<CellRect> EnumerateRectanglesCovering(Predicate<IntVec3> validator)
	{
		tmpUnvistedCells.Clear();
		foreach (IntVec3 cell in Cells)
		{
			if (validator(cell))
			{
				tmpUnvistedCells.Add(cell);
			}
		}
		while (tmpUnvistedCells.Count > 0)
		{
			IntVec3 item = tmpUnvistedCells.First();
			tmpUnvistedCells.Remove(item);
			int i = item.x;
			int num;
			for (num = item.z; tmpUnvistedCells.Contains(new IntVec3(i + 1, 0, num)); i++)
			{
			}
			while (true)
			{
				bool flag = false;
				for (int j = item.x; j <= i; j++)
				{
					if (!tmpUnvistedCells.Contains(new IntVec3(j, 0, num + 1)))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					break;
				}
				num++;
			}
			CellRect cellRect = new CellRect(item.x, item.z, i - item.x + 1, num - item.z + 1);
			foreach (IntVec3 cell2 in cellRect.Cells)
			{
				tmpUnvistedCells.Remove(cell2);
			}
			yield return cellRect;
		}
	}
}
