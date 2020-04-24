using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class PawnTable_Wildlife : PawnTable
	{
		protected override IEnumerable<Pawn> PrimarySortFunction(IEnumerable<Pawn> input)
		{
			return from p in input
				orderby p.RaceProps.baseBodySize descending, p.def.label
				select p;
		}

		public PawnTable_Wildlife(PawnTableDef def, Func<IEnumerable<Pawn>> pawnsGetter, int uiWidth, int uiHeight)
			: base(def, pawnsGetter, uiWidth, uiHeight)
		{
		}
	}
}
