using RimWorld;

namespace Verse.AI.Group
{
	public class Trigger_BecameNonHostileToPlayer : Trigger
	{
		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			if (signal.type == TriggerSignalType.FactionRelationsChanged)
			{
				if (signal.previousRelationKind == FactionRelationKind.Hostile && lord.faction != null)
				{
					return !lord.faction.HostileTo(Faction.OfPlayer);
				}
				return false;
			}
			return false;
		}
	}
}
