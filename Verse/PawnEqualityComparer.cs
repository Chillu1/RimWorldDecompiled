using System.Collections.Generic;

namespace Verse;

public class PawnEqualityComparer : IEqualityComparer<Pawn>
{
	public bool Equals(Pawn x, Pawn y)
	{
		return x.Equals(y);
	}

	public int GetHashCode(Pawn obj)
	{
		return obj.GetHashCode();
	}
}
