using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Precept_NonBlind : ThoughtWorker_Precept
	{
		protected override ThoughtState ShouldHaveThought(Pawn p)
		{
			return !PawnUtility.IsBiologicallyOrArtificiallyBlind(p);
		}
	}
}
