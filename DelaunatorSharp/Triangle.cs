using System.Collections.Generic;
using UnityEngine;

namespace DelaunatorSharp;

public struct Triangle : ITriangle
{
	public int Index { get; set; }

	public IEnumerable<Vector2> Points { get; set; }

	public Triangle(int t, IEnumerable<Vector2> points)
	{
		Points = points;
		Index = t;
	}
}
