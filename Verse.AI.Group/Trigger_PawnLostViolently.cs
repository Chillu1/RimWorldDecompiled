using RimWorld;

namespace Verse.AI.Group
{
	public class Trigger_PawnLostViolently : Trigger
	{
		public bool allowRoofCollapse;

		public Faction ignoreDamageFromFaction;

		public Trigger_PawnLostViolently(bool allowRoofCollapse = true)
		{
			this.allowRoofCollapse = allowRoofCollapse;
		}

		public Trigger_PawnLostViolently(bool allowRoofCollapse, Faction ignoreDamageFromFaction)
		{
			this.allowRoofCollapse = allowRoofCollapse;
			this.ignoreDamageFromFaction = ignoreDamageFromFaction;
		}

		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			if (signal.type == TriggerSignalType.PawnLost)
			{
				if (signal.dinfo.Instigator != null && ignoreDamageFromFaction != null && signal.dinfo.Instigator.Faction == ignoreDamageFromFaction)
				{
					return false;
				}
				if (signal.condition == PawnLostCondition.MadePrisoner)
				{
					return true;
				}
				if ((signal.condition == PawnLostCondition.Incapped || signal.condition == PawnLostCondition.Killed) && (signal.dinfo.Category != DamageInfo.SourceCategory.Collapse || allowRoofCollapse))
				{
					return true;
				}
			}
			return false;
		}
	}
}
