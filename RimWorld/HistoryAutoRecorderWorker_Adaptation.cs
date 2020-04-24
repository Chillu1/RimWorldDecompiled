using Verse;

namespace RimWorld
{
	public class HistoryAutoRecorderWorker_Adaptation : HistoryAutoRecorderWorker
	{
		public override float PullRecord()
		{
			return Find.StoryWatcher.watcherAdaptation.AdaptDays;
		}
	}
}
