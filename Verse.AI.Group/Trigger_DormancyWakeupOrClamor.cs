namespace Verse.AI.Group;

public class Trigger_DormancyWakeupOrClamor : Trigger
{
	public override bool ActivateOn(Lord lord, TriggerSignal signal)
	{
		if (signal.type != TriggerSignalType.DormancyWakeup)
		{
			return signal.type == TriggerSignalType.Clamor;
		}
		return true;
	}
}
