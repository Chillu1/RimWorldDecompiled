using Verse;

namespace RimWorld
{
	public class ThoughtWorker_PsychologicallyNude : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (ModsConfig.IdeologyActive)
			{
				return false;
			}
			return p.apparel.PsychologicallyNude;
		}
	}
}
