namespace Verse.AI.Group
{
	public class Trigger_Memo : Trigger
	{
		private string memo;

		public Trigger_Memo(string memo)
		{
			this.memo = memo;
		}

		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			if (signal.type == TriggerSignalType.Memo)
			{
				return signal.memo == memo;
			}
			return false;
		}
	}
}
