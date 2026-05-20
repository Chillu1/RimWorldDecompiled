using Verse;

namespace RimWorld
{
	public class ThoughtWorker_YoungstersSad : ThoughtWorker_YoungstersMoodBase
	{
		protected override FloatRange MoodRange()
		{
			return ThoughtWorker_MyChildSad.SadMoodRange;
		}
	}
}
