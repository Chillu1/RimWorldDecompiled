namespace RimWorld
{
	public class HistoryAutoRecorderWorker_PopIntent : HistoryAutoRecorderWorker
	{
		public override float PullRecord()
		{
			return StorytellerUtilityPopulation.PopulationIntent * 10f;
		}
	}
}
