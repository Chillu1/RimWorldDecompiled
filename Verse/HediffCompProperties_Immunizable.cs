using System.Collections.Generic;

namespace Verse
{
	public class HediffCompProperties_Immunizable : HediffCompProperties
	{
		public float immunityPerDayNotSick;

		public float immunityPerDaySick;

		public float severityPerDayNotImmune;

		public float severityPerDayImmune;

		public FloatRange severityPerDayNotImmuneRandomFactor = new FloatRange(1f, 1f);

		public List<HediffDefFactor> severityFactorsFromHediffs = new List<HediffDefFactor>();

		public bool hidden;

		public HediffCompProperties_Immunizable()
		{
			compClass = typeof(HediffComp_Immunizable);
		}
	}
}
