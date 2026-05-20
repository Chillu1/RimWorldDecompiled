namespace Verse.AI.Group;

public class Trigger_DormancyWakeup : Trigger
{
	public override bool ActivateOn(Lord lord, TriggerSignal signal)
	{
		return signal.type == TriggerSignalType.DormancyWakeup;
	}
}
