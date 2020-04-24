using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class StorytellerComp_FactionInteraction : StorytellerComp
	{
		private StorytellerCompProperties_FactionInteraction Props => (StorytellerCompProperties_FactionInteraction)props;

		public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
		{
			if (Props.minDanger != 0)
			{
				Map map = target as Map;
				if (map == null || (int)map.dangerWatcher.DangerRating < (int)Props.minDanger)
				{
					yield break;
				}
			}
			float num = StorytellerUtility.AllyIncidentFraction(Props.fullAlliesOnly);
			if (num <= 0f)
			{
				yield break;
			}
			int incCount = IncidentCycleUtility.IncidentCountThisInterval(target, Find.Storyteller.storytellerComps.IndexOf(this), Props.minDaysPassed, 60f, 0f, Props.minSpacingDays, Props.baseIncidentsPerYear, Props.baseIncidentsPerYear, num);
			for (int i = 0; i < incCount; i++)
			{
				IncidentParms parms = GenerateParms(Props.incident.category, target);
				if (Props.incident.Worker.CanFireNow(parms))
				{
					yield return new FiringIncident(Props.incident, this, parms);
				}
			}
		}

		public override string ToString()
		{
			return base.ToString() + " (" + Props.incident.defName + ")";
		}
	}
}
