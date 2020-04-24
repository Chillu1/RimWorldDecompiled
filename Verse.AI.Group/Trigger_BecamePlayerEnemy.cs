using RimWorld;

namespace Verse.AI.Group
{
	public class Trigger_BecamePlayerEnemy : Trigger
	{
		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			if (signal.type == TriggerSignalType.FactionRelationsChanged)
			{
				if (lord.faction != null)
				{
					return lord.faction.HostileTo(Faction.OfPlayer);
				}
				return false;
			}
			return false;
		}
	}
}
