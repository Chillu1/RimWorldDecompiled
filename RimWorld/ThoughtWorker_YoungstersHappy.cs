using Verse;

namespace RimWorld
{
	public class ThoughtWorker_YoungstersHappy : ThoughtWorker_YoungstersMoodBase
	{
		protected override FloatRange MoodRange()
		{
			return ThoughtWorker_MyChildHappy.HappyMoodRange;
		}
	}
}
