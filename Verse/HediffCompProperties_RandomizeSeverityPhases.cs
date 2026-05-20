using System.Collections.Generic;

namespace Verse
{
	public class HediffCompProperties_RandomizeSeverityPhases : HediffCompProperties_Randomizer
	{
		public class Phase
		{
			public HediffCompProperties comp;

			[MustTranslate]
			public string labelPrefix;

			[MustTranslate]
			public string descriptionExtra;

			public float severityPerDay;
		}

		public List<Phase> phases;

		[MustTranslate]
		public string notifyMessage;

		public HediffCompProperties_RandomizeSeverityPhases()
		{
			compClass = typeof(HediffComp_RandomizeSeverityPhases);
		}
	}
}
