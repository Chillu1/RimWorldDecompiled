using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class StorytellerComp_OnOffCycle : StorytellerComp
{
	protected StorytellerCompProperties_OnOffCycle Props => (StorytellerCompProperties_OnOffCycle)props;

	public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
	{
		float num = 1f;
		if (Props.acceptFractionByDaysPassedCurve != null)
		{
			num *= Props.acceptFractionByDaysPassedCurve.Evaluate(GenDate.DaysPassedSinceSettleFloat);
		}
		if (Props.acceptPercentFactorPerThreatPointsCurve != null)
		{
			num *= Props.acceptPercentFactorPerThreatPointsCurve.Evaluate(StorytellerUtility.DefaultThreatPointsNow(target));
		}
		if (Props.acceptPercentFactorPerProgressScoreCurve != null)
		{
			num *= Props.acceptPercentFactorPerProgressScoreCurve.Evaluate(StorytellerUtility.GetProgressScore(target));
		}
		float onDays = Props.onDays;
		float offDays = Props.offDays;
		if ((Props.onDaysNoTreeConnectors != 0f || Props.offDaysNoTreeConnectors != 0f) && Faction.OfPlayer.ideos != null)
		{
			bool flag = false;
			foreach (Ideo allIdeo in Faction.OfPlayer.ideos.AllIdeos)
			{
				if (allIdeo.HasMeme(MemeDefOf.TreeConnection))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				if (Props.onDaysNoTreeConnectors != 0f)
				{
					onDays = Props.onDaysNoTreeConnectors;
				}
				if (Props.offDaysNoTreeConnectors != 0f)
				{
					offDays = Props.offDaysNoTreeConnectors;
				}
			}
		}
		int incCount = IncidentCycleUtility.IncidentCountThisInterval(target, Find.Storyteller.storytellerComps.IndexOf(this), Props.minDaysPassed, onDays, offDays, Props.minSpacingDays, Props.numIncidentsRange.min, Props.numIncidentsRange.max, num);
		for (int i = 0; i < incCount; i++)
		{
			FiringIncident firingIncident = GenerateIncident(target);
			if (firingIncident != null)
			{
				yield return firingIncident;
			}
		}
	}

	private FiringIncident GenerateIncident(IIncidentTarget target)
	{
		if (Props.IncidentCategory == null)
		{
			return null;
		}
		IncidentParms parms = GenerateParms(Props.IncidentCategory, target);
		IncidentDef result;
		if ((float)GenDate.DaysPassedSinceSettle < Props.forceRaidEnemyBeforeDaysPassed)
		{
			if (!IncidentDefOf.RaidEnemy.Worker.CanFireNow(parms))
			{
				return null;
			}
			result = IncidentDefOf.RaidEnemy;
		}
		else if (Props.incident != null)
		{
			if (!Props.incident.Worker.CanFireNow(parms))
			{
				return null;
			}
			result = Props.incident;
		}
		else if (!UsableIncidentsInCategory(Props.IncidentCategory, parms).TryRandomElementByWeight((IncidentDef def) => IncidentChanceFinal(def, target), out result))
		{
			return null;
		}
		return new FiringIncident(result, this, parms);
	}

	public override string ToString()
	{
		if (Props.incident == null && Props.IncidentCategory == null)
		{
			return "";
		}
		return base.ToString() + " (" + ((Props.incident != null) ? Props.incident.defName : Props.IncidentCategory.defName) + ")";
	}
}
