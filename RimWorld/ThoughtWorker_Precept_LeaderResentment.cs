using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Precept_LeaderResentment : ThoughtWorker_Precept
	{
		protected override ThoughtState ShouldHaveThought(Pawn p)
		{
			Pawn pawn = p.Faction?.leader;
			return pawn != null && p.Ideo != pawn.Ideo;
		}
	}
}
