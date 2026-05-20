using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class PawnTable_PlayerPawnsTimetable : PawnTable_PlayerPawns
{
	public PawnTable_PlayerPawnsTimetable(PawnTableDef def, Func<IEnumerable<Pawn>> pawnsGetter, int uiWidth, int uiHeight)
		: base(def, pawnsGetter, uiWidth, uiHeight)
	{
	}
}
