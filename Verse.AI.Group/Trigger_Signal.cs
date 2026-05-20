namespace Verse.AI.Group;

public class Trigger_Signal : Trigger
{
	private string signal;

	public Trigger_Signal(string signal)
	{
		this.signal = signal;
	}

	public override bool ActivateOn(Lord lord, TriggerSignal signal)
	{
		if (signal.type == TriggerSignalType.Signal)
		{
			return signal.signal.tag == this.signal;
		}
		return false;
	}
}
