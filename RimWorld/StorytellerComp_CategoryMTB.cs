using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class StorytellerComp_CategoryMTB : StorytellerComp
{
	protected StorytellerCompProperties_CategoryMTB Props => (StorytellerCompProperties_CategoryMTB)props;

	public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
	{
		float num = Props.mtbDays;
		if (Props.mtbDaysFactorByDaysPassedCurve != null)
		{
			num *= Props.mtbDaysFactorByDaysPassedCurve.Evaluate(GenDate.DaysPassedSinceSettleFloat);
		}
		if (Rand.MTBEventOccurs(num, 60000f, 1000f))
		{
			IncidentParms parms = GenerateParms(Props.category, target);
			if (TrySelectRandomIncident(UsableIncidentsInCategory(Props.category, parms), out var foundDef, target))
			{
				yield return new FiringIncident(foundDef, this, parms);
			}
		}
	}

	public override string ToString()
	{
		return $"{base.ToString()} {Props.category}";
	}
}
