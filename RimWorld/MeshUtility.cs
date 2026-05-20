using System;
using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class MeshUtility
{
	private static List<int> offsets = new List<int>();

	private static List<bool> vertIsUsed = new List<bool>();

	public static void RemoveVertices(List<Vector3> verts, List<TriangleIndices> tris, Predicate<Vector3> predicate)
	{
		int i = 0;
		for (int count = tris.Count; i < count; i++)
		{
			TriangleIndices triangleIndices = tris[i];
			if (predicate(verts[triangleIndices.v1]) || predicate(verts[triangleIndices.v2]) || predicate(verts[triangleIndices.v3]))
			{
				tris[i] = new TriangleIndices(-1, -1, -1);
			}
		}
		tris.RemoveAll((TriangleIndices x) => x.v1 == -1);
		RemoveUnusedVertices(verts, tris);
	}

	public static void RemoveUnusedVertices(List<Vector3> verts, List<TriangleIndices> tris)
	{
		vertIsUsed.Clear();
		int i = 0;
		for (int count = verts.Count; i < count; i++)
		{
			vertIsUsed.Add(item: false);
		}
		int j = 0;
		for (int count2 = tris.Count; j < count2; j++)
		{
			TriangleIndices triangleIndices = tris[j];
			vertIsUsed[triangleIndices.v1] = true;
			vertIsUsed[triangleIndices.v2] = true;
			vertIsUsed[triangleIndices.v3] = true;
		}
		int num = 0;
		offsets.Clear();
		int k = 0;
		for (int count3 = verts.Count; k < count3; k++)
		{
			if (!vertIsUsed[k])
			{
				num++;
			}
			offsets.Add(num);
		}
		int l = 0;
		for (int count4 = tris.Count; l < count4; l++)
		{
			TriangleIndices triangleIndices2 = tris[l];
			tris[l] = new TriangleIndices(triangleIndices2.v1 - offsets[triangleIndices2.v1], triangleIndices2.v2 - offsets[triangleIndices2.v2], triangleIndices2.v3 - offsets[triangleIndices2.v3]);
		}
		verts.RemoveAll((Vector3 elem, int index) => !vertIsUsed[index]);
	}

	public static bool Visible(Vector3 point, float radius, Vector3 viewCenter, float viewAngle)
	{
		if (viewAngle >= 180f)
		{
			return true;
		}
		return Vector3.Angle(viewCenter * radius, point) <= viewAngle;
	}

	public static bool VisibleForWorldgen(Vector3 point, float radius, Vector3 viewCenter, float viewAngle)
	{
		if (viewAngle >= 180f)
		{
			return true;
		}
		float num = Vector3.Angle(viewCenter * radius, point) + -1E-05f;
		if (Mathf.Abs(num - viewAngle) < 1E-06f)
		{
			Log.Warning($"Angle difference {num - viewAngle} is within epsilon; recommend adjusting visibility tweak");
		}
		return num <= viewAngle;
	}

	public static Color32 MutateAlpha(this Color32 input, byte newAlpha)
	{
		input.a = newAlpha;
		return input;
	}
}
