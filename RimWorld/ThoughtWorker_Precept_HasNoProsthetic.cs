using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Precept_HasNoProsthetic : ThoughtWorker_Precept
	{
		protected override ThoughtState ShouldHaveThought(Pawn p)
		{
			return !ThoughtWorker_Precept_HasProsthetic.HasProsthetic(p);
		}
	}
}
