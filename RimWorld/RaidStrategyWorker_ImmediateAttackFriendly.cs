namespace RimWorld
{
	public class RaidStrategyWorker_ImmediateAttackFriendly : RaidStrategyWorker_ImmediateAttack
	{
		public override bool CanUseWith(IncidentParms parms, PawnGroupKindDef groupKind)
		{
			if (!base.CanUseWith(parms, groupKind))
			{
				return false;
			}
			if (parms.faction != null)
			{
				return !parms.faction.HostileTo(Faction.OfPlayer);
			}
			return false;
		}
	}
}
