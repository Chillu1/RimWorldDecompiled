using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class StorytellerComp_GauranlenPodSpawn : StorytellerComp
{
	protected StorytellerCompProperties_GauranlenPodSpawn Props => (StorytellerCompProperties_GauranlenPodSpawn)props;

	public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
	{
		if (!ModsConfig.IdeologyActive || Props.daysBetweenPodSpawns == 0f || Faction.OfPlayer.ideos == null || (float)GenDate.DaysPassed < Props.minDaysPassed)
		{
			yield break;
		}
		int num = 1;
		if (Props.countdownFactorAnyConnectors > 1)
		{
			foreach (Ideo allIdeo in Faction.OfPlayer.ideos.AllIdeos)
			{
				if (allIdeo.HasMeme(MemeDefOf.TreeConnection))
				{
					num = Props.countdownFactorAnyConnectors;
					break;
				}
			}
		}
		Find.IdeoManager.ticksToNextGauranlenSpawn -= 1000 * num;
		if (Find.IdeoManager.ticksToNextGauranlenSpawn <= 0)
		{
			Find.IdeoManager.ticksToNextGauranlenSpawn = (int)(Props.daysBetweenPodSpawns * 60000f);
			yield return new FiringIncident(Props.incident, this, GenerateParms(Props.incident.category, target));
		}
	}
}
