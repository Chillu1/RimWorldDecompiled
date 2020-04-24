namespace Verse.AI.Group
{
	public class Trigger_OnClamor : Trigger
	{
		private ClamorDef clamorType;

		public Trigger_OnClamor(ClamorDef clamorType)
		{
			this.clamorType = clamorType;
		}

		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			if (signal.type == TriggerSignalType.Clamor)
			{
				return signal.clamorType == clamorType;
			}
			return false;
		}
	}
}
