using Verse;

namespace RimWorld
{
	public class CompAbilityEffect_RandomAlliedMilitaryAid : CompAbilityEffect_WithDest
	{
		public new CompProperties_RandomAlliedMilitaryAid Props => (CompProperties_RandomAlliedMilitaryAid)props;

		public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
		{
			base.Apply(target, dest);
			IncidentParms incidentParms = new IncidentParms();
			incidentParms.target = parent.pawn.Map;
			incidentParms.faction = GetRandomAlliedFaction();
			incidentParms.raidArrivalModeForQuickMilitaryAid = true;
			incidentParms.points = Props.points;
			incidentParms.spawnCenter = target.Cell;
			if (!IncidentDefOf.RaidFriendly.Worker.TryExecute(incidentParms))
			{
				Log.Error("Failed to request military aid");
			}
		}

		private Faction GetRandomAlliedFaction()
		{
			return Find.FactionManager.RandomAlliedFaction(allowHidden: true);
		}

		public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
		{
			if (GetRandomAlliedFaction() == null)
			{
				return false;
			}
			return base.CanApplyOn(target, dest);
		}
	}
}
