using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class StorytellerCompProperties_RandomMain : StorytellerCompProperties
	{
		public float mtbDays;

		public List<IncidentCategoryEntry> categoryWeights = new List<IncidentCategoryEntry>();

		public float maxThreatBigIntervalDays = 99999f;

		public FloatRange randomPointsFactorRange = new FloatRange(0.5f, 1.5f);

		public bool skipThreatBigIfRaidBeacon;

		public StorytellerCompProperties_RandomMain()
		{
			compClass = typeof(StorytellerComp_RandomMain);
		}
	}
}
