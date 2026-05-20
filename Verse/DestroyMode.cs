namespace Verse
{
	public enum DestroyMode : byte
	{
		Vanish,
		WillReplace,
		KillFinalize,
		KillFinalizeLeavingsOnly,
		Deconstruct,
		FailConstruction,
		Cancel,
		Refund,
		QuestLogic
	}
}
