using Verse;

namespace RimWorld
{
	public class ThoughtWorker_MyChildrenHappy : ThoughtWorker_MyChildHappy
	{
		protected override bool Active(Pawn parent)
		{
			return ChildrenWithMoodCount(parent) > 1;
		}
	}
}
