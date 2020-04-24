using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PawnColumnWorker_Gap : PawnColumnWorker
	{
		protected virtual int Width => def.gap;

		public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
		{
		}

		public override int GetMinWidth(PawnTable table)
		{
			return Mathf.Max(base.GetMinWidth(table), Width);
		}

		public override int GetMaxWidth(PawnTable table)
		{
			return Mathf.Min(base.GetMaxWidth(table), Width);
		}

		public override int GetMinCellHeight(Pawn pawn)
		{
			return 0;
		}
	}
}
