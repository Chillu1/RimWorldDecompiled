using Verse;

namespace RimWorld
{
	public class Building_Skullspike : Building, IObservedThoughtGiver
	{
		public Thought_Memory GiveObservedThought(Pawn observer)
		{
			if ((observer.Ideo != null && observer.Ideo.IdeoApprovesOfSlavery()) || !observer.IsSlave)
			{
				return null;
			}
			Thought_MemoryObservation obj = (Thought_MemoryObservation)ThoughtMaker.MakeThought(ThoughtDefOf.ObservedSkullspike);
			obj.Target = this;
			return obj;
		}

		public HistoryEventDef GiveObservedHistoryEvent(Pawn observer)
		{
			return null;
		}
	}
}
