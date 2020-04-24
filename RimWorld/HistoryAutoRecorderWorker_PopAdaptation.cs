using Verse;

namespace RimWorld
{
	public class HistoryAutoRecorderWorker_PopAdaptation : HistoryAutoRecorderWorker
	{
		public override float PullRecord()
		{
			return Find.StoryWatcher.watcherPopAdaptation.AdaptDays;
		}
	}
}
