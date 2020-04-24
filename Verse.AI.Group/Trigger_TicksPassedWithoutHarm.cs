namespace Verse.AI.Group
{
	public class Trigger_TicksPassedWithoutHarm : Trigger_TicksPassed
	{
		public Trigger_TicksPassedWithoutHarm(int tickLimit)
			: base(tickLimit)
		{
		}

		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			if (Trigger_PawnHarmed.SignalIsHarm(signal))
			{
				base.Data.ticksPassed = 0;
			}
			return base.ActivateOn(lord, signal);
		}
	}
}
