using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PawnColumnWorker_RemainingSpace : PawnColumnWorker
	{
		public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
		{
		}

		public override int GetMinWidth(PawnTable table)
		{
			return 0;
		}

		public override int GetMaxWidth(PawnTable table)
		{
			return 1000000;
		}

		public override int GetOptimalWidth(PawnTable table)
		{
			return GetMaxWidth(table);
		}

		public override int GetMinCellHeight(Pawn pawn)
		{
			return 0;
		}
	}
}
