using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class PawnColumnWorker_MedicalCare : PawnColumnWorker
	{
		public override void DoHeader(Rect rect, PawnTable table)
		{
			MouseoverSounds.DoRegion(rect);
			base.DoHeader(rect, table);
		}

		public override int GetMinWidth(PawnTable table)
		{
			return Mathf.Max(base.GetMinWidth(table), 28);
		}

		public override int GetMaxWidth(PawnTable table)
		{
			return Mathf.Min(base.GetMaxWidth(table), GetMinWidth(table));
		}

		public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
		{
			MedicalCareUtility.MedicalCareSelectButton(rect, pawn);
		}

		public override int Compare(Pawn a, Pawn b)
		{
			return a.playerSettings.medCare.CompareTo(b.playerSettings.medCare);
		}
	}
}
