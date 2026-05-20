using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Precept_ArtificialBlind : ThoughtWorker_Precept
	{
		protected override ThoughtState ShouldHaveThought(Pawn p)
		{
			return PawnUtility.IsArtificiallyBlind(p);
		}
	}
}
