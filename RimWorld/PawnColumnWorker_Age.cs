using Verse;

namespace RimWorld
{
	public class PawnColumnWorker_Age : PawnColumnWorker_Text
	{
		protected override GameFont DefaultHeaderFont => GameFont.Tiny;

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
