using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Precept_Pain : ThoughtWorker_Precept
	{
		protected override ThoughtState ShouldHaveThought(Pawn p)
		{
			return ThoughtWorker_Pain.CurrentThoughtState(p);
		}
	}
}
