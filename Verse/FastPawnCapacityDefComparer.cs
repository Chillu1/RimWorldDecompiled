using System.Collections.Generic;

namespace Verse
{
	public class FastPawnCapacityDefComparer : IEqualityComparer<PawnCapacityDef>
	{
		public static readonly FastPawnCapacityDefComparer Instance = new FastPawnCapacityDefComparer();

		public bool Equals(PawnCapacityDef x, PawnCapacityDef y)
		{
			return x == y;
		}

		public int GetHashCode(PawnCapacityDef obj)
		{
			return obj.GetHashCode();
		}
	}
}
