using System.Collections.Generic;
using UnityEngine;

namespace DelaunatorSharp;

public interface ITriangle
{
	IEnumerable<Vector2> Points { get; }

	int Index { get; }
}
