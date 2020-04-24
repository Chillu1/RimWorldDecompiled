using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Sick : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			return p.health.hediffSet.AnyHediffMakesSickThought;
		}
	}
}
