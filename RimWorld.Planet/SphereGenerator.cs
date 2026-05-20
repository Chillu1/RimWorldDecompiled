using System.Collections.Generic;
using UnityEngine;

namespace RimWorld.Planet;

public static class SphereGenerator
{
	private static List<TriangleIndices> tris = new List<TriangleIndices>();

	private static List<TriangleIndices> newTris = new List<TriangleIndices>();

	private static Dictionary<long, int> middlePointsCache = new Dictionary<long, int>();

	public static void Generate(int subdivisionsCount, float radius, Vector3 viewCenter, float viewAngle, out List<Vector3> outVerts, out List<int> outIndices)
	{
		middlePointsCache.Clear();
		outVerts = new List<Vector3>();
		IcosahedronGenerator.GenerateIcosahedron(outVerts, tris, radius, viewCenter, viewAngle);
		for (int i = 0; i < subdivisionsCount; i++)
		{
			newTris.Clear();
			int j = 0;
			for (int count = tris.Count; j < count; j++)
			{
				TriangleIndices triangleIndices = tris[j];
				int middlePoint = GetMiddlePoint(triangleIndices.v1, triangleIndices.v2, outVerts, radius);
				int middlePoint2 = GetMiddlePoint(triangleIndices.v2, triangleIndices.v3, outVerts, radius);
				int middlePoint3 = GetMiddlePoint(triangleIndices.v3, triangleIndices.v1, outVerts, radius);
				newTris.Add(new TriangleIndices(triangleIndices.v1, middlePoint, middlePoint3));
				newTris.Add(new TriangleIndices(triangleIndices.v2, middlePoint2, middlePoint));
				newTris.Add(new TriangleIndices(triangleIndices.v3, middlePoint3, middlePoint2));
				newTris.Add(new TriangleIndices(middlePoint, middlePoint2, middlePoint3));
			}
			tris.Clear();
			tris.AddRange(newTris);
		}
		MeshUtility.RemoveVertices(outVerts, tris, (Vector3 x) => !MeshUtility.Visible(x, radius, viewCenter, viewAngle));
		outIndices = new List<int>();
		int num = 0;
		for (int count2 = tris.Count; num < count2; num++)
		{
			TriangleIndices triangleIndices2 = tris[num];
			outIndices.Add(triangleIndices2.v1);
			outIndices.Add(triangleIndices2.v2);
			outIndices.Add(triangleIndices2.v3);
		}
	}

	private static int GetMiddlePoint(int p1, int p2, List<Vector3> verts, float radius)
	{
		long key = ((long)Mathf.Min(p1, p2) << 32) + Mathf.Max(p1, p2);
		if (middlePointsCache.TryGetValue(key, out var value))
		{
			return value;
		}
		Vector3 vector = (verts[p1] + verts[p2]) / 2f;
		int count = verts.Count;
		verts.Add(vector.normalized * radius);
		middlePointsCache.Add(key, count);
		return count;
	}
}
