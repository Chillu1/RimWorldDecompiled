using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Precept_HalfBlind : ThoughtWorker_Precept
	{
		protected override ThoughtState ShouldHaveThought(Pawn p)
		{
			return IsHalfBlind(p);
		}

		public static bool IsHalfBlind(Pawn p)
		{
			if (PawnUtility.IsBiologicallyOrArtificiallyBlind(p))
			{
				return false;
			}
			return HealthUtility.IsMissingSightBodyPart(p);
		}
	}
}
