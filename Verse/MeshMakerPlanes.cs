using UnityEngine;

namespace Verse;

public static class MeshMakerPlanes
{
	private const float BackLiftAmount = 0.0018292684f;

	private const float TwistAmount = 0.0009146342f;

	public static Mesh NewPlaneMesh(float size)
	{
		return NewPlaneMesh(size, flipped: false);
	}

	public static Mesh NewPlaneMesh(float size, bool flipped)
	{
		return NewPlaneMesh(size, flipped, backLift: false);
	}

	public static Mesh NewPlaneMesh(float size, bool flipped, bool backLift)
	{
		return NewPlaneMesh(new Vector2(size, size), flipped, backLift, twist: false);
	}

	public static Mesh NewPlaneMesh(float size, bool flipped, bool backLift, bool twist)
	{
		return NewPlaneMesh(new Vector2(size, size), flipped, backLift, twist);
	}

	public static Mesh NewPlaneMesh(Vector2 size, bool flipped, bool backLift, bool twist)
	{
		Vector3[] array = new Vector3[4];
		Vector2[] array2 = new Vector2[4];
		int[] array3 = new int[6];
		array[0] = new Vector3(-0.5f * size.x, 0f, -0.5f * size.y);
		array[1] = new Vector3(-0.5f * size.x, 0f, 0.5f * size.y);
		array[2] = new Vector3(0.5f * size.x, 0f, 0.5f * size.y);
		array[3] = new Vector3(0.5f * size.x, 0f, -0.5f * size.y);
		if (backLift)
		{
			array[1].y = 0.0018292684f;
			array[2].y = 0.0018292684f;
			array[3].y = 0.0007317074f;
		}
		if (twist)
		{
			array[0].y = 0.0009146342f;
			array[1].y = 0.0004573171f;
			array[2].y = 0f;
			array[3].y = 0.0004573171f;
		}
		if (!flipped)
		{
			array2[0] = new Vector2(0f, 0f);
			array2[1] = new Vector2(0f, 1f);
			array2[2] = new Vector2(1f, 1f);
			array2[3] = new Vector2(1f, 0f);
		}
		else
		{
			array2[0] = new Vector2(1f, 0f);
			array2[1] = new Vector2(1f, 1f);
			array2[2] = new Vector2(0f, 1f);
			array2[3] = new Vector2(0f, 0f);
		}
		array3[0] = 0;
		array3[1] = 1;
		array3[2] = 2;
		array3[3] = 0;
		array3[4] = 2;
		array3[5] = 3;
		Mesh mesh = new Mesh();
		mesh.name = "NewPlaneMesh()";
		mesh.vertices = array;
		mesh.uv = array2;
		mesh.SetTriangles(array3, 0);
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		MeshPool.EnsureMetaDataCached(mesh, size, flipped);
		return mesh;
	}

	public static Mesh NewWholeMapPlane()
	{
		Mesh mesh = NewPlaneMesh(2000f, flipped: false, backLift: false);
		Vector2[] array = new Vector2[4];
		for (int i = 0; i < 4; i++)
		{
			array[i] = mesh.uv[i] * 200f;
		}
		mesh.uv = array;
		return mesh;
	}
}
