using UnityEngine;

namespace DelaunatorSharp;

public struct VoronoiCell : IVoronoiCell
{
	public Vector2[] Points { get; set; }

	public int Index { get; set; }

	public VoronoiCell(int triangleIndex, Vector2[] points)
	{
		Points = points;
		Index = triangleIndex;
	}
}
