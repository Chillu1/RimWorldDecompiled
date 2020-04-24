using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class QuestPart_Filter_AnyPawnAlive : QuestPart_Filter
	{
		public List<Pawn> pawns;

		protected override bool Pass(SignalArgs args)
		{
			if (pawns.NullOrEmpty())
			{
				return false;
			}
			foreach (Pawn pawn in pawns)
			{
				if (!pawn.Destroyed)
				{
					return true;
				}
			}
			return false;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				pawns.RemoveAll((Pawn x) => x == null);
			}
		}
	}
}
