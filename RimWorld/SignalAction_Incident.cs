using Verse;

namespace RimWorld
{
	public class SignalAction_Incident : SignalAction
	{
		public IncidentDef incident;

		public IncidentParms incidentParms;

		protected override void DoAction(SignalArgs args)
		{
			if (incident.Worker.CanFireNow(incidentParms))
			{
				incident.Worker.TryExecute(incidentParms);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref incident, "incident");
			Scribe_Deep.Look(ref incidentParms, "incidentParms");
		}
	}
}
