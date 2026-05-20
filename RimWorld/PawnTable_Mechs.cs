using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class PawnTable_Mechs : PawnTable
	{
		protected override IEnumerable<Pawn> LabelSortFunction(IEnumerable<Pawn> input)
		{
			return input.OrderBy(Overseer).ThenBy(ControlGroupIndex).ThenBy((Pawn p) => p.KindLabel)
				.ThenBy((Pawn p) => p.Label);
			static int ControlGroupIndex(Pawn pawn)
			{
				return pawn.GetMechControlGroup()?.Index ?? int.MaxValue;
			}
			static int Overseer(Pawn pawn)
			{
				return pawn.GetOverseer()?.thingIDNumber ?? int.MaxValue;
			}
		}

		public PawnTable_Mechs(PawnTableDef def, Func<IEnumerable<Pawn>> pawnsGetter, int uiWidth, int uiHeight)
			: base(def, pawnsGetter, uiWidth, uiHeight)
		{
		}
	}
}
