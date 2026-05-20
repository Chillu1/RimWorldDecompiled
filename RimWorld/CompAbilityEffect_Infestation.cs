using Verse;

namespace RimWorld
{
	public class CompAbilityEffect_Infestation : CompAbilityEffect_WithDest
	{
		public new CompProperties_Infestation Props => (CompProperties_Infestation)props;

		public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
		{
			base.Apply(target, dest);
			IncidentParms incidentParms = new IncidentParms();
			incidentParms.target = parent.pawn.Map;
			incidentParms.points = Props.points;
			incidentParms.infestationLocOverride = target.Cell;
			incidentParms.forced = true;
			IncidentDefOf.Infestation.Worker.TryExecute(incidentParms);
		}
	}
}
