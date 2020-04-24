using Verse;

namespace RimWorld
{
	public class ThoughtWorker_PsychologicallyNude : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			return p.apparel.PsychologicallyNude;
		}
	}
}
