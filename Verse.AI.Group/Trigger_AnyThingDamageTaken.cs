using System.Collections.Generic;

namespace Verse.AI.Group
{
	public class Trigger_AnyThingDamageTaken : Trigger
	{
		private List<Thing> things;

		private float damageFraction = 0.5f;

		public Trigger_AnyThingDamageTaken(List<Thing> things, float damageFraction)
		{
			this.things = things;
			this.damageFraction = damageFraction;
		}

		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			if (signal.type == TriggerSignalType.Tick)
			{
				foreach (Thing thing in things)
				{
					if (thing.DestroyedOrNull() || (float)thing.HitPoints < (1f - damageFraction) * (float)thing.MaxHitPoints)
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
