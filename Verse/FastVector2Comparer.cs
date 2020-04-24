using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class FastVector2Comparer : IEqualityComparer<Vector2>
	{
		public static readonly FastVector2Comparer Instance = new FastVector2Comparer();

		public bool Equals(Vector2 x, Vector2 y)
		{
			return x == y;
		}

		public int GetHashCode(Vector2 obj)
		{
			return obj.GetHashCode();
		}
	}
}
