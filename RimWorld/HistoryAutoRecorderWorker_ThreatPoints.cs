using Verse;

namespace RimWorld
{
	public class HistoryAutoRecorderWorker_ThreatPoints : HistoryAutoRecorderWorker
	{
		public override float PullRecord()
		{
			if (Find.AnyPlayerHomeMap == null)
			{
				return 0f;
			}
			return StorytellerUtility.DefaultThreatPointsNow(Find.AnyPlayerHomeMap) / 10f;
		}
	}
}
