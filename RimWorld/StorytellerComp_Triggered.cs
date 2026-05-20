using Verse;

namespace RimWorld;

public class StorytellerComp_Triggered : StorytellerComp
{
	private StorytellerCompProperties_Triggered Props => (StorytellerCompProperties_Triggered)props;

	public override void Notify_PawnEvent(Pawn p, AdaptationEvent ev, DamageInfo? dinfo = null)
	{
		if (!p.RaceProps.Humanlike || !p.IsColonist || (ev != AdaptationEvent.Died && ev != AdaptationEvent.Kidnapped && ev != AdaptationEvent.LostBecauseMapClosed && ev != AdaptationEvent.Downed))
		{
			return;
		}
		foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_OfPlayerFaction)
		{
			if (item.RaceProps.Humanlike && !item.IsPrisoner && ((item != p && !item.Downed) || (ModsConfig.BiotechActive && item.Deathresting && !SanguophageUtility.ShouldBeDeathrestingOrInComaInsteadOfDead(item))))
			{
				return;
			}
		}
		if (ModsConfig.AnomalyActive && (DeathRefusalUtility.HasPlayerControlledDeathRefusal(p) || DeathRefusalUtility.PlayerHasCorpseWithDeathRefusal()))
		{
			return;
		}
		Map anyPlayerHomeMap = Find.AnyPlayerHomeMap;
		if (anyPlayerHomeMap != null)
		{
			IncidentParms parms = StorytellerUtility.DefaultParmsNow(Props.incident.category, anyPlayerHomeMap);
			if (Props.incident.Worker.CanFireNow(parms))
			{
				QueuedIncident qi = new QueuedIncident(new FiringIncident(Props.incident, this, parms), Find.TickManager.TicksGame + Props.delayTicks);
				Find.Storyteller.incidentQueue.Add(qi);
			}
		}
	}

	public override string ToString()
	{
		return base.ToString() + " " + Props.incident;
	}
}
