using Verse;

namespace RimWorld
{
	public class CompAbilityEffect_RaidEnemy : CompAbilityEffect_WithDest
	{
		public new CompProperties_RaidEnemy Props => (CompProperties_RaidEnemy)props;

		public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
		{
			base.Apply(target, dest);
			IncidentParms incidentParms = new IncidentParms();
			incidentParms.target = parent.pawn.Map;
			incidentParms.points = Props.points;
			incidentParms.faction = Find.FactionManager.FirstFactionOfDef(Props.factionDef);
			incidentParms.raidStrategy = Props.raidStrategyDef;
			incidentParms.raidArrivalMode = Props.pawnArrivalModeDef;
			incidentParms.raidStrategy = Props.raidStrategyDef;
			incidentParms.spawnCenter = target.Cell;
			IncidentDefOf.RaidEnemy.Worker.TryExecute(incidentParms);
		}
	}
}
