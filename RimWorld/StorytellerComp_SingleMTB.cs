using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class StorytellerComp_SingleMTB : StorytellerComp
	{
		private StorytellerCompProperties_SingleMTB Props => (StorytellerCompProperties_SingleMTB)props;

		public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
		{
			if (Props.incident.TargetAllowed(target) && Rand.MTBEventOccurs(Props.mtbDays, 60000f, 1000f))
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
			return base.ToString() + " " + Props.incident;
		}
	}
}
