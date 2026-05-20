using System.Collections.Generic;
using RimWorld.QuestGen;
using Verse;

namespace RimWorld
{
	public class StorytellerComp_WorkSite : StorytellerComp
	{
		public StorytellerCompProperties_WorkSite Props => (StorytellerCompProperties_WorkSite)props;

		public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
		{
			if (Find.TickManager.TicksGame < 600000)
			{
				yield break;
			}
			float num = QuestNode_Root_WorkSite.BestAppearanceFrequency();
			if (Props.incident != null && Props.incident.TargetAllowed(target) && !(num <= 0f) && Rand.MTBEventOccurs(Props.baseMtbDays / num, 60000f, 1000f))
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
