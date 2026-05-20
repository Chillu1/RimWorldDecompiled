using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Precept_Blind_Social : ThoughtWorker_Precept_Social
	{
		protected override ThoughtState ShouldHaveThought(Pawn p, Pawn otherPawn)
		{
			return PawnUtility.IsBiologicallyBlind(otherPawn);
		}
	}
}
