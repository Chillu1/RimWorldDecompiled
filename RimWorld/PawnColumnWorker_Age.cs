using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PawnColumnWorker_Age : PawnColumnWorker_Text
	{
		protected override TextAnchor Anchor => TextAnchor.MiddleCenter;

		public override int GetMinWidth(PawnTable table)
		{
			return Mathf.Max(base.GetMinWidth(table), 50);
		}

		public override int Compare(Pawn a, Pawn b)
		{
			return a.ageTracker.AgeBiologicalYears.CompareTo(b.ageTracker.AgeBiologicalYears);
		}

		protected override string GetTextFor(Pawn pawn)
		{
			return pawn.ageTracker.AgeBiologicalYears.ToString();
		}
	}
}
