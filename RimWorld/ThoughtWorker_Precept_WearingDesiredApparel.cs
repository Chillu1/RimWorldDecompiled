using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Precept_WearingDesiredApparel : ThoughtWorker_Precept
	{
		protected override ThoughtState ShouldHaveThought(Pawn p)
		{
			return true;
		}
	}
}
