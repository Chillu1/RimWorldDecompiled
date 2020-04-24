using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class PawnColumnWorker_Info : PawnColumnWorker
	{
		public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
		{
			Widgets.InfoCardButtonCentered(rect, pawn);
		}

		public override int GetMinWidth(PawnTable table)
		{
			return 24;
		}

		public override int GetMaxWidth(PawnTable table)
		{
			return 24;
		}
	}
}
