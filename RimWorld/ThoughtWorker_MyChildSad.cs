using Verse;

namespace RimWorld
{
	public class ThoughtWorker_MyChildSad : ThoughtWorker_RelatedChildMoodBase
	{
		public static readonly FloatRange SadMoodRange = new FloatRange(float.NegativeInfinity, 0.35f);

		protected override FloatRange MoodRange()
		{
			return SadMoodRange;
		}

		protected override bool Active(Pawn parent)
		{
			return ChildrenWithMoodCount(parent) == 1;
		}

		protected override string SingleChildLabel()
		{
			return "MyChildIsSad";
		}
	}
}
