using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PawnColumnWorker_Overseer : PawnColumnWorker_Label
	{
		protected override TextAnchor LabelAlignment => TextAnchor.MiddleCenter;

		public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
		{
			Pawn overseer = pawn.GetOverseer();
			if (overseer != null)
			{
				base.DoCell(rect, overseer, table);
			}
		}

		public override bool CanGroupWith(Pawn pawn, Pawn other)
		{
			Pawn overseer = pawn.GetOverseer();
			if (overseer != null)
			{
				return other.GetOverseer() == overseer;
			}
			return false;
		}
	}
}
