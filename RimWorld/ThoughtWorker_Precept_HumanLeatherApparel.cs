using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Precept_HumanLeatherApparel : ThoughtWorker_Precept
	{
		protected override ThoughtState ShouldHaveThought(Pawn p)
		{
			return ThoughtWorker_HumanLeatherApparel.CurrentThoughtState(p);
		}
	}
}
