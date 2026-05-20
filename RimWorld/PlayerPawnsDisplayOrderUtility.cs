using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public static class PlayerPawnsDisplayOrderUtility
{
	private static Func<Pawn, int> displayOrderGetter = (Pawn x) => (x.playerSettings == null) ? 999999 : x.playerSettings.displayOrder;

	public static void Sort(List<Pawn> pawns)
	{
		pawns.SortBy(displayOrderGetter);
	}

	public static IEnumerable<Pawn> InOrder(IEnumerable<Pawn> pawns)
	{
		return pawns.OrderBy(displayOrderGetter);
	}
}
