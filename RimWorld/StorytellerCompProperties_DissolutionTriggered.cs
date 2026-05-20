using Verse;

namespace RimWorld
{
	public class StorytellerCompProperties_DissolutionTriggered : StorytellerCompProperties
	{
		public IncidentDef incident;

		public ThingDef thing;

		public int delayTicks;

		public StorytellerCompProperties_DissolutionTriggered()
		{
			compClass = typeof(StorytellerComp_DissolutionTriggered);
		}
	}
}
