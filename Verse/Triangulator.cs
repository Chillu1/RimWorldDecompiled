using System.Collections.Generic;
using UnityEngine;

namespace Verse;

public class Triangulator
{
	private readonly List<Vector3> m_points;

	private readonly List<int> indices = new List<int>();

	public Triangulator(List<Vector3> points)
	{
		m_points = points;
	}

	public List<int> Triangulate()
	{
		indices.Clear();
		int count = m_points.Count;
		if (count < 3)
		{
			return indices;
		}
		int[] array = new int[count];
		if (Area() > 0f)
		{
			for (int i = 0; i < count; i++)
			{
				array[i] = i;
			}
		}
		else
		{
			for (int j = 0; j < count; j++)
			{
				array[j] = count - 1 - j;
			}
		}
		int num = count;
		int num2 = 2 * num;
		int num3 = num - 1;
		while (num > 2)
		{
			if (num2-- <= 0)
			{
				return indices;
			}
			int num4 = num3;
			if (num <= num4)
			{
				num4 = 0;
			}
			num3 = num4 + 1;
			if (num <= num3)
			{
				num3 = 0;
			}
			int num5 = num3 + 1;
			if (num <= num5)
			{
				num5 = 0;
			}
			if (Snip(num4, num3, num5, num, array))
			{
				int item = array[num4];
				int item2 = array[num3];
				int item3 = array[num5];
				indices.Add(item);
				indices.Add(item2);
				indices.Add(item3);
				int num6 = num3;
				for (int k = num3 + 1; k < num; k++)
				{
					array[num6] = array[k];
					num6++;
				}
				num--;
				num2 = 2 * num;
			}
		}
		indices.Reverse();
		return indices;
	}

	private float Area()
	{
		int count = m_points.Count;
		float num = 0f;
		int index = count - 1;
		int num2 = 0;
		while (num2 < count)
		{
			Vector3 vector = m_points[index];
			Vector3 vector2 = m_points[num2];
			num += vector.x * vector2.z - vector2.x * vector.z;
			index = num2++;
		}
		return num * 0.5f;
	}

	private bool Snip(int u, int v, int w, int n, int[] V)
	{
		Vector3 a = m_points[V[u]];
		Vector3 b = m_points[V[v]];
		Vector3 c = m_points[V[w]];
		if (Mathf.Epsilon > (b.x - a.x) * (c.z - a.z) - (b.z - a.z) * (c.x - a.x))
		{
			return false;
		}
		for (int i = 0; i < n; i++)
		{
			if (i != u && i != v && i != w)
			{
				Vector3 p = m_points[V[i]];
				if (InsideTriangle(a, b, c, p))
				{
					return false;
				}
			}
		}
		return true;
	}

	private static bool InsideTriangle(Vector3 A, Vector3 B, Vector3 C, Vector3 P)
	{
		float num = C.x - B.x;
		float num2 = C.z - B.z;
		float num3 = A.x - C.x;
		float num4 = A.z - C.z;
		float num5 = B.x - A.x;
		float num6 = B.z - A.z;
		float num7 = P.x - A.x;
		float num8 = P.z - A.z;
		float num9 = P.x - B.x;
		float num10 = P.z - B.z;
		float num11 = P.x - C.x;
		float num12 = P.z - C.z;
		float num13 = num * num10 - num2 * num9;
		float num14 = num5 * num8 - num6 * num7;
		float num15 = num3 * num12 - num4 * num11;
		if (num13 >= 0f && num15 >= 0f)
		{
			return num14 >= 0f;
		}
		return false;
	}
}
