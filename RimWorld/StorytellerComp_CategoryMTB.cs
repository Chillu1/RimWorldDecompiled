using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class StorytellerComp_CategoryMTB : StorytellerComp
	{
		protected StorytellerCompProperties_CategoryMTB Props => (StorytellerCompProperties_CategoryMTB)props;

		public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
		{
			float num = Props.mtbDays;
			if (Props.mtbDaysFactorByDaysPassedCurve != null)
			{
				num *= Props.mtbDaysFactorByDaysPassedCurve.Evaluate(GenDate.DaysPassedFloat);
			}
			if (Rand.MTBEventOccurs(num, 60000f, 1000f))
			{
				IncidentParms parms = GenerateParms(Props.category, target);
				if (UsableIncidentsInCategory(Props.category, parms).TryRandomElementByWeight((IncidentDef incDef) => IncidentChanceFinal(incDef), out var result))
				{
					yield return new FiringIncident(result, this, parms);
				}
			}
		}

		public override string ToString()
		{
			return base.ToString() + " " + Props.category;
		}
	}
}
