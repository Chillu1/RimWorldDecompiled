using System;

namespace Verse.AI.Group
{
	public class Trigger_Custom : Trigger
	{
		private Func<TriggerSignal, bool> condition;

		public Trigger_Custom(Func<TriggerSignal, bool> condition)
		{
			this.condition = condition;
		}

		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			return condition(signal);
		}
	}
}
