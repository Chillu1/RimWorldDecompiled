using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Precept_HasProsthetic : ThoughtWorker_Precept
	{
		protected override ThoughtState ShouldHaveThought(Pawn p)
		{
			return HasProsthetic(p);
		}

		public static bool HasProsthetic(Pawn p)
		{
			return ThoughtWorker_Precept_HasProsthetic_Count.ProstheticsCount(p) > 0;
		}
	}
}
