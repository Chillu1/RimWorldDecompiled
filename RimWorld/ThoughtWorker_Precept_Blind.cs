using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Precept_Blind : ThoughtWorker_Precept
	{
		protected override ThoughtState ShouldHaveThought(Pawn p)
		{
			return PawnUtility.IsBiologicallyBlind(p);
		}
	}
}
