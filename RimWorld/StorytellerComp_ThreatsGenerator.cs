using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class StorytellerComp_ThreatsGenerator : StorytellerComp
	{
		protected StorytellerCompProperties_ThreatsGenerator Props => (StorytellerCompProperties_ThreatsGenerator)props;

		public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
		{
			foreach (FiringIncident item in ThreatsGenerator.MakeIntervalIncidents(Props.parms, target, (target as Map)?.generationTick ?? 0))
			{
				item.source = this;
				yield return item;
			}
		}
	}
}
