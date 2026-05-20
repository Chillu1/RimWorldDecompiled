using Verse;

namespace RimWorld
{
	public class ThoughtWorker_MyChildrenSad : ThoughtWorker_MyChildSad
	{
		protected override bool Active(Pawn parent)
		{
			return ChildrenWithMoodCount(parent) > 1;
		}
	}
}
