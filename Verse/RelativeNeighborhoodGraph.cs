using System.Collections.Generic;
using DelaunatorSharp;
using UnityEngine;

namespace Verse;

public class RelativeNeighborhoodGraph
{
	public readonly Dictionary<Vector2, List<Vector2>> connections = new Dictionary<Vector2, List<Vector2>>();

	public RelativeNeighborhoodGraph(Delaunator delaunator)
	{
		Vector2[] points = delaunator.Points;
		foreach (Vector2 key in points)
		{
			connections[key] = new List<Vector2>();
		}
		points = delaunator.Points;
		foreach (Vector2 vector in points)
		{
			Vector2[] points2 = delaunator.Points;
			foreach (Vector2 vector2 in points2)
			{
				if (connections[vector].Contains(vector2) || !TryGetEdge(delaunator, vector, vector2, out var edge))
				{
					continue;
				}
				float weight = GetWeight(edge);
				bool flag = false;
				Vector2[] points3 = delaunator.Points;
				foreach (Vector2 vector3 in points3)
				{
					if (TryGetEdge(delaunator, vector, vector3, out var edge2) && TryGetEdge(delaunator, vector3, vector2, out var edge3))
					{
						float weight2 = GetWeight(edge2);
						float weight3 = GetWeight(edge3);
						if (Mathf.Max(weight2, weight3) < weight)
						{
							flag = true;
							break;
						}
					}
				}
				if (!flag)
				{
					connections[vector].Add(vector2);
					connections[vector2].Add(vector);
				}
			}
		}
	}

	private static float GetWeight(Edge edge)
	{
		return (edge.Q - edge.P).magnitude;
	}

	private static bool TryGetEdge(Delaunator delaunator, Vector2 a, Vector2 b, out Edge edge)
	{
		foreach (IEdge edge2 in delaunator.GetEdges())
		{
			if ((edge2.P == a || edge2.Q == a) && (edge2.P == b || edge2.Q == b))
			{
				edge = (Edge)(object)edge2;
				return true;
			}
		}
		edge = default(Edge);
		return false;
	}
}
