namespace Verse.AI.Group
{
	public class Trigger_FractionPawnsLost : Trigger
	{
		private float fraction = 0.5f;

		public Trigger_FractionPawnsLost(float fraction)
		{
			this.fraction = fraction;
		}

		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			if (signal.type == TriggerSignalType.PawnLost)
			{
				return (float)lord.numPawnsLostViolently >= (float)lord.numPawnsEverGained * fraction;
			}
			return false;
		}
	}
}
