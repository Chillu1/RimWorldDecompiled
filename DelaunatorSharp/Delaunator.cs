using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DelaunatorSharp;

public class Delaunator
{
	private readonly double EPSILON = Math.Pow(2.0, -52.0);

	private readonly int[] EDGE_STACK = new int[512];

	private readonly int hashSize;

	private readonly int[] hullPrev;

	private readonly int[] hullNext;

	private readonly int[] hullTri;

	private readonly int[] hullHash;

	private double cx;

	private double cy;

	private int trianglesLen;

	private readonly double[] coords;

	private readonly int hullStart;

	private readonly int hullSize;

	public int[] Triangles { get; private set; }

	public int[] Halfedges { get; private set; }

	public Vector2[] Points { get; private set; }

	public int[] Hull { get; private set; }

	public Delaunator(Vector2[] points)
	{
		if (points.Length < 3)
		{
			throw new ArgumentOutOfRangeException("Need at least 3 points");
		}
		Points = points;
		coords = new double[Points.Length * 2];
		for (int i = 0; i < Points.Length; i++)
		{
			Vector2 vector = Points[i];
			coords[2 * i] = vector.x;
			coords[2 * i + 1] = vector.y;
		}
		int num = points.Length;
		int num2 = 2 * num - 5;
		Triangles = new int[num2 * 3];
		Halfedges = new int[num2 * 3];
		hashSize = (int)Math.Ceiling(Math.Sqrt(num));
		hullPrev = new int[num];
		hullNext = new int[num];
		hullTri = new int[num];
		hullHash = new int[hashSize];
		int[] array = new int[num];
		double num3 = double.PositiveInfinity;
		double num4 = double.PositiveInfinity;
		double num5 = double.NegativeInfinity;
		double num6 = double.NegativeInfinity;
		for (int j = 0; j < num; j++)
		{
			double num7 = coords[2 * j];
			double num8 = coords[2 * j + 1];
			if (num7 < num3)
			{
				num3 = num7;
			}
			if (num8 < num4)
			{
				num4 = num8;
			}
			if (num7 > num5)
			{
				num5 = num7;
			}
			if (num8 > num6)
			{
				num6 = num8;
			}
			array[j] = j;
		}
		double ax = (num3 + num5) / 2.0;
		double ay = (num4 + num6) / 2.0;
		double num9 = double.PositiveInfinity;
		int num10 = 0;
		int num11 = 0;
		int num12 = 0;
		for (int k = 0; k < num; k++)
		{
			double num13 = Dist(ax, ay, coords[2 * k], coords[2 * k + 1]);
			if (num13 < num9)
			{
				num10 = k;
				num9 = num13;
			}
		}
		double num14 = coords[2 * num10];
		double num15 = coords[2 * num10 + 1];
		num9 = double.PositiveInfinity;
		for (int l = 0; l < num; l++)
		{
			if (l != num10)
			{
				double num16 = Dist(num14, num15, coords[2 * l], coords[2 * l + 1]);
				if (num16 < num9 && num16 > 0.0)
				{
					num11 = l;
					num9 = num16;
				}
			}
		}
		double num17 = coords[2 * num11];
		double num18 = coords[2 * num11 + 1];
		double num19 = double.PositiveInfinity;
		for (int m = 0; m < num; m++)
		{
			if (m != num10 && m != num11)
			{
				double num20 = Circumradius(num14, num15, num17, num18, coords[2 * m], coords[2 * m + 1]);
				if (num20 < num19)
				{
					num12 = m;
					num19 = num20;
				}
			}
		}
		double num21 = coords[2 * num12];
		double num22 = coords[2 * num12 + 1];
		if (num19 == double.PositiveInfinity)
		{
			throw new Exception("No Delaunay triangulation exists for this input.");
		}
		if (Orient(num14, num15, num17, num18, num21, num22))
		{
			int num23 = num11;
			double num24 = num17;
			double num25 = num18;
			num11 = num12;
			num17 = num21;
			num18 = num22;
			num12 = num23;
			num21 = num24;
			num22 = num25;
		}
		Vector2 vector2 = Circumcenter(num14, num15, num17, num18, num21, num22);
		cx = vector2.x;
		cy = vector2.y;
		double[] array2 = new double[num];
		for (int n = 0; n < num; n++)
		{
			array2[n] = Dist(coords[2 * n], coords[2 * n + 1], vector2.x, vector2.y);
		}
		Quicksort(array, array2, 0, num - 1);
		hullStart = num10;
		hullSize = 3;
		hullNext[num10] = (hullPrev[num12] = num11);
		hullNext[num11] = (hullPrev[num10] = num12);
		hullNext[num12] = (hullPrev[num11] = num10);
		hullTri[num10] = 0;
		hullTri[num11] = 1;
		hullTri[num12] = 2;
		hullHash[HashKey(num14, num15)] = num10;
		hullHash[HashKey(num17, num18)] = num11;
		hullHash[HashKey(num21, num22)] = num12;
		trianglesLen = 0;
		AddTriangle(num10, num11, num12, -1, -1, -1);
		double num26 = 0.0;
		double num27 = 0.0;
		for (int num28 = 0; num28 < array.Length; num28++)
		{
			int num29 = array[num28];
			double num30 = coords[2 * num29];
			double num31 = coords[2 * num29 + 1];
			if (num28 > 0 && Math.Abs(num30 - num26) <= EPSILON && Math.Abs(num31 - num27) <= EPSILON)
			{
				continue;
			}
			num26 = num30;
			num27 = num31;
			if (num29 == num10 || num29 == num11 || num29 == num12)
			{
				continue;
			}
			int num32 = 0;
			for (int num33 = 0; num33 < hashSize; num33++)
			{
				int num34 = HashKey(num30, num31);
				num32 = hullHash[(num34 + num33) % hashSize];
				if (num32 != -1 && num32 != hullNext[num32])
				{
					break;
				}
			}
			num32 = hullPrev[num32];
			int num35 = num32;
			int num36 = hullNext[num35];
			while (!Orient(num30, num31, coords[2 * num35], coords[2 * num35 + 1], coords[2 * num36], coords[2 * num36 + 1]))
			{
				num35 = num36;
				if (num35 == num32)
				{
					num35 = int.MaxValue;
					break;
				}
				num36 = hullNext[num35];
			}
			if (num35 == int.MaxValue)
			{
				continue;
			}
			int num37 = AddTriangle(num35, num29, hullNext[num35], -1, -1, hullTri[num35]);
			hullTri[num29] = Legalize(num37 + 2);
			hullTri[num35] = num37;
			hullSize++;
			int num38 = hullNext[num35];
			num36 = hullNext[num38];
			while (Orient(num30, num31, coords[2 * num38], coords[2 * num38 + 1], coords[2 * num36], coords[2 * num36 + 1]))
			{
				num37 = AddTriangle(num38, num29, num36, hullTri[num29], -1, hullTri[num38]);
				hullTri[num29] = Legalize(num37 + 2);
				hullNext[num38] = num38;
				hullSize--;
				num38 = num36;
				num36 = hullNext[num38];
			}
			if (num35 == num32)
			{
				num36 = hullPrev[num35];
				while (Orient(num30, num31, coords[2 * num36], coords[2 * num36 + 1], coords[2 * num35], coords[2 * num35 + 1]))
				{
					num37 = AddTriangle(num36, num29, num35, -1, hullTri[num35], hullTri[num36]);
					Legalize(num37 + 2);
					hullTri[num36] = num37;
					hullNext[num35] = num35;
					hullSize--;
					num35 = num36;
					num36 = hullPrev[num35];
				}
			}
			hullStart = (hullPrev[num29] = num35);
			hullNext[num35] = (hullPrev[num38] = num29);
			hullNext[num29] = num38;
			hullHash[HashKey(num30, num31)] = num29;
			hullHash[HashKey(coords[2 * num35], coords[2 * num35 + 1])] = num35;
		}
		Hull = new int[hullSize];
		int num39 = hullStart;
		for (int num40 = 0; num40 < hullSize; num40++)
		{
			Hull[num40] = num39;
			num39 = hullNext[num39];
		}
		hullPrev = (hullNext = (hullTri = null));
		Triangles = Triangles.Take(trianglesLen).ToArray();
		Halfedges = Halfedges.Take(trianglesLen).ToArray();
	}

	private int Legalize(int a)
	{
		int num = 0;
		int num4;
		while (true)
		{
			int num2 = Halfedges[a];
			int num3 = a - a % 3;
			num4 = num3 + (a + 2) % 3;
			if (num2 == -1)
			{
				if (num == 0)
				{
					break;
				}
				a = EDGE_STACK[--num];
				continue;
			}
			int num5 = num2 - num2 % 3;
			int num6 = num3 + (a + 1) % 3;
			int num7 = num5 + (num2 + 2) % 3;
			int num8 = Triangles[num4];
			int num9 = Triangles[a];
			int num10 = Triangles[num6];
			int num11 = Triangles[num7];
			if (InCircle(coords[2 * num8], coords[2 * num8 + 1], coords[2 * num9], coords[2 * num9 + 1], coords[2 * num10], coords[2 * num10 + 1], coords[2 * num11], coords[2 * num11 + 1]))
			{
				Triangles[a] = num11;
				Triangles[num2] = num8;
				int num12 = Halfedges[num7];
				if (num12 == -1)
				{
					int num13 = hullStart;
					do
					{
						if (hullTri[num13] == num7)
						{
							hullTri[num13] = a;
							break;
						}
						num13 = hullPrev[num13];
					}
					while (num13 != hullStart);
				}
				Link(a, num12);
				Link(num2, Halfedges[num4]);
				Link(num4, num7);
				int num14 = num5 + (num2 + 1) % 3;
				if (num < EDGE_STACK.Length)
				{
					EDGE_STACK[num++] = num14;
				}
			}
			else
			{
				if (num == 0)
				{
					break;
				}
				a = EDGE_STACK[--num];
			}
		}
		return num4;
	}

	private static bool InCircle(double ax, double ay, double bx, double by, double cx, double cy, double px, double py)
	{
		double num = ax - px;
		double num2 = ay - py;
		double num3 = bx - px;
		double num4 = by - py;
		double num5 = cx - px;
		double num6 = cy - py;
		double num7 = num * num + num2 * num2;
		double num8 = num3 * num3 + num4 * num4;
		double num9 = num5 * num5 + num6 * num6;
		return num * (num4 * num9 - num8 * num6) - num2 * (num3 * num9 - num8 * num5) + num7 * (num3 * num6 - num4 * num5) < 0.0;
	}

	private int AddTriangle(int i0, int i1, int i2, int a, int b, int c)
	{
		int num = trianglesLen;
		Triangles[num] = i0;
		Triangles[num + 1] = i1;
		Triangles[num + 2] = i2;
		Link(num, a);
		Link(num + 1, b);
		Link(num + 2, c);
		trianglesLen += 3;
		return num;
	}

	private void Link(int a, int b)
	{
		Halfedges[a] = b;
		if (b != -1)
		{
			Halfedges[b] = a;
		}
	}

	private int HashKey(double x, double y)
	{
		return (int)(Math.Floor(PseudoAngle(x - cx, y - cy) * (double)hashSize) % (double)hashSize);
	}

	private static double PseudoAngle(double dx, double dy)
	{
		double num = dx / (Math.Abs(dx) + Math.Abs(dy));
		return ((dy > 0.0) ? (3.0 - num) : (1.0 + num)) / 4.0;
	}

	private static void Quicksort(int[] ids, double[] dists, int left, int right)
	{
		if (right - left <= 20)
		{
			for (int i = left + 1; i <= right; i++)
			{
				int num = ids[i];
				double num2 = dists[num];
				int num3 = i - 1;
				while (num3 >= left && dists[ids[num3]] > num2)
				{
					ids[num3 + 1] = ids[num3--];
				}
				ids[num3 + 1] = num;
			}
			return;
		}
		int i2 = left + right >> 1;
		int num4 = left + 1;
		int num5 = right;
		Swap(ids, i2, num4);
		if (dists[ids[left]] > dists[ids[right]])
		{
			Swap(ids, left, right);
		}
		if (dists[ids[num4]] > dists[ids[right]])
		{
			Swap(ids, num4, right);
		}
		if (dists[ids[left]] > dists[ids[num4]])
		{
			Swap(ids, left, num4);
		}
		int num6 = ids[num4];
		double num7 = dists[num6];
		while (true)
		{
			num4++;
			if (!(dists[ids[num4]] < num7))
			{
				do
				{
					num5--;
				}
				while (dists[ids[num5]] > num7);
				if (num5 < num4)
				{
					break;
				}
				Swap(ids, num4, num5);
			}
		}
		ids[left + 1] = ids[num5];
		ids[num5] = num6;
		if (right - num4 + 1 >= num5 - left)
		{
			Quicksort(ids, dists, num4, right);
			Quicksort(ids, dists, left, num5 - 1);
		}
		else
		{
			Quicksort(ids, dists, left, num5 - 1);
			Quicksort(ids, dists, num4, right);
		}
	}

	private static void Swap(int[] arr, int i, int j)
	{
		int num = arr[i];
		arr[i] = arr[j];
		arr[j] = num;
	}

	private static bool Orient(double px, double py, double qx, double qy, double rx, double ry)
	{
		return (qy - py) * (rx - qx) - (qx - px) * (ry - qy) < 0.0;
	}

	private static double Circumradius(double ax, double ay, double bx, double by, double cx, double cy)
	{
		double num = bx - ax;
		double num2 = by - ay;
		double num3 = cx - ax;
		double num4 = cy - ay;
		double num5 = num * num + num2 * num2;
		double num6 = num3 * num3 + num4 * num4;
		double num7 = 0.5 / (num * num4 - num2 * num3);
		double num8 = (num4 * num5 - num2 * num6) * num7;
		double num9 = (num * num6 - num3 * num5) * num7;
		return num8 * num8 + num9 * num9;
	}

	private static Vector2 Circumcenter(double ax, double ay, double bx, double by, double cx, double cy)
	{
		double num = bx - ax;
		double num2 = by - ay;
		double num3 = cx - ax;
		double num4 = cy - ay;
		double num5 = num * num + num2 * num2;
		double num6 = num3 * num3 + num4 * num4;
		double num7 = 0.5 / (num * num4 - num2 * num3);
		double num8 = ax + (num4 * num5 - num2 * num6) * num7;
		double num9 = ay + (num * num6 - num3 * num5) * num7;
		return new Vector2((float)num8, (float)num9);
	}

	private static double Dist(double ax, double ay, double bx, double by)
	{
		double num = ax - bx;
		double num2 = ay - by;
		return num * num + num2 * num2;
	}

	public IEnumerable<ITriangle> GetTriangles()
	{
		for (int t = 0; t < Triangles.Length / 3; t++)
		{
			yield return new Triangle(t, GetTrianglePoints(t));
		}
	}

	public IEnumerable<IEdge> GetEdges()
	{
		for (int e = 0; e < Triangles.Length; e++)
		{
			if (e > Halfedges[e])
			{
				Vector2 p = Points[Triangles[e]];
				Vector2 q = Points[Triangles[NextHalfedge(e)]];
				yield return new Edge(e, p, q);
			}
		}
	}

	public IEnumerable<IEdge> GetVoronoiEdges(Func<int, Vector2> triangleVerticeSelector = null)
	{
		if (triangleVerticeSelector == null)
		{
			triangleVerticeSelector = (int x) => GetCentroid(x);
		}
		for (int e = 0; e < Triangles.Length; e++)
		{
			if (e < Halfedges[e])
			{
				Vector2 p = triangleVerticeSelector(TriangleOfEdge(e));
				Vector2 q = triangleVerticeSelector(TriangleOfEdge(Halfedges[e]));
				yield return new Edge(e, p, q);
			}
		}
	}

	public IEnumerable<IEdge> GetVoronoiEdgesBasedOnCircumCenter()
	{
		return GetVoronoiEdges(GetTriangleCircumcenter);
	}

	public IEnumerable<IEdge> GetVoronoiEdgesBasedOnCentroids()
	{
		return GetVoronoiEdges(GetCentroid);
	}

	public IEnumerable<IVoronoiCell> GetVoronoiCells(Func<int, Vector2> triangleVerticeSelector = null)
	{
		if (triangleVerticeSelector == null)
		{
			triangleVerticeSelector = (int x) => GetCentroid(x);
		}
		HashSet<int> seen = new HashSet<int>();
		List<Vector2> vertices = new List<Vector2>(10);
		for (int e = 0; e < Triangles.Length; e++)
		{
			int num = Triangles[NextHalfedge(e)];
			if (!seen.Add(num))
			{
				continue;
			}
			foreach (int item in EdgesAroundPoint(e))
			{
				vertices.Add(triangleVerticeSelector(TriangleOfEdge(item)));
			}
			yield return new VoronoiCell(num, vertices.ToArray());
			vertices.Clear();
		}
	}

	public IEnumerable<IVoronoiCell> GetVoronoiCellsBasedOnCircumcenters()
	{
		return GetVoronoiCells(GetTriangleCircumcenter);
	}

	public IEnumerable<IVoronoiCell> GetVoronoiCellsBasedOnCentroids()
	{
		return GetVoronoiCells(GetCentroid);
	}

	public IEnumerable<IEdge> GetHullEdges()
	{
		return CreateHull(GetHullPoints());
	}

	public Vector2[] GetHullPoints()
	{
		return Array.ConvertAll(Hull, (int x) => Points[x]);
	}

	public Vector2[] GetTrianglePoints(int t)
	{
		List<Vector2> list = new List<Vector2>();
		foreach (int item in PointsOfTriangle(t))
		{
			list.Add(Points[item]);
		}
		return list.ToArray();
	}

	public Vector2[] GetRellaxedPoints()
	{
		List<Vector2> list = new List<Vector2>();
		foreach (IVoronoiCell voronoiCellsBasedOnCircumcenter in GetVoronoiCellsBasedOnCircumcenters())
		{
			list.Add(GetCentroid(voronoiCellsBasedOnCircumcenter.Points));
		}
		return list.ToArray();
	}

	public IEnumerable<IEdge> GetEdgesOfTriangle(int t)
	{
		return CreateHull(from p in EdgesOfTriangle(t)
			select Points[p]);
	}

	public static IEnumerable<IEdge> CreateHull(IEnumerable<Vector2> points)
	{
		return points.Zip(points.Skip(1).Append(points.FirstOrDefault()), (Vector2 a, Vector2 b) => new Edge(0, a, b)).OfType<IEdge>();
	}

	public Vector2 GetTriangleCircumcenter(int t)
	{
		Vector2[] trianglePoints = GetTrianglePoints(t);
		return GetCircumcenter(trianglePoints[0], trianglePoints[1], trianglePoints[2]);
	}

	public Vector2 GetCentroid(int t)
	{
		return GetCentroid(GetTrianglePoints(t));
	}

	public static Vector2 GetCircumcenter(Vector2 a, Vector2 b, Vector2 c)
	{
		return Circumcenter(a.x, a.y, b.x, b.y, c.x, c.y);
	}

	public static Vector2 GetCentroid(Vector2[] points)
	{
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		int num4 = 0;
		int num5 = points.Length - 1;
		while (num4 < points.Length)
		{
			float num6 = points[num4].x * points[num5].y - points[num5].x * points[num4].y;
			num += (double)num6;
			num2 += (double)((points[num4].x + points[num5].x) * num6);
			num3 += (double)((points[num4].y + points[num5].y) * num6);
			num5 = num4++;
		}
		if (Math.Abs(num) < 1.0000000116860974E-07)
		{
			return default(Vector2);
		}
		num *= 3.0;
		return new Vector2((float)(num2 / num), (float)(num3 / num));
	}

	public void ForEachTriangle(Action<ITriangle> callback)
	{
		foreach (ITriangle triangle in GetTriangles())
		{
			callback?.Invoke(triangle);
		}
	}

	public void ForEachTriangleEdge(Action<IEdge> callback)
	{
		foreach (IEdge edge in GetEdges())
		{
			callback?.Invoke(edge);
		}
	}

	public void ForEachVoronoiEdge(Action<IEdge> callback)
	{
		foreach (IEdge voronoiEdge in GetVoronoiEdges())
		{
			callback?.Invoke(voronoiEdge);
		}
	}

	public void ForEachVoronoiCellBasedOnCentroids(Action<IVoronoiCell> callback)
	{
		foreach (IVoronoiCell voronoiCellsBasedOnCentroid in GetVoronoiCellsBasedOnCentroids())
		{
			callback?.Invoke(voronoiCellsBasedOnCentroid);
		}
	}

	public void ForEachVoronoiCellBasedOnCircumcenters(Action<IVoronoiCell> callback)
	{
		foreach (IVoronoiCell voronoiCellsBasedOnCircumcenter in GetVoronoiCellsBasedOnCircumcenters())
		{
			callback?.Invoke(voronoiCellsBasedOnCircumcenter);
		}
	}

	public void ForEachVoronoiCell(Action<IVoronoiCell> callback, Func<int, Vector2> triangleVertexSelector = null)
	{
		foreach (IVoronoiCell voronoiCell in GetVoronoiCells(triangleVertexSelector))
		{
			callback?.Invoke(voronoiCell);
		}
	}

	public IEnumerable<int> EdgesAroundPoint(int start)
	{
		int incoming = start;
		do
		{
			yield return incoming;
			int num = NextHalfedge(incoming);
			incoming = Halfedges[num];
		}
		while (incoming != -1 && incoming != start);
	}

	public IEnumerable<int> PointsOfTriangle(int t)
	{
		int[] array = EdgesOfTriangle(t);
		foreach (int num in array)
		{
			yield return Triangles[num];
		}
	}

	public IEnumerable<int> TrianglesAdjacentToTriangle(int t)
	{
		List<int> list = new List<int>();
		int[] array = EdgesOfTriangle(t);
		foreach (int num in array)
		{
			int num2 = Halfedges[num];
			if (num2 >= 0)
			{
				list.Add(TriangleOfEdge(num2));
			}
		}
		return list;
	}

	public static int NextHalfedge(int e)
	{
		if (e % 3 != 2)
		{
			return e + 1;
		}
		return e - 2;
	}

	public static int PreviousHalfedge(int e)
	{
		if (e % 3 != 0)
		{
			return e - 1;
		}
		return e + 2;
	}

	public static int[] EdgesOfTriangle(int t)
	{
		return new int[3]
		{
			3 * t,
			3 * t + 1,
			3 * t + 2
		};
	}

	public static int TriangleOfEdge(int e)
	{
		return e / 3;
	}
}
