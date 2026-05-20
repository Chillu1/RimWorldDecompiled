using Verse;

namespace RimWorld
{
	public interface IObservedThoughtGiver
	{
		Thought_Memory GiveObservedThought(Pawn observer);

		HistoryEventDef GiveObservedHistoryEvent(Pawn observer);
	}
}
