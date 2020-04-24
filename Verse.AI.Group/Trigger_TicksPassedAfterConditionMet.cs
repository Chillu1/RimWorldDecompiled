using System;

namespace Verse.AI.Group
{
	public class Trigger_TicksPassedAfterConditionMet : Trigger_TicksPassed
	{
		private Func<bool> condition;

		private int checkEveryTicks;

		protected new TriggerData_TicksPassedAfterConditionMet Data => (TriggerData_TicksPassedAfterConditionMet)data;

		public Trigger_TicksPassedAfterConditionMet(int tickLimit, Func<bool> condition, int checkEveryTicks = 1)
			: base(tickLimit)
		{
			this.condition = condition;
			this.checkEveryTicks = checkEveryTicks;
			data = new TriggerData_TicksPassedAfterConditionMet();
		}

		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			if (!Data.conditionMet && signal.type == TriggerSignalType.Tick && Find.TickManager.TicksGame % checkEveryTicks == 0)
			{
				Data.conditionMet = condition();
			}
			if (Data.conditionMet)
			{
				return base.ActivateOn(lord, signal);
			}
			return false;
		}
	}
}
