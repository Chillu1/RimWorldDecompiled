using System.Collections.Generic;
using UnityEngine;

namespace RimWorld;

public static class LineMeshGenerator
{
	public static Mesh Generate(Vector2[] points, float width)
	{
		Vector3[] array = new Vector3[points.Length * 2];
		Vector2[] array2 = new Vector2[array.Length];
		int[] array3 = new int[2 * (points.Length - 1) * 3];
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < points.Length; i++)
		{
			Vector2 zero = Vector2.zero;
			if (i < points.Length - 1)
			{
				zero += points[(i + 1) % points.Length] - points[i];
			}
			if (i > 0)
			{
				zero += points[i] - points[(i - 1 + points.Length) % points.Length];
			}
			zero.Normalize();
			Vector2 vector = new Vector2(0f - zero.y, zero.x);
			Vector2 vector2 = points[i] + vector * (width * 0.5f);
			Vector2 vector3 = points[i] - vector * (width * 0.5f);
			array[num] = new Vector3(vector2.x, 0f, vector2.y);
			array[num + 1] = new Vector3(vector3.x, 0f, vector3.y);
			float num3 = (float)i / (float)(points.Length - 1);
			float y = 1f - Mathf.Abs(2f * num3 - 1f);
			array2[num] = new Vector2(0f, y);
			array2[num + 1] = new Vector2(1f, y);
			if (i < points.Length - 1)
			{
				array3[num2] = num;
				array3[num2 + 1] = (num + 2) % array.Length;
				array3[num2 + 2] = num + 1;
				array3[num2 + 3] = num + 1;
				array3[num2 + 4] = (num + 2) % array.Length;
				array3[num2 + 5] = (num + 3) % array.Length;
			}
			num += 2;
			num2 += 6;
		}
		return new Mesh
		{
			vertices = array,
			triangles = array3,
			uv = array2
		};
	}

	public static Vector2[] CalculateEvenlySpacedPoints(List<Vector2> points, float spacing, float resolution = 1f)
	{
		List<Vector2> list = new List<Vector2> { points[0] };
		Vector2 vector = points[0];
		int num = points.Count / 3;
		float num2 = 0f;
		for (int i = 0; i < num; i++)
		{
			Vector2[] pointsInSegment = GetPointsInSegment(points, i);
			float num3 = Vector2.Distance(pointsInSegment[0], pointsInSegment[1]) + Vector2.Distance(pointsInSegment[1], pointsInSegment[2]) + Vector2.Distance(pointsInSegment[2], pointsInSegment[3]);
			int num4 = Mathf.CeilToInt((Vector2.Distance(pointsInSegment[0], pointsInSegment[3]) + num3 / 2f) * resolution * 10f);
			float num5 = 0f;
			while (num5 <= 1f)
			{
				num5 += 1f / (float)num4;
				Vector2 vector2 = Bezier.EvaluateCubic(pointsInSegment[0], pointsInSegment[1], pointsInSegment[2], pointsInSegment[3], num5);
				num2 += Vector2.Distance(vector, vector2);
				while (num2 >= spacing)
				{
					float num6 = num2 - spacing;
					Vector2 vector3 = vector2 + (vector - vector2).normalized * num6;
					list.Add(vector3);
					num2 = num6;
					vector = vector3;
				}
				vector = vector2;
			}
		}
		return list.ToArray();
	}

	private static Vector2[] GetPointsInSegment(List<Vector2> points, int i)
	{
		return new Vector2[4]
		{
			points[i * 3],
			points[i * 3 + 1],
			points[i * 3 + 2],
			points[LoopIndex(points, i * 3 + 3)]
		};
	}

	private static int LoopIndex(List<Vector2> points, int i)
	{
		return (i + points.Count) % points.Count;
	}
}
