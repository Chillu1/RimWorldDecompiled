namespace Verse.AI.Group
{
	public class Trigger_TicksPassedAndNoRecentHarm : Trigger_TicksPassed
	{
		private const int MinTicksSinceDamage = 300;

		public Trigger_TicksPassedAndNoRecentHarm(int tickLimit)
			: base(tickLimit)
		{
		}

		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			if (base.ActivateOn(lord, signal) && Find.TickManager.TicksGame - lord.lastPawnHarmTick >= 300)
			{
				return true;
			}
			return false;
		}
	}
}
