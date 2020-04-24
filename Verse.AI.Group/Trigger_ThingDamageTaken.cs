namespace Verse.AI.Group
{
	public class Trigger_ThingDamageTaken : Trigger
	{
		private Thing thing;

		private float damageFraction = 0.5f;

		public Trigger_ThingDamageTaken(Thing thing, float damageFraction)
		{
			this.thing = thing;
			this.damageFraction = damageFraction;
		}

		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			if (signal.type == TriggerSignalType.Tick)
			{
				if (!thing.DestroyedOrNull())
				{
					return (float)thing.HitPoints < (1f - damageFraction) * (float)thing.MaxHitPoints;
				}
				return true;
			}
			return false;
		}
	}
}
