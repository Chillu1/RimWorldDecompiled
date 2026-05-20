using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Precept_Darklight : ThoughtWorker_Precept
	{
		protected override ThoughtState ShouldHaveThought(Pawn p)
		{
			if (!p.Awake() || PawnUtility.IsBiologicallyOrArtificiallyBlind(p))
			{
				return false;
			}
			return DarklightUtility.IsDarklightAt(p.Position, p.Map);
		}
	}
}
