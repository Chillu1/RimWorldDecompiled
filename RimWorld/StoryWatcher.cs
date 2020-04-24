using Verse;

namespace RimWorld
{
	public sealed class StoryWatcher : IExposable
	{
		public StatsRecord statsRecord = new StatsRecord();

		public StoryWatcher_Adaptation watcherAdaptation = new StoryWatcher_Adaptation();

		public StoryWatcher_PopAdaptation watcherPopAdaptation = new StoryWatcher_PopAdaptation();

		public void StoryWatcherTick()
		{
			watcherAdaptation.AdaptationWatcherTick();
			watcherPopAdaptation.PopAdaptationWatcherTick();
		}

		public void ExposeData()
		{
			Scribe_Deep.Look(ref statsRecord, "statsRecord");
			Scribe_Deep.Look(ref watcherAdaptation, "watcherAdaptation");
			Scribe_Deep.Look(ref watcherPopAdaptation, "watcherPopAdaptation");
			BackCompatibility.PostExposeData(this);
		}
	}
}
