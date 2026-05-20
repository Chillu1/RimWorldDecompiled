using Verse;

namespace RimWorld
{
	public class ThoughtWorker_MyChildHappy : ThoughtWorker_RelatedChildMoodBase
	{
		public static readonly FloatRange HappyMoodRange = new FloatRange(0.6f, float.PositiveInfinity);

		protected override FloatRange MoodRange()
		{
			return HappyMoodRange;
		}

		protected override bool Active(Pawn parent)
		{
			return ChildrenWithMoodCount(parent) == 1;
		}

		protected override string SingleChildLabel()
		{
			return "MyChildIsHappy";
		}
	}
}
