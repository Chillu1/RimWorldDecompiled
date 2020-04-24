namespace Verse.AI.Group
{
	public abstract class TriggerFilter
	{
		public abstract bool AllowActivation(Lord lord, TriggerSignal signal);
	}
}
