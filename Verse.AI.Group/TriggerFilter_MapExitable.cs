namespace Verse.AI.Group;

public class TriggerFilter_MapExitable : TriggerFilter
{
	public override bool AllowActivation(Lord lord, TriggerSignal signal)
	{
		return lord.Map.CanEverExit;
	}
}
