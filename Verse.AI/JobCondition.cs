namespace Verse.AI
{
	public enum JobCondition : byte
	{
		None,
		Ongoing,
		Succeeded,
		Incompletable,
		InterruptOptional,
		InterruptForced,
		QueuedNoLongerValid,
		Errored,
		ErroredPather
	}
}
