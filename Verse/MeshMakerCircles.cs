using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse;

public static class MeshMakerCircles
{
	private const float PieRadius = 0.55f;

	private const int MaxDegreesWide = 361;

	private const int CircleVertexCount = 92;

	private static readonly List<Vector3> tmpVerts = new List<Vector3>(361);

	private static readonly Vector2[] emptyUVs = new Vector2[361];

	private static readonly Vector3[] tmpCircleVerts = new Vector3[92];

	private static readonly int[] tmpTris = new int[273];

	public static void MakePieMeshes(Mesh[] meshes)
	{
		if (meshes.Length < 361)
		{
			throw new ArgumentException();
		}
		Vector3[] array = new Vector3[362];
		array[0] = new Vector3(0f, 0f, 0f);
		for (int i = 0; i <= 360; i++)
		{
			float num = (float)i / 180f * MathF.PI;
			float x = (float)(0.550000011920929 * Math.Cos(num));
			float z = (float)(0.550000011920929 * Math.Sin(num));
			array[i + 1] = new Vector3(x, 0f, z);
		}
		int[] array2 = new int[1080];
		for (int j = 0; j < 360; j++)
		{
			array2[j * 3] = j + 2;
			array2[j * 3 + 1] = j + 1;
			array2[j * 3 + 2] = 0;
		}
		Bounds bounds = default(Bounds);
		Vector3 vector = 0.55f * new Vector3(1f, 0f, 1f);
		bounds.SetMinMax(-vector, vector);
		for (int k = 0; k < 361; k++)
		{
			meshes[k] = new Mesh();
			meshes[k].vertices = array;
			meshes[k].SetTriangles(array2, 0, 3 * k, 0, calculateBounds: false);
			meshes[k].RecalculateNormals();
			meshes[k].bounds = bounds;
		}
	}

	public static Mesh MakeCircleMesh(float radius)
	{
		tmpCircleVerts[0] = Vector3.zero;
		int num = 0;
		int num2 = 1;
		while (num <= 360)
		{
			float f = (float)num / 180f * MathF.PI;
			tmpCircleVerts[num2] = new Vector3(radius * Mathf.Cos(f), 0f, radius * Mathf.Sin(f));
			num += 4;
			num2++;
		}
		for (int i = 1; i < tmpCircleVerts.Length; i++)
		{
			int num3 = (i - 1) * 3;
			tmpTris[num3] = 0;
			tmpTris[num3 + 1] = (i + 1) % tmpCircleVerts.Length;
			tmpTris[num3 + 2] = i;
		}
		Mesh mesh = new Mesh();
		mesh.name = $"CircleMesh_{radius:0.#}";
		mesh.SetVertices(tmpCircleVerts);
		mesh.SetTriangles(tmpTris, 0);
		return mesh;
	}
}
