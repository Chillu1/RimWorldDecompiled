namespace Verse.AI.Group
{
	public class Trigger_ChanceOnSignal : Trigger
	{
		private TriggerSignalType signalType;

		private float chance;

		public Trigger_ChanceOnSignal(TriggerSignalType signalType, float chance)
		{
			this.signalType = signalType;
			this.chance = chance;
		}

		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			if (signal.type == signalType)
			{
				return Rand.Value < chance;
			}
			return false;
		}
	}
}
