using Verse;

namespace RimWorld;

public class StorytellerComp_DissolutionTriggered : StorytellerComp
{
	private StorytellerCompProperties_DissolutionTriggered Props => (StorytellerCompProperties_DissolutionTriggered)props;

	public override void Notify_DissolutionEvent(Thing thing)
	{
		if (thing.def == Props.thing)
		{
			IncidentParms incidentParms = new IncidentParms
			{
				target = thing.MapHeld,
				spawnCenter = thing.Position
			};
			incidentParms.points = StorytellerUtility.DefaultThreatPointsNow(incidentParms.target);
			if (Rand.Chance(IncidentChanceFinal(Props.incident, incidentParms.target)) && Props.incident.Worker.CanFireNow(incidentParms))
			{
				Find.Storyteller.incidentQueue.Add(Props.incident, Find.TickManager.TicksGame + Props.delayTicks, incidentParms);
			}
		}
	}
}
